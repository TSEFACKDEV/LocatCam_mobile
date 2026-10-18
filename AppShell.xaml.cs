using LocatCam_mobile.Services;
using LocatCam_mobile.Services.Interfaces;
using Microsoft.Maui.Storage;

namespace LocatCam_mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Garde d'auth : vérifier après le chargement du Shell (évite le blocage au démarrage)
        _ = CheckAuthAsync();
    }

    private async Task CheckAuthAsync()
    {
        // Attendre que la fenêtre soit prête (Shell.Current disponible)
        await Task.Delay(150);

        try
        {
            // Ne pas interrompre l'onboarding : c'est lui qui décide de la suite.
            if (!Preferences.Get("OnboardingDone", false))
                return;

            var services = Application.Current?.Handler?.MauiContext?.Services;
            var authService = services?.GetRequiredService<IAuthService>();

            bool connecte;
            if (authService != null)
            {
                connecte = await authService.IsLoggedInAsync();
            }
            else
            {
                // Repli : lire directement le token du SecureStorage
                var token = await SecureStorage.GetAsync("locatcam_access_token");
                connecte = !string.IsNullOrWhiteSpace(token);
            }

            if (!connecte)
            {
                // Rediriger vers Login seulement si on n'y est pas déjà
                var courante = NavigationShell.RouteCourante;
                if (courante != "LoginPage" && courante != "RegisterPage")
                    NavigationShell.AllerVers("LoginPage");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AppShell] Erreur garde d'auth: {ex.Message}");
        }
    }
}
