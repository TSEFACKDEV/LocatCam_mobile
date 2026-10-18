using LocatCam_mobile.Services.Interfaces;

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
        // Attendre que le Handler soit disponible pour accéder aux services DI
        await Task.Delay(100);

        var services = Application.Current?.Handler?.MauiContext?.Services;
        var authService = services?.GetRequiredService<IAuthService>();

        if (authService != null && !await authService.IsLoggedInAsync())
        {
            // Rediriger vers Login seulement si on n'y est pas déjà
            if (CurrentState.Location.AbsolutePath != "//LoginPage")
            {
                await GoToAsync("//LoginPage");
            }
        }
    }
}
