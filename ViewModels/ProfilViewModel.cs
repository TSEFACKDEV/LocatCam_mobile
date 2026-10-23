using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocatCam_mobile.Models;
using LocatCam_mobile.Services.Interfaces;
using Microsoft.Maui.Media;
using System.IO;
using System.Net.Http.Json;

namespace LocatCam_mobile.ViewModels;

/// <summary>
/// ViewModel de la page profil utilisateur.
/// Permet de consulter et de modifier la photo de profil.
/// </summary>
public partial class ProfilViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IAnnonceService _annonceService;

    [ObservableProperty]
    private string _nom = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _role = string.Empty;

    [ObservableProperty]
    private string _telephone = string.Empty;

    [ObservableProperty]
    private string _photoUrl = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _estEnChargementPhoto;

    public ProfilViewModel(IAuthService authService, IAnnonceService annonceService)
    {
        _authService = authService;
        _annonceService = annonceService;
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
                PhotoUrl = user.PhotoUrl ?? string.Empty;
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

    /// <summary>Sélectionne une photo depuis la galerie et l'upload sur Cloudinary.</summary>
    [RelayCommand]
    public async Task SelectionnerPhotoProfilAsync()
    {
        try
        {
            var photo = await MediaPicker.PickPhotoAsync();
            if (photo == null) return;

            EstEnChargementPhoto = true;

            using var stream = await photo.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();

            // 1. Obtenir la signature Cloudinary depuis l'API
            var signature = await _annonceService.GetSignatureAsync();
            if (signature == null)
            {
                System.Diagnostics.Debug.WriteLine("[ProfilViewModel] Impossible d'obtenir la signature Cloudinary.");
                return;
            }

            // 2. Uploader l'image sur Cloudinary
            const string CloudinaryCloudName = "locatcam";
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
            using var content = new MultipartFormDataContent();

            content.Add(new ByteArrayContent(bytes, 0, bytes.Length), "file", photo.FileName ?? "profile.jpg");
            content.Add(new StringContent(signature.Timestamp.ToString()), "timestamp");
            content.Add(new StringContent(signature.Folder), "folder");
            content.Add(new StringContent(signature.UploadPreset), "upload_preset");
            content.Add(new StringContent(signature.Signature), "signature");

            var uploadUrl = $"https://api.cloudinary.com/v1_1/{CloudinaryCloudName}/image/upload";
            var uploadResponse = await httpClient.PostAsync(uploadUrl, content);

            if (!uploadResponse.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"[ProfilViewModel] Échec upload Cloudinary: {uploadResponse.StatusCode}");
                return;
            }

            var uploadResult = await uploadResponse.Content.ReadFromJsonAsync<CloudinaryUploadResult>();
            if (uploadResult == null) return;

            // 3. Mettre à jour le serveur avec l'URL de la photo
            var succes = await _authService.UpdatePhotoAsync(uploadResult.SecureUrl);
            if (succes)
            {
                PhotoUrl = uploadResult.SecureUrl;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProfilViewModel] Erreur upload photo: {ex.Message}");
        }
        finally
        {
            EstEnChargementPhoto = false;
        }
    }

    [RelayCommand]
    public async Task DeconnexionAsync()
    {
        await _authService.LogoutAsync();
        Services.NavigationShell.AllerVers("LoginPage");
    }
}
