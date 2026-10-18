using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocatCam_mobile.Models;
using LocatCam_mobile.Services.Interfaces;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

namespace LocatCam_mobile.ViewModels;

/// <summary>
/// ViewModel de la carte interactive (Phase E).
/// Charge les annonces dans une bounding box via <see cref="IAnnonceService.GetAnnoncesMapAsync"/>
/// et prépare le payload JSON injecté dans Leaflet (C# &rarr; JS).
/// </summary>
public partial class MapViewModel : ObservableObject
{
    private readonly IAnnonceService _annonceService;

    public MapViewModel(IAnnonceService annonceService)
    {
        _annonceService = annonceService;
    }

    /// <summary>Annonces retournées par la dernière requête de bounding box.</summary>
    public List<AnnonceDto> LastAnnounces { get; private set; } = new();

    /// <summary>Payload JSON sérialisé à injecter dans <c>addMarkers(...)</c> côté JS.</summary>
    [ObservableProperty]
    private string _markersJson = "[]";

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

    private AnnonceDto? _selectedAnnonce;

    /// <summary>Annonce sélectionnée (ouverture du bottom sheet).</summary>
    public AnnonceDto? SelectedAnnonce
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

    /// <summary>
    /// Charge les annonces dans la bounding box donnée et prépare le payload JSON.
    /// Appelée depuis la page WebView (C# &rarr; JS).
    /// </summary>
    public async Task LoadAnnouncementsAsync(
        double minLat, double maxLat, double minLng, double maxLng)
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var annonces = await _annonceService.GetAnnoncesMapAsync(minLat, maxLat, minLng, maxLng);
            LastAnnounces = annonces ?? new List<AnnonceDto>();
            MarkersJson = BuildMarkersJson();
        }
        catch (Exception ex)
        {
            LastAnnounces = new List<AnnonceDto>();
            MarkersJson = "[]";
            ErrorMessage = "Carte indisponible hors-ligne";
            System.Diagnostics.Debug.WriteLine($"[MapViewModel] Erreur chargement annonces carte: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Ferme le bottom sheet et désélectionne l'annonce.</summary>
    [RelayCommand]
    public void CloseBottomSheet()
    {
        SelectedAnnonce = null;
        IsBottomSheetOpen = false;
    }

    /// <summary>Navigue vers la page détaillée de l'annonce sélectionnée.</summary>
    [RelayCommand]
    public async Task ViewDetailsAsync()
    {
        if (SelectedAnnonce == null) return;

        await Task.CompletedTask;
        Services.AnnonceSelection.Annonce = SelectedAnnonce;
        Services.NavigationShell.AllerVersAvecRetour("DetailAnnoncePage");
        CloseBottomSheet();
    }

    private string BuildMarkersJson()
    {
        var payload = LastAnnounces.Select(a => new
        {
            id = a.Id,
            titre = a.Titre,
            prix = a.Prix,
            quartier = a.Quartier,
            ville = a.Ville,
            latitude = a.Latitude,
            longitude = a.Longitude,
            photo = a.Photos?.FirstOrDefault()?.UrlCloudinary ?? string.Empty
        });

        return JsonSerializer.Serialize(payload);
    }
}
