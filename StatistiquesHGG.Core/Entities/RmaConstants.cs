namespace StatistiquesHGG.Core.Entities;

public static class RmaConstants
{
    // Services de consultation externe (28 spécialités RMA)
    public static readonly string[] ServicesConsultation = {
        "ANESTHESIE/REANIMATION", "CHIRURGIE GENERALE", "CHIRURGIE VISCERALE",
        "NEUROCHIRURGIE", "TRAUMATOLOGIE/ORTHOPEDIE", "ODONTOSTOMATOLOGIE",
        "MEDECINE GENERALE", "ONCOLOGIE", "NUTRITION", "PEDIATRIE",
        "PSYCHIATRIE", "GYNECOLOGIE/OBSTETRIQUE", "CONSULTATION PRENATALE",
        "CONSULTATION POST NATALE", "PLANNING FAMILIAL", "CARDIOLOGIE",
        "DERMATOLOGIE", "ENDOCRINOLOGIE", "HEPATO-GASTRO-ENTEROLOGIE",
        "HEMATOLOGIE", "NEUROLOGIE", "PNEUMOLOGIE", "RHUMATOLOGIE",
        "OPHTALMOLOGIE", "KINESITHERAPIE", "CHIRURGIE MAXILLO-FACIALE",
        "CHIRURGIE PEDIATRIQUE", "MEDECINE INTERNE"
    };

    // Services d'hospitalisation
    public static readonly (string Service, string[] Activites)[] ServicesHospitalisation =
    {
        ("GYNECOLOGIE/OBSTETRIQUE", new[] {
            "Hospitalisation pour maladie",
            "Hospitalisation pour accouchement par voie basse",
            "Hospitalisation pour accouchement par césarienne",
            "Hospitalisation pour autres actes chirurgicaux"
        }),
        ("UNITE DE REANIMATION",   new[] { "Hospitalisation" }),
        ("MEDECINE INTERNE",       new[] { "Hospitalisation" }),
        ("CHIRURGIE",              new[] { "Hospitalisation" }),
        ("ONCOLOGIE",              new[] { "Hospitalisation" }),
        ("PEDIATRIE",              new[] { "Hospitalisation" }),
        ("UNITE DE NEONATALOGIE",  new[] { "Hospitalisation" }),
    };

    // Actes chirurgicaux (Table 1 RMA)
    public static readonly (string Specialite, string[] Actes)[] ActesChirurgicaux =
    {
        ("CHIRURGIE GENERALE",     new[] { "Interventions chirurgicales" }),
        ("CHIRURGIE VISCERALE",    new[] { "Interventions chirurgicales" }),
        ("CHIRURGIE PEDIATRIQUE",  new[] { "Interventions chirurgicales" }),
        ("NEUROCHIRURGIE",         new[] { "Interventions chirurgicales" }),
        ("TRAUMATOLOGIE",          new[] { "Interventions chirurgicales" }),
        ("GYNECOLOGIE/OBSTETRIQUE",new[] { "Interventions chirurgicales" }),
        ("CHIRURGIE MAXILLO-FACIALE", new[] { "Interventions chirurgicales" }),
    };

    // Actes Odontostomatologie
    public static readonly string[] ActesOdonto =
    {
        "DETARTRAGES", "OBTURATIONS", "EXTRACTIONS",
        "RADIO RETRO ALVEOLAIRE", "CURETAGE DE POCHE",
        "PULPOTOMIE", "DEVITALISATION", "BIOPSIE",
        "CHIRURGIE MAXILLO-FACIALE", "POSE D'APPAREIL DENTAIRE"
    };

    // Actes Ophtalmologie
    public static readonly string[] ActesOphtalmo =
    {
        "FOND ŒIL", "REFRACTION CYCLO", "REFRACTION SIMPLE",
        "TONOMETRIE (PIO)", "GONIOSCOPIE", "CHAMP VISUEL",
        "LASER YAG", "PACHYMETRIE", "ABLATION CE",
        "TEST FLUORESCEINE", "TEST ISHIHARA", "SHIRMER",
        "ECHO OCULAIRE", "RAPPORT MEDICAL"
    };

    // Actes Autres spécialités (Table 2 RMA)
    public static readonly (string Service, string[] Actes)[] AutresActes =
    {
        ("ANAPATH", new[] { "Analyses cytologiques", "Analyses histologiques" }),
        ("CARDIOLOGIE", new[] {
            "Échocardiographie", "Écho trans-œsophagienne",
            "ECG", "Test d'effort", "Holter cardiaque"
        }),
        ("IMAGERIE", new[] { "Radio Standard", "Échographie", "Scanner", "IRM" }),
        ("KINESITHERAPIE", new[] {
            "Rééducation para faciale", "Rééducation épaule", "Rééducation genou",
            "Rééducation hémiplégie", "Rééducation membre inférieur",
            "Rééducation membre supérieur", "Rééducation respiratoire",
            "Rééducation paraplégie", "Rééducation tétraplégie",
            "Rééducation cheville", "Rééducation coude", "Rééducation poignet",
            "Paralysie plexus brachial", "Rééducation rachis"
        }),
        ("ENDOCRINOLOGIE", new[] { "Consultation", "Bilan diabétique" }),
        ("NEUROLOGIE", new[] { "Consultation", "EEG", "EMG" }),
        ("HEMATOLOGIE", new[] { "Consultation", "Transfusion" }),
    };
}
