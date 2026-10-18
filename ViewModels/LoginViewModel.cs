using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocatCam_mobile.Models;
using LocatCam_mobile.Services.Interfaces;
using Microsoft.Maui.Networking;

namespace LocatCam_mobile.ViewModels;

/// <summary>
/// ViewModel de la page de connexion.
/// </summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _motDePasse = string.Empty;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _messageIsError;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(MotDePasse))
        {
            Message = "Veuillez saisir l'email et le mot de passe.";
            MessageIsError = true;
            return;
        }

        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            Message = "Aucune connexion Internet. Vérifiez votre réseau puis réessayez.";
            MessageIsError = true;
            return;
        }

        IsBusy = true;
        Message = string.Empty;

        try
        {
            var result = await _authService.LoginAsync(Email, MotDePasse);

            if (result == null)
            {
                Message = string.IsNullOrWhiteSpace(_authService.DerniereErreur)
                    ? "Email ou mot de passe incorrect."
                    : _authService.DerniereErreur;
                MessageIsError = true;
                return;
            }

            Message = $"Connexion réussie ! Bienvenue, {result.User.Nom}.";
            MessageIsError = false;

            // Navigation vers l'accueil après un court délai pour laisser le message s'afficher
            await Task.Delay(800);
            Services.NavigationShell.AllerVers("HomePage");
        }
        catch (Exception ex)
        {
            Message = $"Erreur : {ex.Message}";
            MessageIsError = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task GoToRegisterAsync()
    {
        await Task.CompletedTask;
        Services.NavigationShell.AllerVers("RegisterPage");
    }
}
