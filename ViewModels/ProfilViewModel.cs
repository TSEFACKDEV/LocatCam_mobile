using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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

    [ObservableProperty]
    private string _messageErreur = string.Empty;

    [ObservableProperty]
    private string _messageSucces = string.Empty;

    /// <summary>Vrai si aucune photo n'est disponible : on affiche alors l'initiale.</summary>
    public bool AfficherInitiales => string.IsNullOrWhiteSpace(PhotoUrl);

    public ProfilViewModel(IAuthService authService, IAnnonceService annonceService)
    {
        _authService = authService;
        _annonceService = annonceService;
    }

    partial void OnPhotoUrlChanged(string value)
        => OnPropertyChanged(nameof(AfficherInitiales));

    [RelayCommand]
    public async Task ChargerProfilAsync()
    {
        IsLoading = true;
        MessageErreur = string.Empty;

        try
        {
            // Recharger depuis le serveur pour récupérer la photo à jour,
            // puis retomber sur le cache local si le serveur est injoignable.
            var user = await _authService.RafraichirProfilAsync();

            if (user != null)
            {
                Nom = user.Nom;
                Email = user.Email;
                Role = user.Role;
                Telephone = user.Telephone;
                PhotoUrl = user.PhotoUrl ?? string.Empty;
            }
            else
            {
                MessageErreur = "Impossible de charger votre profil. Reconnectez-vous.";
            }
        }
        catch (Exception ex)
        {
            MessageErreur = $"Erreur : {ex.Message}";
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
        MessageErreur = string.Empty;
        MessageSucces = string.Empty;

        try
        {
            var photo = await MediaPicker.PickPhotoAsync();
            if (photo == null) return;

            EstEnChargementPhoto = true;

            using var stream = await photo.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();

            // 1. Obtenir les paramètres de signature depuis l'API (nom du cloud inclus)
            var signature = await _annonceService.GetSignatureAsync();
            if (signature == null || string.IsNullOrWhiteSpace(signature.CloudName))
            {
                MessageErreur = "Impossible d'obtenir la signature Cloudinary. "
                    + (_annonceService.DerniereErreur ?? "Vérifiez votre connexion et reconnectez-vous.");
                return;
            }

            // 2. Uploader l'image sur Cloudinary avec les paramètres signés
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
            using var content = new MultipartFormDataContent();

            content.Add(new ByteArrayContent(bytes, 0, bytes.Length), "file", photo.FileName ?? "profile.jpg");
            content.Add(new StringContent(signature.Timestamp.ToString()), "timestamp");
            content.Add(new StringContent(signature.Folder), "folder");
            content.Add(new StringContent(signature.ApiKey), "api_key");

            // Ne jamais envoyer un preset vide : Cloudinary le refuserait.
            if (!string.IsNullOrWhiteSpace(signature.UploadPreset))
                content.Add(new StringContent(signature.UploadPreset), "upload_preset");

            content.Add(new StringContent(signature.Signature), "signature");

            var uploadUrl = $"https://api.cloudinary.com/v1_1/{signature.CloudName}/image/upload";
            var uploadResponse = await httpClient.PostAsync(uploadUrl, content);

            if (!uploadResponse.IsSuccessStatusCode)
            {
                var errorContent = await uploadResponse.Content.ReadAsStringAsync();
                MessageErreur = $"Échec de l'upload (HTTP {(int)uploadResponse.StatusCode}). {ExtraireMessageCloudinary(errorContent)}";
                System.Diagnostics.Debug.WriteLine($"[ProfilViewModel] Échec Cloudinary: {uploadResponse.StatusCode} - {errorContent}");
                return;
            }

            var uploadResult = await uploadResponse.Content.ReadFromJsonAsync<CloudinaryUploadResult>();
            if (uploadResult == null || string.IsNullOrWhiteSpace(uploadResult.SecureUrl))
            {
                MessageErreur = "Réponse Cloudinary invalide (URL de l'image absente).";
                return;
            }

            // 3. Enregistrer l'URL de la photo sur le serveur LocatCam
            var succes = await _authService.UpdatePhotoAsync(uploadResult.SecureUrl);
            if (!succes)
            {
                MessageErreur = "L'image a été envoyée, mais le serveur LocatCam n'a pas pu enregistrer la photo. Réessayez.";
                return;
            }

            PhotoUrl = uploadResult.SecureUrl;
            MessageSucces = "Photo de profil mise à jour avec succès !";

            // Mettre à jour l'avatar de la barre de navigation immédiatement.
            WeakReferenceMessenger.Default.Send(new PhotoProfilMessage(PhotoUrl));
        }
        catch (PermissionException)
        {
            MessageErreur = "Autorisez l'accès aux photos dans les réglages de l'application.";
        }
        catch (Exception ex)
        {
            MessageErreur = $"Erreur lors de l'upload : {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[ProfilViewModel] Erreur upload photo: {ex.Message}");
        }
        finally
        {
            EstEnChargementPhoto = false;
        }
    }

    /// <summary>Extrait le message lisible renvoyé par Cloudinary.</summary>
    private static string ExtraireMessageCloudinary(string json)
    {
        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty("error", out var error) &&
                error.TryGetProperty("message", out var message))
                return message.GetString() ?? json;
        }
        catch
        {
            // Réponse non JSON : on renvoie le corps brut.
        }

        return json;
    }

    [RelayCommand]
    public async Task DeconnexionAsync()
    {
        await _authService.LogoutAsync();
        Services.NavigationShell.AllerVers("LoginPage");
    }
}
