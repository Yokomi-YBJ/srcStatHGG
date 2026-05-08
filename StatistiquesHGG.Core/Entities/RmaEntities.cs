namespace StatistiquesHGG.Core.Entities;

// ===== SAISIE RMA — par totaux journaliers =====

/// <summary>Saisie journalière des consultations externes par service</summary>
public class SaisieConsultation
{
    public int      Id            { get; set; }
    public int      ServiceId     { get; set; }
    public Service? Service       { get; set; }
    public DateTime DateSaisie    { get; set; }
    public int      SaisieParId   { get; set; }
    public Utilisateur? SaisiePar { get; set; }
    public bool     Validee       { get; set; } = false;
    public int?     ValideeParId  { get; set; }
    public DateTime? DateValidation { get; set; }

    // Nouvelles consultations (patients jamais vus avant)
    public int NouveauxHommes   { get; set; }
    public int NouvellesFemmes  { get; set; }
    public int NouveauxEnfants  { get; set; }

    // Anciennes consultations (patients déjà enregistrés)
    public int AnciensHommes    { get; set; }
    public int AnciennesFemmes  { get; set; }
    public int AnciensEnfants   { get; set; }

    // v5: Tracking des nouveaux patients
    public int NouveauxPatientsComptabilises { get; set; }

    public int TotalNouveaux  => NouveauxHommes + NouvellesFemmes + NouveauxEnfants;
    public int TotalAnciens   => AnciensHommes + AnciennesFemmes + AnciensEnfants;
    public int TotalGeneral   => TotalNouveaux + TotalAnciens;
}

/// <summary>Saisie journalière des hospitalisations</summary>
public class SaisieHospitalisation
{
    public int      Id            { get; set; }
    public int      ServiceId     { get; set; }
    public Service? Service       { get; set; }
    public string   TypeActivite  { get; set; } = string.Empty;
    public DateTime DateSaisie    { get; set; }
    public int      SaisieParId   { get; set; }
    public Utilisateur? SaisiePar { get; set; }
    public bool     Validee       { get; set; } = false;
    public int?     ValideeParId  { get; set; }
    public DateTime? DateValidation { get; set; }

    public int Hommes   { get; set; }
    public int Femmes   { get; set; }
    public int Enfants  { get; set; }
    public int JoursHospitalisation { get; set; }

    // v5: Tracking patient
    public int NombreEntrees { get; set; }
    public int NombreSorties { get; set; }

    public int Total => Hommes + Femmes + Enfants;
}

/// <summary>Saisie journalière des actes chirurgicaux</summary>
public class SaisieActeChirurgical
{
    public int      Id            { get; set; }
    public int      ServiceId     { get; set; }
    public Service? Service       { get; set; }
    public string   TypeActe      { get; set; } = string.Empty;
    public DateTime DateSaisie    { get; set; }
    public int      SaisieParId   { get; set; }
    public Utilisateur? SaisiePar { get; set; }
    public bool     Validee       { get; set; } = false;
    public int?     ValideeParId  { get; set; }
    public DateTime? DateValidation { get; set; }

    public int Hommes  { get; set; }
    public int Femmes  { get; set; }
    public int Enfants { get; set; }
    public int Total   => Hommes + Femmes + Enfants;
}

/// <summary>Saisie journalière des actes urgences</summary>
public class SaisieUrgence
{
    public int      Id            { get; set; }
    public DateTime DateSaisie    { get; set; }
    public int      SaisieParId   { get; set; }
    public Utilisateur? SaisiePar { get; set; }
    public bool     Validee       { get; set; } = false;
    public int?     ValideeParId  { get; set; }
    public DateTime? DateValidation { get; set; }

    // Colonnes spécifiques urgences
    public int MiseEnObservationH  { get; set; }
    public int MiseEnObservationF  { get; set; }
    public int MiseEnObservationEM { get; set; }
    public int MiseEnObservationEF { get; set; }

    public int TransfertH  { get; set; }
    public int TransfertF  { get; set; }
    public int TransfertEM { get; set; }
    public int TransfertEF { get; set; }

    public int SortieAutoriseeH  { get; set; }
    public int SortieAutoriseeF  { get; set; }
    public int SortieAutoriseeEM { get; set; }
    public int SortieAutoriseeEF { get; set; }

    public int ScamH  { get; set; }
    public int ScamF  { get; set; }
    public int ScamEM { get; set; }
    public int ScamEF { get; set; }

    public int EvadesH  { get; set; }
    public int EvadesF  { get; set; }
    public int EvadesEM { get; set; }
    public int EvadesEF { get; set; }
}

/// <summary>Saisie journalière des décès (v5: auto-link to morgue if needed)</summary>
public class SaisieDeces
{
    public int      Id            { get; set; }
    public int      ServiceId     { get; set; }
    public Service? Service       { get; set; }
    public string   TypeActivite  { get; set; } = string.Empty;
    public DateTime DateSaisie    { get; set; }
    public int      SaisieParId   { get; set; }
    public Utilisateur? SaisiePar { get; set; }
    public bool     Validee       { get; set; } = false;
    public int?     ValideeParId  { get; set; }
    public DateTime? DateValidation { get; set; }

    public int Hommes  { get; set; }
    public int Femmes  { get; set; }
    public int Enfants { get; set; }

    // v5: Auto-transfer to morgue management
    public bool TransfereAMorgueAuto { get; set; } = false;
    public int? MorgueEntreeId { get; set; }

    public int Total   => Hommes + Femmes + Enfants;
}

/// <summary>Saisie des actes laboratoire</summary>
public class SaisieLaboratoire
{
    public int      Id            { get; set; }
    public DateTime DateSaisie    { get; set; }
    public int      SaisieParId   { get; set; }
    public Utilisateur? SaisiePar { get; set; }
    public bool     Validee       { get; set; } = false;
    public int?     ValideeParId  { get; set; }
    public DateTime? DateValidation { get; set; }

    public int Bacteriologie   { get; set; }
    public int Biochimie       { get; set; }
    public int Hematologie     { get; set; }
    public int Hormonologie    { get; set; }
    public int Immunologie     { get; set; }
    public int Parasitologie   { get; set; }
    public int Anatomopathologie { get; set; }
}

/// <summary>Saisie vaccinaton</summary>
public class SaisieVaccination
{
    public int      Id            { get; set; }
    public DateTime DateSaisie    { get; set; }
    public int      SaisieParId   { get; set; }
    public Utilisateur? SaisiePar { get; set; }
    public bool     Validee       { get; set; } = false;
    public int?     ValideeParId  { get; set; }
    public DateTime? DateValidation { get; set; }

    public int BCG_VPO0_M { get; set; }
    public int BCG_VPO0_F { get; set; }
    public int Penta1_M   { get; set; }
    public int Penta1_F   { get; set; }
    public int Penta2_M   { get; set; }
    public int Penta2_F   { get; set; }
    public int Penta3_M   { get; set; }
    public int Penta3_F   { get; set; }
    public int VAR_M      { get; set; }
    public int VAR_F      { get; set; }
    public int VAA_M      { get; set; }
    public int VAA_F      { get; set; }
    public int VPI_M      { get; set; }
    public int VPI_F      { get; set; }
    public int Td_M       { get; set; }
    public int Td_F       { get; set; }
    public int HPV_M      { get; set; }
    public int HPV_F      { get; set; }
}

/// <summary>Saisie banque de sang</summary>
public class SaisieBanqueSang
{
    public int      Id            { get; set; }
    public DateTime DateSaisie    { get; set; }
    public int      SaisieParId   { get; set; }
    public Utilisateur? SaisiePar { get; set; }
    public bool     Validee       { get; set; } = false;
    public int?     ValideeParId  { get; set; }
    public DateTime? DateValidation { get; set; }

    public int DonneursRecusM    { get; set; }
    public int DonneursRecusF    { get; set; }
    public int PochesCollecteesM { get; set; }
    public int PochesCollecteesF { get; set; }
    public int PochesServiesM    { get; set; }
    public int PochesServiesF    { get; set; }
    public int PochesServiesE    { get; set; }
}

/// <summary>Saisie morgue/thanathopraxie (v5: distinguish hôpital vs external)</summary>
public class SaisieMorgue
{
    public int      Id            { get; set; }
    public DateTime DateSaisie    { get; set; }
    public int      SaisieParId   { get; set; }
    public Utilisateur? SaisiePar { get; set; }
    public bool     Validee       { get; set; } = false;
    public int?     ValideeParId  { get; set; }
    public DateTime? DateValidation { get; set; }

    public int CasHommes  { get; set; }
    public int CasFemmes  { get; set; }
    public int CasEnfants { get; set; }
    public int JoursH     { get; set; }
    public int JoursF     { get; set; }
    public int JoursE     { get; set; }

    // v5: Track décédés hôpital vs provenance externe
    public int CasFromHopital { get; set; }
    public int CasExterieurs  { get; set; }

    public int Total      => CasHommes + CasFemmes + CasEnfants;
}

// ===== SAISIE MAPE — hebdomadaire =====
public class SaisieMape
{
    public int      Id             { get; set; }
    public DateTime DateDebut      { get; set; }
    public DateTime DateFin        { get; set; }
    public int      SemaineEpi     { get; set; }
    public int      AnneeEpi       { get; set; }
    public int      SaisieParId    { get; set; }
    public Utilisateur? SaisiePar  { get; set; }
    public bool     Validee        { get; set; } = false;
    public int?     ValideeParId   { get; set; }
    public DateTime? DateValidation { get; set; }
    public DateTime DateEnregistrement { get; set; } = DateTime.Now;

    public List<LigneMape> Lignes  { get; set; } = new();
}

public class LigneMape
{
    public int       Id          { get; set; }
    public int       SaisieMapeId { get; set; }
    public SaisieMape? SaisieMape { get; set; }
    public string    Maladie     { get; set; } = string.Empty;
    public bool      EstUrgence  { get; set; } = false; // marqué * dans le formulaire

    // Cas suspects par tranche d'âge (M/F)
    public int CS_0_14_M  { get; set; }
    public int CS_0_14_F  { get; set; }
    public int CS_15_24_M { get; set; }
    public int CS_15_24_F { get; set; }
    public int CS_25_64_M { get; set; }
    public int CS_25_64_F { get; set; }
    public int CS_65p_M   { get; set; }
    public int CS_65p_F   { get; set; }

    // Décès par tranche d'âge (M/F)
    public int DC_0_14_M  { get; set; }
    public int DC_0_14_F  { get; set; }
    public int DC_15_24_M { get; set; }
    public int DC_15_24_F { get; set; }
    public int DC_25_64_M { get; set; }
    public int DC_25_64_F { get; set; }
    public int DC_65p_M   { get; set; }
    public int DC_65p_F   { get; set; }

    public int NbEchantillons { get; set; }
    public int CasConfirmes   { get; set; }

    public int TotalCasSuspects => CS_0_14_M + CS_0_14_F + CS_15_24_M + CS_15_24_F +
                                   CS_25_64_M + CS_25_64_F + CS_65p_M + CS_65p_F;
    public int TotalDeces => DC_0_14_M + DC_0_14_F + DC_15_24_M + DC_15_24_F +
                             DC_25_64_M + DC_25_64_F + DC_65p_M + DC_65p_F;
}

