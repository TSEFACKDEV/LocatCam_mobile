using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocatCam_mobile.Models;
using LocatCam_mobile.Services.Interfaces;
using Microsoft.Maui.Devices.Sensors;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LocatCam_mobile.ViewModels;

/// <summary>
/// ViewModel de la carte interactive (Phase E).
///
/// La carte est le point d'entrée principal pour découvrir les logements autour de soi :
///   - Géolocalisation automatique au démarrage ("Vous êtes ici")
///   - Cercle de rayon autour de l'utilisateur (500 m → 10 km)
///   - Marqueurs colorés selon la proximité (vert < 800 m, jaune < 2,5 km, rouge au-delà)
///   - Bottom sheet avec distance exacte depuis votre position
/// </summary>
public partial class MapViewModel : ObservableObject
{
    private readonly IAnnonceService _annonceService;

    public MapViewModel(IAnnonceService annonceService)
    {
        _annonceService = annonceService;
    }

    // ------------------------------------------------------------------
    // Position utilisateur ("Vous êtes ici")
    // ------------------------------------------------------------------

    [ObservableProperty]
    private double _currentLatitude;

    [ObservableProperty]
    private double _currentLongitude;

    [ObservableProperty]
    private bool _isLocating;

    [ObservableProperty]
    private bool _hasLocation;

    [ObservableProperty]
    private string _locationStatus = "Position non détectée";

    // ------------------------------------------------------------------
    // Rayon de recherche autour de l'utilisateur
    // ------------------------------------------------------------------

    /// <summary>Rayon actuel en mètres.</summary>
    [ObservableProperty]
    private int _userRadiusMeters = 2000;

    /// <summary>Options de rayon proposées à l'utilisateur.</summary>
    public ObservableCollection<RayonOption> RayonOptions { get; } = new()
    {
        new RayonOption { Label = "500 m", Meters = 500 },
        new RayonOption { Label = "1 km", Meters = 1000 },
        new RayonOption { Label = "2 km", Meters = 2000 },
        new RayonOption { Label = "5 km", Meters = 5000 },
        new RayonOption { Label = "10 km", Meters = 10000 }
    };

    /// <summary>Option de rayon actuellement sélectionnée.</summary>
    [ObservableProperty]
    private RayonOption? _selectedRayon;

    // ------------------------------------------------------------------
    // Annonces affichées sur la carte (avec distance)
    // ------------------------------------------------------------------

    /// <summary>Annonces retournées pour la zone courante, enrichies avec la distance.</summary>
    public List<CarteAnnonce> LastAnnounces { get; private set; } = new();

    /// <summary>Payload JSON injecté dans Leaflet (marqueurs + distances).</summary>
    [ObservableProperty]
    private string _markersJson = "[]";

    /// <summary>Nombre d'annonces affichées sur la carte.</summary>
    [ObservableProperty]
    private int _annoncesCount;

    /// <summary>URL de la photo principale de l'annonce sélectionnée (bottom sheet).</summary>
    [ObservableProperty]
    private string _mainPhotoUrl = string.Empty;

    /// <summary>Indique si le bottom sheet est ouvert.</summary>
    [ObservableProperty]
    private bool _isBottomSheetOpen;

    /// <summary>Indique une opération de chargement en cours.</summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>Message d'erreur à afficher (ex: carte indisponible hors-ligne).</summary>
    [ObservableProperty]
    private string _errorMessage = string.Empty;

    private CarteAnnonce? _selectedAnnonce;

    /// <summary>Annonce sélectionnée (ouverture du bottom sheet).</summary>
    public CarteAnnonce? SelectedAnnonce
    {
        get => _selectedAnnonce;
        set
        {
            if (SetProperty(ref _selectedAnnonce, value))
            {
                IsBottomSheetOpen = value != null;
                MainPhotoUrl = value?.Photos?.FirstOrDefault()?.UrlCloudinary ?? string.Empty;
            }
        }
    }

    // ------------------------------------------------------------------
    // Initialisation : géolocalisation automatique au démarrage
    // ------------------------------------------------------------------

    /// <summary>
    /// Détecte la position GPS de l'utilisateur, centre la carte sur elle
    /// et charge les annonces dans le rayon par défaut.
    /// À appeler dès l'affichage de la page carte.
    /// </summary>
    public async Task InitialiserPositionAsync()
    {
        IsLocating = true;
        LocationStatus = "Détection de votre position…";

        try
        {
            Location? location = await Geolocation.Default.GetLastKnownLocationAsync();

            if (location == null)
            {
                location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(10)
                });
            }

            if (location != null)
            {
                CurrentLatitude = location.Latitude;
                CurrentLongitude = location.Longitude;
                HasLocation = true;
                LocationStatus = $"Vous êtes ici ({location.Latitude.ToString("0.000000", CultureInfo.InvariantCulture)}, {location.Longitude.ToString("0.000000", CultureInfo.InvariantCulture)})";

                // Sélectionne le rayon par défaut (2 km)
                SelectedRayon = RayonOptions.FirstOrDefault(r => r.Meters == 2000) ?? RayonOptions.First();
                UserRadiusMeters = SelectedRayon.Meters;

                await ChargerAnnoncesAutourDeMoiAsync();
            }
            else
            {
                LocationStatus = "Position indisponible — carte centrée sur Douala";
                await ChargerZoneParDefautAsync();
            }
        }
        catch (FeatureNotSupportedException)
        {
            LocationStatus = "Géolocalisation non prise en charge";
            await ChargerZoneParDefautAsync();
        }
        catch (PermissionException)
        {
            LocationStatus = "Permission géolocalisation refusée";
            await ChargerZoneParDefautAsync();
        }
        catch (Exception ex)
        {
            LocationStatus = "Erreur de géolocalisation";
            System.Diagnostics.Debug.WriteLine($"[MapViewModel] Erreur géolocalisation: {ex.Message}");
            await ChargerZoneParDefautAsync();
        }
        finally
        {
            IsLocating = false;
        }
    }

    /// <summary>Recentre la carte sur l'utilisateur et recharge les annonces autour.</summary>
    [RelayCommand]
    public async Task CentrerSurMoiAsync()
    {
        if (!HasLocation)
        {
            await InitialiserPositionAsync();
            return;
        }

        // Notifier la page mobile de recentrer la WebView
        // (la page appelle InjectUserPositionAsync après ce command)
        await ChargerAnnoncesAutourDeMoiAsync();
    }

    /// <summary>Change le rayon de recherche et recharge les annonces.</summary>
    [RelayCommand]
    public async Task ChangerRayonAsync(object? parametre)
    {
        int meters;
        if (parametre is int intValue)
            meters = intValue;
        else if (parametre is string s && int.TryParse(s, out var parsed))
            meters = parsed;
        else
            return;

        UserRadiusMeters = meters;
        SelectedRayon = RayonOptions.FirstOrDefault(r => r.Meters == meters);
        await ChargerAnnoncesAutourDeMoiAsync();
    }

    // ------------------------------------------------------------------
    // Chargement des annonces autour de l'utilisateur
    // ------------------------------------------------------------------

    private async Task ChargerAnnoncesAutourDeMoiAsync()
    {
        if (!HasLocation) return;

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            // Construire une bounding box autour de la position utilisateur
            // 1 degré de latitude ≈ 111 000 mètres
            // 1 degré de longitude ≈ 111 000 * cos(latitude) mètres
            var latRad = CurrentLatitude * Math.PI / 180.0;
            var metersPerDegreeLng = 111000.0 * Math.Cos(latRad);

            var latDelta = UserRadiusMeters / 111000.0;
            var lngDelta = UserRadiusMeters / metersPerDegreeLng;

            var minLat = CurrentLatitude - latDelta;
            var maxLat = CurrentLatitude + latDelta;
            var minLng = CurrentLongitude - lngDelta;
            var maxLng = CurrentLongitude + lngDelta;

            await ChargerAnnoncesDansBoundsAsync(minLat, maxLat, minLng, maxLng);
        }
        catch (Exception ex)
        {
            LastAnnounces = new List<CarteAnnonce>();
            MarkersJson = "[]";
            AnnoncesCount = 0;
            ErrorMessage = "Carte indisponible hors-ligne";
            System.Diagnostics.Debug.WriteLine($"[MapViewModel] Erreur chargement annonces carte: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Charge les annonces dans une bounding box donnée et prépare le payload JSON.
    /// Utilisée quand l'utilisateur déplace la carte (bounds changent) ou pour le chargement initial.
    /// </summary>
    public async Task LoadAnnouncementsAsync(
        double minLat, double maxLat, double minLng, double maxLng)
    {
        await ChargerAnnoncesDansBoundsAsync(minLat, maxLat, minLng, maxLng);
    }

    private async Task ChargerAnnoncesDansBoundsAsync(
        double minLat, double maxLat, double minLng, double maxLng)
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var annonces = await _annonceService.GetAnnoncesMapAsync(minLat, maxLat, minLng, maxLng);
            var liste = annonces?.ToList() ?? new List<AnnonceDto>();

            var enrichies = liste.Select(a => EnrichirAvecDistance(a)).ToList();
            LastAnnounces = enrichies;

            MarkersJson = BuildMarkersJson(enrichies);
            AnnoncesCount = enrichies.Count(a => a.Latitude != 0 && a.Longitude != 0);
        }
        catch (Exception ex)
        {
            LastAnnounces = new List<CarteAnnonce>();
            MarkersJson = "[]";
            AnnoncesCount = 0;
            ErrorMessage = "Carte indisponible hors-ligne";
            System.Diagnostics.Debug.WriteLine($"[MapViewModel] Erreur chargement annonces carte: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ChargerZoneParDefautAsync()
    {
        // Fallback : Douala si pas de géolocalisation — on affiche les annonces sans distance
        try
        {
            var annonces = await _annonceService.GetAnnoncesMapAsync(3.5, 4.6, 9.2, 10.3);
            var liste = annonces?.ToList() ?? new List<AnnonceDto>();

            var enrichies = liste.Select(a =>
            {
                var carte = new CarteAnnonce
                {
                    Id = a.Id, Titre = a.Titre, Description = a.Description, Prix = a.Prix,
                    Type = a.Type, Quartier = a.Quartier, Ville = a.Ville, Adresse = a.Adresse,
                    Latitude = a.Latitude, Longitude = a.Longitude, EstMeuble = a.EstMeuble,
                    NombrePieces = a.NombrePieces, Disponible = a.Disponible, UserId = a.UserId,
                    NomBailleur = a.NomBailleur, TelephoneBailleur = a.TelephoneBailleur,
                    EstVerifie = a.EstVerifie, Statut = a.Statut, DateCreation = a.DateCreation,
                    Photos = a.Photos
                };
                // Pas de position utilisateur : distance inconnue
                carte.DistanceMeters = null;
                carte.DistanceTexte = null;
                return carte;
            }).ToList();

            LastAnnounces = enrichies;
            MarkersJson = BuildMarkersJson(enrichies);
            AnnoncesCount = enrichies.Count(a => a.Latitude != 0 && a.Longitude != 0);
        }
        catch (Exception ex)
        {
            LastAnnounces = new List<CarteAnnonce>();
            MarkersJson = "[]";
            AnnoncesCount = 0;
            ErrorMessage = "Carte indisponible hors-ligne";
            System.Diagnostics.Debug.WriteLine($"[MapViewModel] Erreur zone par défaut: {ex.Message}");
        }
    }

    /// <summary>Calcule la distance haversine entre l'utilisateur et l'annonce.</summary>
    private CarteAnnonce EnrichirAvecDistance(AnnonceDto a)
    {
        var carte = new CarteAnnonce
        {
            Id = a.Id,
            Titre = a.Titre,
            Description = a.Description,
            Prix = a.Prix,
            Type = a.Type,
            Quartier = a.Quartier,
            Ville = a.Ville,
            Adresse = a.Adresse,
            Latitude = a.Latitude,
            Longitude = a.Longitude,
            EstMeuble = a.EstMeuble,
            NombrePieces = a.NombrePieces,
            Disponible = a.Disponible,
            UserId = a.UserId,
            NomBailleur = a.NomBailleur,
            TelephoneBailleur = a.TelephoneBailleur,
            EstVerifie = a.EstVerifie,
            Statut = a.Statut,
            DateCreation = a.DateCreation,
            Photos = a.Photos
        };

        if (HasLocation && a.Latitude != 0 && a.Longitude != 0)
        {
            var dist = CalculerDistance(CurrentLatitude, CurrentLongitude, a.Latitude, a.Longitude);
            carte.DistanceMeters = dist;
            carte.DistanceTexte = FormaterDistance(dist);
        }

        return carte;
    }

    private static double CalculerDistance(double lat1, double lng1, double lat2, double lng2)
    {
        const double R = 6371000; // mètres
        var rad = Math.PI / 180.0;
        var dLat = (lat2 - lat1) * rad;
        var dLng = (lng2 - lng1) * rad;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * rad) * Math.Cos(lat2 * rad) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static string FormaterDistance(double meters)
    {
        if (meters < 1000) return Math.Round(meters) + " m";
        return (meters / 1000).ToString("0.0", CultureInfo.InvariantCulture) + " km";
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private string BuildMarkersJson(IEnumerable<CarteAnnonce> annonces)
    {
        var payload = annonces
            .Where(a => a.Latitude != 0 && a.Longitude != 0)
            .Select(a => new
            {
                id = a.Id,
                titre = a.Titre,
                prix = a.Prix,
                quartier = a.Quartier,
                ville = a.Ville,
                latitude = a.Latitude,
                longitude = a.Longitude,
                photo = a.Photos?.FirstOrDefault()?.UrlCloudinary ?? string.Empty,
                distanceMeters = a.DistanceMeters,
                distanceTexte = a.DistanceTexte
            });

        return JsonSerializer.Serialize(payload, _jsonOptions);
    }

    // ------------------------------------------------------------------
    // Bottom sheet actions
    // ------------------------------------------------------------------

    [RelayCommand]
    public void CloseBottomSheet()
    {
        SelectedAnnonce = null;
        IsBottomSheetOpen = false;
    }

    [RelayCommand]
    public async Task ViewDetailsAsync()
    {
        if (SelectedAnnonce == null) return;

        Services.AnnonceSelection.Annonce = SelectedAnnonce;
        Services.NavigationShell.AllerVersAvecRetour("DetailAnnoncePage", "CartePage");
        CloseBottomSheet();
    }
}

/// <summary>Option de rayon prédéfinie pour le filtre de proximité.</summary>
public class RayonOption
{
    public string Label { get; set; } = string.Empty;
    public int Meters { get; set; }
}
