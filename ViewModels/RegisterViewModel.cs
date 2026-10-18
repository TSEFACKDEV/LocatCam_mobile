using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocatCam_mobile.Models;
using LocatCam_mobile.Services.Interfaces;
using System.Text.RegularExpressions;

namespace LocatCam_mobile.ViewModels;

/// <summary>
/// ViewModel de la page d'inscription.
/// </summary>
public partial class RegisterViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _nom = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _telephone = string.Empty;

    [ObservableProperty]
    private string _motDePasse = string.Empty;

    [ObservableProperty]
    private string _confirmationMotDePasse = string.Empty;

    [ObservableProperty]
    private string _role = "Etudiant";

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _messageIsError;

    public RegisterViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    public async Task RegisterAsync()
    {
        // Validation côté mobile
        if (string.IsNullOrWhiteSpace(Nom))
        {
            Message = "Le nom est requis.";
            MessageIsError = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(Email) || !EstEmailValide(Email))
        {
            Message = "Veuillez saisir une adresse email valide.";
            MessageIsError = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(Telephone) || !EstTelephoneValide(Telephone))
        {
            Message = "Veuillez saisir un téléphone camerounais valide (+237 6XX XXX XXX).";
            MessageIsError = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(MotDePasse) || MotDePasse.Length < 6)
        {
            Message = "Le mot de passe doit contenir au moins 6 caractères.";
            MessageIsError = true;
            return;
        }

        if (MotDePasse != ConfirmationMotDePasse)
        {
            Message = "Les mots de passe ne correspondent pas.";
            MessageIsError = true;
            return;
        }

        if (Role != "Etudiant" && Role != "Bailleur")
        {
            Message = "Le rôle doit être Etudiant ou Bailleur.";
            MessageIsError = true;
            return;
        }

        IsBusy = true;
        Message = string.Empty;

        try
        {
            var success = await _authService.RegisterAsync(
                Nom, Email, Telephone, MotDePasse, Role);

            if (!success)
            {
                Message = "L'inscription a échoué (email ou téléphone déjà utilisé ?).";
                MessageIsError = true;
                return;
            }

            Message = "Inscription réussie ! Vous pouvez maintenant vous connecter.";
            MessageIsError = false;

            await Task.Delay(1000);
            Services.NavigationShell.AllerVers("LoginPage");
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
    public async Task GoToLoginAsync()
    {
        await Task.CompletedTask;
        Services.NavigationShell.AllerVers("LoginPage");
    }

    private static bool EstEmailValide(string email)
    {
        try
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }

    private static bool EstTelephoneValide(string telephone)
    {
        // Format camerounais : +237 6XX XXX XXX ou 6XX XXX XXX
        var cleaned = telephone.Replace(" ", "").Replace("+", "").Replace("-", "");
        if (cleaned.StartsWith("237"))
            cleaned = cleaned.Substring(3);
        return cleaned.Length == 9 && cleaned.StartsWith("6");
    }
}
