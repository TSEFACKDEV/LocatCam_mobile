using LocatCam_mobile.Services;
using LocatCam_mobile.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Storage;

namespace LocatCam_mobile.Views;

/// <summary>
/// Page de démarrage (splash applicatif).
///
/// Elle est le premier élément du Shell : elle évite que l'accueil ou la page
/// de connexion ne s'affiche brièvement avant la redirection. Elle décide une
/// seule fois de la première page à présenter :
///   1. Onboarding si c'est le premier lancement ;
///   2. Accueil si un utilisateur est connecté ;
///   3. Connexion sinon.
/// </summary>
public partial class StartupPage : ContentPage
{
    private const int DureeMinimaleMs = 550;

    private bool _demarrageEffectue;

    public StartupPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_demarrageEffectue)
            return;

        _demarrageEffectue = true;

        var debut = DateTime.UtcNow;
        var onboardingTermine = Preferences.Get("OnboardingDone", false);
        var connecte = false;

        if (onboardingTermine && Preferences.Get("locatcam_session_active", false))
        {
            // Vérification bornée : SecureStorage ne doit jamais bloquer le démarrage.
            var verification = EstConnecteAsync();
            var gagnant = await Task.WhenAny(verification, Task.Delay(1500));
            connecte = gagnant == verification && await verification;
        }

        // Conserver brièvement l'écran de marque pour éviter un « flash ».
        var ecoule = (DateTime.UtcNow - debut).TotalMilliseconds;
        if (ecoule < DureeMinimaleMs)
            await Task.Delay(DureeMinimaleMs - (int)ecoule);

        var route = !onboardingTermine
            ? "OnboardingPage"
            : connecte ? "HomePage" : "LoginPage";

        NavigationShell.AllerVers(route);
    }

    private static async Task<bool> EstConnecteAsync()
    {
        try
        {
            var authService = App.ServiceProvider?.GetService<IAuthService>();
            if (authService != null)
                return await authService.IsLoggedInAsync();

            var token = await SecureStorage.GetAsync("locatcam_access_token");
            return !string.IsNullOrWhiteSpace(token);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StartupPage] Vérification de session: {ex.Message}");
            return false;
        }
    }
}
