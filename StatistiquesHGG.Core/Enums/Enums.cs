namespace StatistiquesHGG.Core.Enums;

public enum RoleType
{
    SuperAdmin,         // Accès total
    Consulteur,         // Lecture seule Dashboard
    ChefDeSaisie,       // Saisie RMA, MAPE, Performances (service assigné)
    AgentDeSaisie       // Saisie RMA uniquement (service assigné)
}

public enum StatutValidation
{
    EnAttente,
    Validee,
    Rejetee
}

public enum NiveauPerformance
{
    Excellent,
    Bon,
    Moyen,
    AAmeliorer
}

public enum TypeDonnee
{
    Consultation,
    Hospitalisation,
    Accouchement,
    ExamenLaboratoire,
    ExamenImagerie,
    Rehabilitation,
    Vaccination,
    BanqueSang,
    Deces,
    Urgence,
    Thanathopraxie
}

public enum TypeRapport
{
    RMA,
    MAPE,
    DHIS2,
    Custom
}

public enum TypeAlerte
{
    Saisie,
    Epidemio,
    Qualite,
    Performance
}

public enum SeveriteAlerte
{
    Info = 1,
    Warning = 2,
    Critique = 3
}

public enum TypeSauvegarde
{
    Auto,
    Manuel
}

public enum StatutSauvegarde
{
    EnCours,
    Reussie,
    Echouee
}

public enum GenrePatient
{
    Masculin,
    Feminin,
    Enfant
}

public enum TypeAccouchement
{
    Cesarienne,
    VoieBasse
}

public enum ModeSortie
{
    Guerison,
    Deces,
    Transfert,
    Evasion,
    Autres
}

public enum TypeExamenImagerie
{
    Radio,
    Echographie,
    Scanner,
    IRM
}
