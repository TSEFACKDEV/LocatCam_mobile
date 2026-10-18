using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocatCam_mobile.Services.Interfaces;

namespace LocatCam_mobile.ViewModels;

/// <summary>
/// ViewModel de la page profil utilisateur.
/// </summary>
public partial class ProfilViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _nom = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _role = string.Empty;

    [ObservableProperty]
    private string _telephone = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public ProfilViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    public async Task ChargerProfilAsync()
    {
        IsLoading = true;

        try
        {
            var user = await _authService.GetCurrentUserAsync();

            if (user != null)
            {
                Nom = user.Nom;
                Email = user.Email;
                Role = user.Role;
                Telephone = user.Telephone;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProfilViewModel] Erreur: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task DeconnexionAsync()
    {
        await _authService.LogoutAsync();
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
