using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocatCam_mobile.Models;
using LocatCam_mobile.Services.Interfaces;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace LocatCam_mobile.ViewModels;

/// <summary>
/// Étapes du formulaire de publication d'annonce (Phase F).
/// </summary>
public enum EtapePublier
{
    Infos = 0,      // titre, description, prix, type, quartier, ville, adresse
    Localisation,   // latitude / longitude (saisie ou carte)
    Options,        // meublé, nombre de pièces, disponible
    Photos,         // DÉSACTIVÉES — contournement anomalie B1
    Apercu,         // récapitulatif avant publication
    Termine         // annonce publiée avec succès
}

/// <summary>
/// ViewModel de la page de publication d'annonce (Phase F).
/// Formule étape par étape. L'upload de photos est désactivé tant que
/// l'anomalie API B1 (POST /api/photos/signature sans auth) n'est pas corrigée.
/// </summary>
public partial class PublierViewModel : ObservableObject
{
    private readonly IAnnonceService _annonceService;
    private readonly IFavoriService _favoriService;

    public PublierViewModel(IAnnonceService annonceService, IFavoriService favoriService)
    {
        _annonceService = annonceService;
        _favoriService = favoriService;
    }

    // ------------------------------------------------------------------
    // État du formulaire
    // ------------------------------------------------------------------

    [ObservableProperty]
    private EtapePublier _etapeCourante = EtapePublier.Infos;

    [ObservableProperty]
    private string _titre = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _prixTexte = string.Empty;

    [ObservableProperty]
    private string _type = string.Empty;

    [ObservableProperty]
    private string _quartier = string.Empty;

    [ObservableProperty]
    private string _ville = string.Empty;

    [ObservableProperty]
    private string _adresse = string.Empty;

    [ObservableProperty]
    private double _latitude;

    [ObservableProperty]
    private double _longitude;

    [ObservableProperty]
    private string _latitudeTexte = string.Empty;

    [ObservableProperty]
    private string _longitudeTexte = string.Empty;

    [ObservableProperty]
    private bool _estMeuble;

    [ObservableProperty]
    private int _nombrePieces = 1;

    [ObservableProperty]
    private string _nombrePiecesTexte = "1";

    [ObservableProperty]
    private bool _disponible = true;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _messageErreur = string.Empty;

    [ObservableProperty]
    private string _messageSucces = string.Empty;

    [ObservableProperty]
    private AnnonceDto? _annonceCreee;

    /// <summary>Images sélectionnées par l'utilisateur (miniatures affichées).</summary>
    [ObservableProperty]
    private ObservableCollection<ImageSource> _photosSelectionnees = new();

    /// <summary>Fichiers bruts sélectionnés via MediaPicker (pour l'upload).</summary>
    private List<FileResult> _fichiersSelectionnes = new();

    /// <summary>Indique si au moins une photo est sélectionnée.</summary>
    public bool ADesPhotos => PhotosSelectionnees.Count > 0;

    /// <summary>Libre du nombre de photos sélectionnées.</summary>
    public string NbPhotosTexte => $"{PhotosSelectionnees.Count} photo(s) sélectionnée(s)";

    /// <summary>Nom du cloud Cloudinary (doit correspondre à la configuration serveur).</summary>
    private const string CloudinaryCloudName = "locatcam";

    /// <summary>Types de bien proposés.</summary>
    public ObservableCollection<string> TypesBien { get; } = new()
    {
        "Appartement", "Maison", "Studio", "Chambre"
    };

    // ------------------------------------------------------------------
    // Visibilité des étapes (pour le XAML)
    // ------------------------------------------------------------------

    public bool EstEtapeInfos => EtapeCourante == EtapePublier.Infos;
    public bool EstEtapeLocalisation => EtapeCourante == EtapePublier.Localisation;
    public bool EstEtapeOptions => EtapeCourante == EtapePublier.Options;
    public bool EstEtapePhotos => EtapeCourante == EtapePublier.Photos;
    public bool EstEtapeApercu => EtapeCourante == EtapePublier.Apercu;
    public bool EstEtapeTermine => EtapeCourante == EtapePublier.Termine;

    /// <summary>Libellé de progression affiché dans l'en-tête ("Étape 2 / 5").</summary>
    public string EtapeTexte => EtapeCourante == EtapePublier.Termine
        ? "Terminé"
        : $"Étape {(int)EtapeCourante + 1} / 5";

    private void NotifierEtapes()
    {
        OnPropertyChanged(nameof(EstEtapeInfos));
        OnPropertyChanged(nameof(EstEtapeLocalisation));
        OnPropertyChanged(nameof(EstEtapeOptions));
        OnPropertyChanged(nameof(EstEtapePhotos));
        OnPropertyChanged(nameof(EstEtapeApercu));
        OnPropertyChanged(nameof(EstEtapeTermine));
        OnPropertyChanged(nameof(EtapeTexte));
    }

    // ------------------------------------------------------------------
    // Navigation entre étapes
    // ------------------------------------------------------------------

    [RelayCommand]
    private void Suivant()
    {
        MessageErreur = string.Empty;

        // Validation de l'étape courante avant d'avancer
        if (!ValiderEtapeCourante())
            return;

        if (EtapeCourante < EtapePublier.Termine)
        {
            EtapeCourante++;
            NotifierEtapes();
        }
    }

    [RelayCommand]
    private void Precedent()
    {
        MessageErreur = string.Empty;
        if (EtapeCourante > EtapePublier.Infos)
        {
            EtapeCourante--;
            NotifierEtapes();
        }
    }

    /// <summary>Récupère la position GPS actuelle et pré-remplit latitude/longitude.</summary>
    [RelayCommand]
    private async Task MaPositionAsync()
    {
        MessageErreur = string.Empty;
        MessageSucces = string.Empty;
        try
        {
            Location? location = await Geolocation.Default.GetLastKnownLocationAsync();

            if (location == null)
            {
                // Aucune position connue : tenter une nouvelle acquisition avec timeout
                location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(10)
                });
            }

            if (location != null)
            {
                Latitude = location.Latitude;
                Longitude = location.Longitude;
                LatitudeTexte = Latitude.ToString(CultureInfo.InvariantCulture);
                LongitudeTexte = Longitude.ToString(CultureInfo.InvariantCulture);
                MessageSucces = "Position actuelle détectée et enregistrée.";
            }
            else
            {
                MessageErreur = "Impossible de détecter votre position. Saisissez manuellement les coordonnées.";
            }
        }
        catch (FeatureNotSupportedException)
        {
            MessageErreur = "La géolocalisation n'est pas prise en charge sur cet appareil.";
        }
        catch (PermissionException)
        {
            MessageErreur = "Permission de géolocalisation refusée. Saisissez manuellement les coordonnées.";
        }
        catch (Exception ex)
        {
            MessageErreur = "Erreur de géolocalisation : " + ex.Message;
            System.Diagnostics.Debug.WriteLine($"[PublierViewModel] Erreur géolocalisation: {ex.Message}");
        }
    }

    /// <summary>Ouvre la galerie pour sélectionner des photos du logement.</summary>
    [RelayCommand]
    private async Task SelectionnerPhotosAsync()
    {
        MessageErreur = string.Empty;
        try
        {
            const int MaxPhotos = 6;
            var placesRestantes = MaxPhotos - _fichiersSelectionnes.Count;
            if (placesRestantes <= 0)
            {
                MessageErreur = $"Vous avez déjà sélectionné le maximum de {MaxPhotos} photos.";
                return;
            }

            var selection = await MediaPicker.PickPhotosAsync();
            if (selection == null || !selection.Any()) return;

            foreach (var file in selection.Take(placesRestantes))
            {
                if (_fichiersSelectionnes.Count >= MaxPhotos) break;
                _fichiersSelectionnes.Add(file);
                await using var stream = await file.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var bytes = ms.ToArray();
                PhotosSelectionnees.Add(ImageSource.FromStream(() => new MemoryStream(bytes)));
            }

            OnPropertyChanged(nameof(ADesPhotos));
            OnPropertyChanged(nameof(NbPhotosTexte));
        }
        catch (Exception ex)
        {
            MessageErreur = "Impossible de sélectionner les photos : " + ex.Message;
            System.Diagnostics.Debug.WriteLine($"[PublierViewModel] Erreur sélection photos: {ex.Message}");
        }
    }

    /// <summary>Supprime une photo sélectionnée (index).</summary>
    [RelayCommand]
    private void SupprimerPhoto(int index)
    {
        if (index < 0 || index >= _fichiersSelectionnes.Count) return;
        _fichiersSelectionnes.RemoveAt(index);
        PhotosSelectionnees.RemoveAt(index);
        OnPropertyChanged(nameof(ADesPhotos));
        OnPropertyChanged(nameof(NbPhotosTexte));
    }

    /// <summary>
    /// Upload les photos sélectionnées vers Cloudinary (signature serveur) puis les déclare
    /// auprès de l'API liée à l'annonce.
    /// </summary>
    private async Task UploaderPhotosAsync(string annonceId)
    {
        if (_fichiersSelectionnes.Count == 0) return;

        try
        {
            // 1. Obtenir la signature Cloudinary depuis l'API (nécessite un token valide)
            var signature = await _annonceService.GetSignatureAsync();
            if (signature == null)
            {
                System.Diagnostics.Debug.WriteLine("[PublierViewModel] Impossible d'obtenir la signature Cloudinary.");
                return;
            }

            // 2. Uploader chaque image sur Cloudinary
            var photosADclarer = new List<PhotoCreateDto>();
            int ordre = 0;

            foreach (var file in _fichiersSelectionnes)
            {
                using var stream = await file.OpenReadAsync();
                using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
                using var content = new MultipartFormDataContent();

                content.Add(new StreamContent(stream), "file", file.FileName ?? "image.jpg");
                content.Add(new StringContent(signature.Timestamp.ToString()), "timestamp");
                content.Add(new StringContent(signature.Folder), "folder");
                content.Add(new StringContent(signature.UploadPreset), "upload_preset");
                content.Add(new StringContent(signature.Signature), "signature");

                var uploadUrl = $"https://api.cloudinary.com/v1_1/{CloudinaryCloudName}/image/upload";
                var uploadResponse = await httpClient.PostAsync(uploadUrl, content);

                if (!uploadResponse.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[PublierViewModel] Échec upload Cloudinary: {uploadResponse.StatusCode}");
                    continue;
                }

                var uploadResult = await uploadResponse.Content.ReadFromJsonAsync<CloudinaryUploadResult>();
                if (uploadResult == null) continue;

                photosADclarer.Add(new PhotoCreateDto
                {
                    AnnonceId = annonceId,
                    UrlCloudinary = uploadResult.SecureUrl,
                    PublicId = uploadResult.PublicId,
                    Ordre = ordre++,
                    EstPrincipale = ordre == 1
                });
            }

            // 3. Déclarer les photos auprès de l'API
            if (photosADclarer.Count > 0)
            {
                await _annonceService.AddPhotosAsync(annonceId, photosADclarer);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PublierViewModel] Erreur upload photos: {ex.Message}");
        }
    }

    /// <summary>Valide les champs de l'étape courante.</summary>
    private bool ValiderEtapeCourante()
    {
        switch (EtapeCourante)
        {
            case EtapePublier.Infos:
                if (string.IsNullOrWhiteSpace(Titre))
                {
                    MessageErreur = "Le titre est requis.";
                    return false;
                }
                if (Titre.Length < 5)
                {
                    MessageErreur = "Le titre doit contenir au moins 5 caractères.";
                    return false;
                }
                if (string.IsNullOrWhiteSpace(Description))
                {
                    MessageErreur = "La description est requise.";
                    return false;
                }
                if (Description.Length < 10)
                {
                    MessageErreur = "La description doit contenir au moins 10 caractères.";
                    return false;
                }
                if (!EssayerLireDecimal(PrixTexte, out var prix) || prix <= 0)
                {
                    MessageErreur = "Le prix doit être un nombre supérieur à 0.";
                    return false;
                }
                if (string.IsNullOrWhiteSpace(Type))
                {
                    MessageErreur = "Le type de bien est requis.";
                    return false;
                }
                if (string.IsNullOrWhiteSpace(Quartier))
                {
                    MessageErreur = "Le quartier est requis.";
                    return false;
                }
                if (string.IsNullOrWhiteSpace(Ville))
                {
                    MessageErreur = "La ville est requise.";
                    return false;
                }
                if (string.IsNullOrWhiteSpace(Adresse))
                {
                    MessageErreur = "L'adresse est requise.";
                    return false;
                }
                break;

            case EtapePublier.Localisation:
                if (!EssayerLireDouble(LatitudeTexte, out var latitude) ||
                    !EssayerLireDouble(LongitudeTexte, out var longitude))
                {
                    MessageErreur = "Indiquez une latitude et une longitude valides (ex : 3.8480 et 11.5021).";
                    return false;
                }
                if (latitude is < -90 or > 90 || longitude is < -180 or > 180)
                {
                    MessageErreur = "Les coordonnées sont hors limites (latitude -90 à 90, longitude -180 à 180).";
                    return false;
                }
                if (latitude == 0 && longitude == 0)
                {
                    MessageErreur = "Veuillez indiquer une localisation (latitude / longitude).";
                    return false;
                }

                Latitude = latitude;
                Longitude = longitude;
                break;

            case EtapePublier.Options:
                if (!int.TryParse(NombrePiecesTexte?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var pieces) ||
                    pieces < 1)
                {
                    MessageErreur = "Le nombre de pièces doit être d'au moins 1.";
                    return false;
                }

                NombrePieces = pieces;
                break;

            case EtapePublier.Photos:
                // Photos désactivées (contournement B1) — on passe directement
                break;
        }

        return true;
    }

    // ------------------------------------------------------------------
    // Publication
    // ------------------------------------------------------------------

    [RelayCommand]
    private async Task PublierAsync()
    {
        MessageErreur = string.Empty;
        MessageSucces = string.Empty;

        if (!ValiderEtapeCourante())
            return;

        IsLoading = true;

        try
        {
            var dto = new AnnonceCreateDto
            {
                Titre = Titre.Trim(),
                Description = Description.Trim(),
                Prix = Prix,
                Type = Type,
                Quartier = Quartier.Trim(),
                Ville = Ville.Trim(),
                Adresse = Adresse.Trim(),
                Latitude = Latitude,
                Longitude = Longitude,
                EstMeuble = EstMeuble,
                NombrePieces = NombrePieces,
                Disponible = Disponible
            };

            var annonce = await _annonceService.CreateAnnonceAsync(dto);

            if (annonce == null)
            {
                MessageErreur = "Impossible de publier l'annonce. Réessayez plus tard.";
                return;
            }

            // Upload des photos sur Cloudinary puis déclaration auprès de l'API
            await UploaderPhotosAsync(annonce.Id.ToString());

            AnnonceCreee = annonce;
            EtapeCourante = EtapePublier.Termine;
            NotifierEtapes();

            MessageSucces = "Votre annonce a été publiée avec succès !";
        }
        catch (Exception ex)
        {
            MessageErreur = "Erreur lors de la publication : " + ex.Message;
            System.Diagnostics.Debug.WriteLine($"[PublierViewModel] Erreur: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Réinitialise le formulaire pour une nouvelle publication.</summary>
    [RelayCommand]
    private void NouvelleAnnonce()
    {
        Titre = string.Empty;
        Description = string.Empty;
        PrixTexte = string.Empty;
        Type = string.Empty;
        Quartier = string.Empty;
        Ville = string.Empty;
        Adresse = string.Empty;
        Latitude = 0;
        Longitude = 0;
        LatitudeTexte = string.Empty;
        LongitudeTexte = string.Empty;
        EstMeuble = false;
        NombrePieces = 1;
        NombrePiecesTexte = "1";
        Disponible = true;
        MessageErreur = string.Empty;
        MessageSucces = string.Empty;
        AnnonceCreee = null;
        _fichiersSelectionnes.Clear();
        PhotosSelectionnees.Clear();
        OnPropertyChanged(nameof(ADesPhotos));
        OnPropertyChanged(nameof(NbPhotosTexte));
        EtapeCourante = EtapePublier.Infos;
        NotifierEtapes();
    }

    /// <summary>Prix saisi (propriété calculée utilisée pour l'aperçu et l'envoi).</summary>
    private decimal Prix => EssayerLireDecimal(PrixTexte, out var valeur) ? valeur : 0m;

    /// <summary>
    /// Lit un nombre saisi au format français ou invariant
    /// (espaces de milliers, virgule décimale).
    /// </summary>
    private static bool EssayerLireDecimal(string? texte, out decimal valeur)
    {
        valeur = 0m;
        if (string.IsNullOrWhiteSpace(texte))
            return false;

        var nettoye = texte
            .Replace("\u00A0", string.Empty)
            .Replace(" ", string.Empty)
            .Replace(",", ".")
            .Trim();

        return decimal.TryParse(nettoye, NumberStyles.Number, CultureInfo.InvariantCulture, out valeur);
    }

    private static bool EssayerLireDouble(string? texte, out double valeur)
    {
        valeur = 0d;
        if (string.IsNullOrWhiteSpace(texte))
            return false;

        var nettoye = texte
            .Replace("\u00A0", string.Empty)
            .Replace(" ", string.Empty)
            .Replace(",", ".")
            .Trim();

        return double.TryParse(nettoye, NumberStyles.Float, CultureInfo.InvariantCulture, out valeur);
    }
}
