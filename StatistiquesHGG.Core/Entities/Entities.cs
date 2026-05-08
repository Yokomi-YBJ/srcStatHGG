using StatistiquesHGG.Core.Enums;

namespace StatistiquesHGG.Core.Entities;

public class Utilisateur
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string MotDePasseHash { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool Actif { get; set; } = true;
    public DateTime DateCreation { get; set; } = DateTime.Now;
    public DateTime? DerniereConnexion { get; set; }
    public RoleType Role { get; set; }
    public int? ServiceId { get; set; }
    public Service? Service { get; set; }
    public string NomComplet => $"{Prenom} {Nom}";
}

public class Service
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public bool Actif { get; set; } = true;
    public string? Description { get; set; }
    public int? CapaciteLits { get; set; }
    public List<DonneeHospitaliere> Donnees { get; set; } = new();
}

public class DonneeHospitaliere
{
    public int Id { get; set; }
    public TypeDonnee TypeDonnee { get; set; }
    public DateTime DateEnregistrement { get; set; }
    public int ServiceId { get; set; }
    public Service? Service { get; set; }
    public StatutValidation Statut { get; set; } = StatutValidation.EnAttente;
    public int SaisieParId { get; set; }
    public Utilisateur? SaisiePar { get; set; }
    public DateTime DateSaisie { get; set; } = DateTime.Now;
    public int? ValideeParId { get; set; }
    public Utilisateur? ValideePar { get; set; }
    public DateTime? DateValidation { get; set; }
    public string? CommentaireValidation { get; set; }
}

public class Consultation : DonneeHospitaliere
{
    public string? Motif { get; set; }
    public string? Diagnostic { get; set; }
    public GenrePatient Genre { get; set; }
    public int? Age { get; set; }
    public bool NouvelleConsultation { get; set; } = true;
    public string? Orientation { get; set; }
}

public class Hospitalisation : DonneeHospitaliere
{
    public DateTime DateAdmission { get; set; }
    public DateTime? DateSortie { get; set; }
    public string? Diagnostic { get; set; }
    public GenrePatient Genre { get; set; }
    public int? Age { get; set; }
    public ModeSortie? ModeSortie { get; set; }
    public int Duree => DateSortie.HasValue ? (int)(DateSortie.Value - DateAdmission).TotalDays : 0;
}

public class Accouchement : DonneeHospitaliere
{
    public DateTime DateAccouchement { get; set; }
    public TypeAccouchement Type { get; set; }
    public GenrePatient GenreNouveauNe { get; set; }
    public decimal? PoidsNaissance { get; set; }
    public string? Complications { get; set; }
    public int DureeSejour { get; set; }
}

public class ExamenLaboratoire : DonneeHospitaliere
{
    public string TypeExamen { get; set; } = string.Empty;
    public string? Resultat { get; set; }
    public int? ServiceDemandeurId { get; set; }
}

public class ExamenImagerie : DonneeHospitaliere
{
    public TypeExamenImagerie TypeExamen { get; set; }
    public string? Resultat { get; set; }
    public int? ServiceDemandeurId { get; set; }
}

public class Rehabilitation : DonneeHospitaliere
{
    public string TypeReeducation { get; set; } = string.Empty;
    public GenrePatient Genre { get; set; }
    public int NombreSeances { get; set; } = 1;
}

public class Indicateur
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public string? Formule { get; set; }
    public string? Unite { get; set; }
    public string? Categorie { get; set; }
    public List<ValeurIndicateur> Valeurs { get; set; } = new();
}

public class ValeurIndicateur
{
    public int Id { get; set; }
    public int IndicateurId { get; set; }
    public Indicateur? Indicateur { get; set; }
    public decimal Valeur { get; set; }
    public DateTime Periode { get; set; }
    public int? ServiceId { get; set; }
    public Service? Service { get; set; }
    public DateTime DateCalcul { get; set; } = DateTime.Now;
}

public class CibleIndicateur
{
    public int Id { get; set; }
    public int IndicateurId { get; set; }
    public Indicateur? Indicateur { get; set; }
    public int ServiceId { get; set; }
    public Service? Service { get; set; }
    public decimal ValeurCible { get; set; }
    public string Periode { get; set; } = "MENSUEL";
    public bool Actif { get; set; } = true;
    public DateTime? DateDebut { get; set; }
    public DateTime? DateFin { get; set; }
}

public class ScorePerformance
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public Service? Service { get; set; }
    public DateTime Periode { get; set; }
    public decimal ValeurReelle { get; set; }
    public decimal ValeurCible { get; set; }
    public decimal Score => ValeurCible != 0 ? Math.Round((ValeurReelle / ValeurCible) * 100, 2) : 0;
    public int Rang { get; set; }
    public NiveauPerformance Niveau { get; set; }
    public DateTime DateCalcul { get; set; } = DateTime.Now;
    public List<DetailScoreIndicateur> Details { get; set; } = new();
    public string NiveauLibelle => Niveau switch
    {
        NiveauPerformance.Excellent => "Excellent",
        NiveauPerformance.Bon => "Bon",
        NiveauPerformance.Moyen => "Moyen",
        NiveauPerformance.AAmeliorer => "À améliorer",
        _ => "Inconnu"
    };
}

public class DetailScoreIndicateur
{
    public int Id { get; set; }
    public int ScorePerformanceId { get; set; }
    public ScorePerformance? ScorePerformance { get; set; }
    public int IndicateurId { get; set; }
    public Indicateur? Indicateur { get; set; }
    public decimal ValeurReelle { get; set; }
    public decimal ValeurCible { get; set; }
    public decimal ScoreIndicateur => ValeurCible != 0 ? Math.Round((ValeurReelle / ValeurCible) * 100, 2) : 0;
    public decimal Poids { get; set; } = 1.0m;
}

public class Rapport
{
    public int Id { get; set; }
    public TypeRapport Type { get; set; }
    public string Titre { get; set; } = string.Empty;
    public DateTime Periode { get; set; }
    public int GenereParId { get; set; }
    public Utilisateur? GenerePar { get; set; }
    public DateTime DateGeneration { get; set; } = DateTime.Now;
    public string? CheminFichier { get; set; }
}

public class Alerte
{
    public int Id { get; set; }
    public TypeAlerte Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public SeveriteAlerte Severite { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.Now;
    public bool Traitee { get; set; } = false;
    public int? TraiteeParId { get; set; }
    public DateTime? DateTraitement { get; set; }
}

public class LogAction
{
    public int Id { get; set; }
    public int UtilisateurId { get; set; }
    public Utilisateur? Utilisateur { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? TableCible { get; set; }
    public int? EnregistrementId { get; set; }
    public string? AncienneValeur { get; set; }
    public string? NouvelleValeur { get; set; }
    public DateTime DateAction { get; set; } = DateTime.Now;
    public string? AdresseIP { get; set; }
}

public class Sauvegarde
{
    public int Id { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.Now;
    public TypeSauvegarde Type { get; set; }
    public long? Taille { get; set; }
    public string? CheminFichier { get; set; }
    public StatutSauvegarde Statut { get; set; }
}
