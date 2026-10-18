using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocatCam_mobile.Models;
using LocatCam_mobile.Services.Interfaces;

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

        IsBusy = true;
        Message = string.Empty;

        try
        {
            var result = await _authService.LoginAsync(Email, MotDePasse);

            if (result == null)
            {
                Message = "Email ou mot de passe incorrect.";
                MessageIsError = true;
                return;
            }

            Message = $"Connexion réussie ! Bienvenue, {result.User.Nom}.";
            MessageIsError = false;

            // Navigation vers l'accueil après un court délai pour laisser le message s'afficher
            await Task.Delay(800);
            await Shell.Current.GoToAsync("//MainPage");
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
        await Shell.Current.GoToAsync("RegisterPage");
    }
}
