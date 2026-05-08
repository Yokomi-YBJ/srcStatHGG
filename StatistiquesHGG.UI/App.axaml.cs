using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StatistiquesHGG.Business.Services;
using StatistiquesHGG.Infrastructure.Data;
using StatistiquesHGG.Reporting.Generators;
using StatistiquesHGG.UI.Views;

namespace StatistiquesHGG.UI;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        var sc = new ServiceCollection();
        ConfigureServices(sc);
        Services = sc.BuildServiceProvider();

        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            try { db.Database.EnsureCreated(); } catch { }
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new LoginWindow
            {
                DataContext = Services.GetRequiredService<LoginViewModel>()
            };

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var cs = AppSettings.ConnectionString;
        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(cs, ServerVersion.AutoDetect(cs),
                b => b.MigrationsAssembly("StatistiquesHGG.Infrastructure")),
            ServiceLifetime.Transient);

        // Business services
        services.AddTransient<AuthenticationService>();
        services.AddTransient<StatistiquesService>();
        services.AddTransient<PatientRmaService>();      // v5: New patient/death management
        services.AddTransient<RbacService>();            // v5: Role-based access control
        services.AddTransient<RMAGenerator>();

        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<SaisieRmaViewModel>();        // Nouveau
        services.AddTransient<ValidationViewModel>();
        services.AddTransient<RapportViewModel>();
        services.AddTransient<UtilisateursViewModel>();
        services.AddTransient<ClassementViewModel>();
        services.AddTransient<CiblesViewModel>();
    }
}
