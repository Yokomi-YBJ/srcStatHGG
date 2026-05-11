using Microsoft.EntityFrameworkCore;
using StatistiquesHGG.Core.Entities;
using StatistiquesHGG.Core.Enums;

namespace StatistiquesHGG.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Tables existantes
    public DbSet<Utilisateur>           Utilisateurs           { get; set; }
    public DbSet<Service>               Services               { get; set; }
    public DbSet<Indicateur>            Indicateurs            { get; set; }
    public DbSet<ValeurIndicateur>      ValeursIndicateurs     { get; set; }
    public DbSet<CibleIndicateur>       CiblesIndicateurs      { get; set; }
    public DbSet<ScorePerformance>      ScoresPerformance      { get; set; }
    public DbSet<DetailScoreIndicateur> DetailsScoreIndicateur { get; set; }
    public DbSet<Rapport>               Rapports               { get; set; }
    public DbSet<Alerte>                Alertes                { get; set; }
    public DbSet<LogAction>             LogsActions            { get; set; }
    public DbSet<Sauvegarde>            Sauvegardes            { get; set; }

    // Nouvelles tables RMA
    public DbSet<SaisieConsultation>    SaisiesConsultation    { get; set; }
    public DbSet<SaisieHospitalisation> SaisiesHospitalisation { get; set; }
    public DbSet<SaisieActeChirurgical> SaisiesActeChirurgical { get; set; }
    public DbSet<SaisieUrgence>         SaisiesUrgence         { get; set; }
    public DbSet<SaisieDeces>           SaisiesDeces           { get; set; }
    public DbSet<SaisieLaboratoire>     SaisiesLaboratoire     { get; set; }
    public DbSet<SaisieVaccination>     SaisiesVaccination     { get; set; }
    public DbSet<SaisieBanqueSang>      SaisiesBanqueSang      { get; set; }
    public DbSet<SaisieMorgue>          SaisiesMorgue          { get; set; }
    public DbSet<SaisieMape>            SaisiesMape            { get; set; }
    public DbSet<LigneMape>             LignesMape             { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enums as strings
        modelBuilder.Entity<Utilisateur>().Property(u => u.Role).HasConversion<string>();
        modelBuilder.Entity<ScorePerformance>().Property(s => s.Niveau).HasConversion<string>();
        modelBuilder.Entity<Rapport>().Property(r => r.Type).HasConversion<string>();
        modelBuilder.Entity<Alerte>().Property(a => a.Type).HasConversion<string>();
        modelBuilder.Entity<Alerte>().Property(a => a.Severite).HasConversion<string>();
        modelBuilder.Entity<Sauvegarde>().Property(s => s.Type).HasConversion<string>();
        modelBuilder.Entity<Sauvegarde>().Property(s => s.Statut).HasConversion<string>();

        // Indexes
        modelBuilder.Entity<Utilisateur>().HasIndex(u => u.Login).IsUnique();
        modelBuilder.Entity<Service>().HasIndex(s => s.Code).IsUnique();
        modelBuilder.Entity<SaisieConsultation>().HasIndex(s => new { s.ServiceId, s.DateSaisie });
        modelBuilder.Entity<SaisieHospitalisation>().HasIndex(s => new { s.ServiceId, s.DateSaisie });
        modelBuilder.Entity<SaisieMape>().HasIndex(s => new { s.AnneeEpi, s.SemaineEpi }).IsUnique();

        // Seed Services
        modelBuilder.Entity<Service>().HasData(
            new Service { Id = 1,  Code = "CHIR_GEN",    Libelle = "Chirurgie Générale",         Actif = true, CapaciteLits = 30 },
            new Service { Id = 2,  Code = "CHIR_PED",    Libelle = "Chirurgie Pédiatrique",       Actif = true, CapaciteLits = 20 },
            new Service { Id = 3,  Code = "NEURO",       Libelle = "Neurochirurgie",              Actif = true, CapaciteLits = 15 },
            new Service { Id = 4,  Code = "TRAUMA",      Libelle = "Traumatologie/Orthopédie",   Actif = true, CapaciteLits = 25 },
            new Service { Id = 5,  Code = "MAXILLO",     Libelle = "Maxillo-Faciale",             Actif = true, CapaciteLits = 10 },
            new Service { Id = 6,  Code = "CHIR_VISC",   Libelle = "Chirurgie Viscérale",         Actif = true, CapaciteLits = 15 },
            new Service { Id = 7,  Code = "MED_GEN",     Libelle = "Médecine Générale",           Actif = true, CapaciteLits = 40 },
            new Service { Id = 8,  Code = "MED_INT",     Libelle = "Médecine Interne",            Actif = true, CapaciteLits = 40 },
            new Service { Id = 9,  Code = "ONCOLOGIE",   Libelle = "Oncologie",                   Actif = true, CapaciteLits = 20 },
            new Service { Id = 10, Code = "NUTRITION",   Libelle = "Nutrition",                   Actif = true, CapaciteLits = 10 },
            new Service { Id = 11, Code = "CARDIO",      Libelle = "Cardiologie",                 Actif = true, CapaciteLits = 20 },
            new Service { Id = 12, Code = "GYNECO",      Libelle = "Gynécologie/Obstétrique",     Actif = true, CapaciteLits = 40 },
            new Service { Id = 13, Code = "PEDIATRIE",   Libelle = "Pédiatrie",                   Actif = true, CapaciteLits = 35 },
            new Service { Id = 14, Code = "NEONAT",      Libelle = "Unité de Néonatalogie",       Actif = true, CapaciteLits = 20 },
            new Service { Id = 15, Code = "PSYCHIATRIE", Libelle = "Psychiatrie",                 Actif = true, CapaciteLits = 20 },
            new Service { Id = 16, Code = "REANIMATION", Libelle = "Anesthésie/Réanimation",      Actif = true, CapaciteLits = 10 },
            new Service { Id = 17, Code = "OPHTALMO",    Libelle = "Ophtalmologie",               Actif = true, CapaciteLits = 10 },
            new Service { Id = 18, Code = "ODONTO",      Libelle = "Odontostomatologie",          Actif = true, CapaciteLits = 5  },
            new Service { Id = 19, Code = "LABO",        Libelle = "Laboratoire",                 Actif = true },
            new Service { Id = 20, Code = "IMAGERIE",    Libelle = "Imagerie Médicale",           Actif = true },
            new Service { Id = 21, Code = "CPN",         Libelle = "Consultation Prénatale",      Actif = true },
            new Service { Id = 22, Code = "VACCIN",      Libelle = "Vaccination",                 Actif = true },
            new Service { Id = 23, Code = "BANQ_SANG",   Libelle = "Banque de Sang",              Actif = true },
            new Service { Id = 24, Code = "URGENCES",    Libelle = "Urgences",                    Actif = true, CapaciteLits = 15 },
            new Service { Id = 25, Code = "KINE",        Libelle = "Kinésithérapie",              Actif = true },
            new Service { Id = 26, Code = "MORGUE",      Libelle = "Morgue/Thanathopraxie",       Actif = true },
            new Service { Id = 27, Code = "ENDOCRINO",   Libelle = "Endocrinologie",              Actif = true },
            new Service { Id = 28, Code = "NEURO_MED",   Libelle = "Neurologie",                  Actif = true },
            new Service { Id = 29, Code = "HEMATO",      Libelle = "Hématologie",                 Actif = true },
            new Service { Id = 30, Code = "PNEUMO",      Libelle = "Pneumologie",                 Actif = true },
            new Service { Id = 31, Code = "RHUMATO",     Libelle = "Rhumatologie",                Actif = true },
            new Service { Id = 32, Code = "DERMATO",     Libelle = "Dermatologie",                Actif = true },
            new Service { Id = 33, Code = "HGE",         Libelle = "Hépatho-Gastro-Entérologie",  Actif = true },
            new Service { Id = 34, Code = "ANAPATH",     Libelle = "Anatomopathologie",           Actif = true }
        );

        // Seed Super Admin
        modelBuilder.Entity<Utilisateur>().HasData(new Utilisateur
        {
            Id = 1,
            Login = "stive.admin",
            MotDePasseHash = "$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2uheWG/igi.",
            Nom = "STIVE", Prenom = "M.",
            Email = "admin@hgg.cm",
            Actif = true,
            Role = RoleType.SuperAdmin,
            DateCreation = new DateTime(2026, 1, 1)
        });

        // Seed Indicateurs
        modelBuilder.Entity<Indicateur>().HasData(
            new Indicateur { Id = 1, Code = "TX_OCC",    Libelle = "Taux d'occupation des lits",  Unite = "%",       Categorie = "Occupation" },
            new Indicateur { Id = 2, Code = "DMS",       Libelle = "Durée Moyenne de Séjour",      Unite = "jours",   Categorie = "Occupation" },
            new Indicateur { Id = 3, Code = "TX_MORT",   Libelle = "Taux de Mortalité",            Unite = "%",       Categorie = "Mortalité"  },
            new Indicateur { Id = 4, Code = "TX_CESAR",  Libelle = "Taux de Césarienne",           Unite = "%",       Categorie = "Maternité"  },
            new Indicateur { Id = 5, Code = "NB_CONSULT",Libelle = "Nombre de Consultations",      Unite = "cas",     Categorie = "Activité"   },
            new Indicateur { Id = 6, Code = "NB_HOSP",   Libelle = "Nombre d'Hospitalisations",    Unite = "cas",     Categorie = "Activité"   },
            new Indicateur { Id = 7, Code = "NB_ACTES",  Libelle = "Actes Chirurgicaux",           Unite = "actes",   Categorie = "Chirurgie"  },
            new Indicateur { Id = 8, Code = "NB_SEANCES",Libelle = "Séances Kinésithérapie",       Unite = "séances", Categorie = "Réhabilitation" }
        );
    }
}
