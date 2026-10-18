using LocatCam_mobile.Services.Interfaces;

namespace LocatCam_mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Garde d'auth : vérifier après le chargement du Shell
        _ = CheckAuthAsync();
    }

    private async Task CheckAuthAsync()
    {
        // Attendre que le Handler soit disponible pour accéder aux services DI
        await Task.Delay(200);

        var services = Application.Current?.Handler?.MauiContext?.Services;
        var authService = services?.GetRequiredService<IAuthService>();

        if (authService != null && !await authService.IsLoggedInAsync())
        {
            await GoToAsync("//LoginPage");
        }
    }
}
