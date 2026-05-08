using Microsoft.EntityFrameworkCore;
using StatistiquesHGG.Core.Entities;
using StatistiquesHGG.Infrastructure.Data;

namespace StatistiquesHGG.Business.Services;

/// <summary>
/// Service de gestion des patients RMA en v5
/// - Gère la distinction nouveau vs ancien patient (consultations)
/// - Gère la logique d'intégration auto à la morgue (décès)
/// </summary>
public class PatientRmaService
{
    private readonly AppDbContext _context;

    public PatientRmaService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Enregistre une consultation et comptabilise automatiquement nouveau vs ancien
    /// </summary>
    public async Task<SaisieConsultation> EnregistrerConsultationAsync(
        int serviceId, 
        int nouvellesH, int nouvellesF, int nouvellesE,
        int anciensH, int anciensF, int anciensE,
        DateTime dateSaisie, 
        int saisieParId)
    {
        var consultation = new SaisieConsultation
        {
            ServiceId = serviceId,
            DateSaisie = dateSaisie,
            SaisieParId = saisieParId,
            NouveauxHommes = nouvellesH,
            NouvellesFemmes = nouvellesF,
            NouveauxEnfants = nouvellesE,
            AnciensHommes = anciensH,
            AnciennesFemmes = anciensF,
            AnciensEnfants = anciensE,
            NouveauxPatientsComptabilises = nouvellesH + nouvellesF + nouvellesE,
            Validee = false
        };

        _context.SaisiesConsultation.Add(consultation);
        await _context.SaveChangesAsync();
        return consultation;
    }

    /// <summary>
    /// Enregistre une hospitalisation avec tracking des entrées/sorties
    /// </summary>
    public async Task<SaisieHospitalisation> EnregistrerHospitalisationAsync(
        int serviceId,
        string typeActivite,
        int hommes, int femmes, int enfants,
        int joursHospitalisation,
        int nombreEntrees, int nombreSorties,
        DateTime dateSaisie,
        int saisieParId)
    {
        var hospitalisation = new SaisieHospitalisation
        {
            ServiceId = serviceId,
            TypeActivite = typeActivite,
            DateSaisie = dateSaisie,
            SaisieParId = saisieParId,
            Hommes = hommes,
            Femmes = femmes,
            Enfants = enfants,
            JoursHospitalisation = joursHospitalisation,
            NombreEntrees = nombreEntrees,
            NombreSorties = nombreSorties,
            Validee = false
        };

        _context.SaisiesHospitalisation.Add(hospitalisation);
        await _context.SaveChangesAsync();
        return hospitalisation;
    }

    /// <summary>
    /// v5: Enregistre un décès avec éventuelle auto-intégration à la morgue
    /// Si transfereAMorgue = true, crée auto une entrée morgue et la marque comme CasFromHopital
    /// </summary>
    public async Task<SaisieDeces> EnregistrerDecesEtMorgueAsync(
        int serviceId,
        int hommes, int femmes, int enfants,
        bool transfereAMorgue,
        DateTime dateSaisie,
        int saisieParId)
    {
        var deces = new SaisieDeces
        {
            ServiceId = serviceId,
            DateSaisie = dateSaisie,
            SaisieParId = saisieParId,
            Hommes = hommes,
            Femmes = femmes,
            Enfants = enfants,
            TransfereAMorgueAuto = transfereAMorgue,
            Validee = false
        };

        _context.SaisiesDeces.Add(deces);

        // Auto-transfer à morgue si demandé
        if (transfereAMorgue)
        {
            int totalCas = hommes + femmes + enfants;
            
            var morgue = new SaisieMorgue
            {
                DateSaisie = dateSaisie,
                SaisieParId = saisieParId,
                CasHommes = hommes,
                CasFemmes = femmes,
                CasEnfants = enfants,
                CasFromHopital = totalCas,  // Mark as hospital deaths only
                CasExterieurs = 0,
                Validee = false
            };

            _context.SaisiesMorgue.Add(morgue);
            await _context.SaveChangesAsync();
            
            // Link deces to morgue
            deces.MorgueEntreeId = morgue.Id;
        }

        await _context.SaveChangesAsync();
        return deces;
    }

    /// <summary>
    /// Obtient le total des décès issus du centre (hôpital seulement)
    /// Exclut les corps provenant de l'extérieur
    /// </summary>
    public async Task<int> GetDecesHopitalTotalAsync(DateTime debut, DateTime fin)
    {
        return await _context.SaisiesMorgue
            .Where(m => m.DateSaisie >= debut && m.DateSaisie <= fin && m.Validee)
            .SumAsync(m => m.CasFromHopital);
    }

    /// <summary>
    /// Obtient le total des décès d'origine externe (morgue)
    /// </summary>
    public async Task<int> GetDecesExternesAsync(DateTime debut, DateTime fin)
    {
        return await _context.SaisiesMorgue
            .Where(m => m.DateSaisie >= debut && m.DateSaisie <= fin && m.Validee)
            .SumAsync(m => m.CasExterieurs);
    }

    /// <summary>
    /// Calcule le taux de décès hôpital (excluant corps extérieurs)
    /// </summary>
    public async Task<decimal> CalculerTauxDecesHopitalAsync(DateTime debut, DateTime fin)
    {
        var decesHopital = await GetDecesHopitalTotalAsync(debut, fin);

        var sortiesTotal = await _context.SaisiesConsultation
            .Where(c => c.DateSaisie >= debut && c.DateSaisie <= fin && c.Validee)
            .SumAsync(c => (decimal)(c.TotalGeneral));

        // Add hospitalisations
        sortiesTotal += await _context.SaisiesHospitalisation
            .Where(h => h.DateSaisie >= debut && h.DateSaisie <= fin && h.Validee)
            .SumAsync(h => (decimal)(h.Hommes + h.Femmes + h.Enfants));

        if (sortiesTotal == 0) return 0;

        return (decesHopital / sortiesTotal) * 100;
    }

    /// <summary>
    /// Compte les nouveaux patients (consultation)
    /// </summary>
    public async Task<int> CompterNouveauxPatientsAsync(int serviceId, DateTime debut, DateTime fin)
    {
        return await _context.SaisiesConsultation
            .Where(c => c.ServiceId == serviceId
                     && c.DateSaisie >= debut
                     && c.DateSaisie <= fin
                     && c.Validee)
            .SumAsync(c => c.NouveauxPatientsComptabilises);
    }

    /// <summary>
    /// Détermine si un patient est nouveau basé sur historique
    /// </summary>
    public async Task<bool> IsPatientNewAsync(string patientMatricule)
    {
        if (string.IsNullOrWhiteSpace(patientMatricule))
            return true;
            
        var existing = await _context.Patients
            .FirstOrDefaultAsync(p => p.Matricule == patientMatricule && p.Actif);
            
        return existing == null;
    }
    
    /// <summary>
    /// Crée ou met à jour un patient (Formulaire A - RMA)
    /// </summary>
    public async Task<Patient> CreerOuMettreAJourPatientAsync(
        string matricule,
        string nom,
        string prenoms,
        DateTime dateNaissance,
        string sexe,
        string quartier,
        int? serviceId)
    {
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Matricule == matricule);
            
        if (patient == null)
        {
            // Nouveau patient
            patient = new Patient
            {
                Matricule = matricule,
                Nom = nom,
                Prenoms = prenoms,
                DateNaissance = dateNaissance,
                Sexe = sexe,
                Quartier = quartier,
                ServiceIdActuel = serviceId,
                DateCreation = DateTime.Now,
                Actif = true
            };
            
            _context.Patients.Add(patient);
            
            // Enregistrer le premier mouvement
            var premierMouvement = new PatientMouvement
            {
                Patient = patient,
                TypeMouvement = "Consultation",
                ServiceId = serviceId,
                DateMouvement = DateTime.Now,
                EstNouveauPatient = true
            };
            _context.PatientsMouvements.Add(premierMouvement);
        }
        else
        {
            // Patient existant - mise à jour
            patient.Nom = nom;
            patient.Prenoms = prenoms;
            patient.DateNaissance = dateNaissance;
            patient.Sexe = sexe;
            patient.Quartier = quartier;
            patient.ServiceIdActuel = serviceId;
        }
        
        await _context.SaveChangesAsync();
        return patient;
    }
    
    /// <summary>
    /// Enregistre un mouvement pour un patient existant (Formulaire B - Mouvement & Suivi)
    /// </summary>
    public async Task<PatientMouvement> EnregistrerMouvementAsync(
        int patientId,
        string typeMouvement,
        int? serviceId,
        string motif,
        int? dureeJours = null,
        DateTime? dateSortie = null,
        bool transfereAMorgue = false)
    {
        var mouvement = new PatientMouvement
        {
            PatientId = patientId,
            TypeMouvement = typeMouvement,
            ServiceId = serviceId,
            Motif = motif,
            DureeJours = dureeJours,
            DateSortie = dateSortie,
            TransfereAMorgue = transfereAMorgue,
            DateMouvement = DateTime.Now,
            EstNouveauPatient = false
        };
        
        _context.PatientsMouvements.Add(mouvement);
        
        // Mettre à jour le service actuel du patient
        var patient = await _context.Patients.FindAsync(patientId);
        if (patient != null)
        {
            patient.ServiceIdActuel = serviceId;
        }
        
        await _context.SaveChangesAsync();
        return mouvement;
    }
    
    /// <summary>
    /// Recherche des patients par matricule (autocomplétion)
    /// </summary>
    public async Task<List<Patient>> RechercherPatientsParMatriculeAsync(string prefixe)
    {
        if (string.IsNullOrWhiteSpace(prefixe))
            return new List<Patient>();
            
        return await _context.Patients
            .Where(p => p.Matricule.StartsWith(prefixe) && p.Actif)
            .OrderBy(p => p.Matricule)
            .Take(10)
            .ToListAsync();
    }
    
    /// <summary>
    /// Obtient l'historique complet d'un patient
    /// </summary>
    public async Task<Patient?> ObtenirPatientAvecHistoriqueAsync(int patientId)
    {
        return await _context.Patients
            .Include(p => p.ServiceActuel)
            .Include(p => p.Mouvements.ThenInclude(m => m.Service))
            .FirstOrDefaultAsync(p => p.Id == patientId);
    }
}
