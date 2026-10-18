using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocatCam_mobile.Models;
using LocatCam_mobile.Services.Interfaces;
using System.Collections.ObjectModel;

namespace LocatCam_mobile.ViewModels;

/// <summary>
/// ViewModel de la page d'accueil : liste des annonces avec filtres et pagination.
/// </summary>
public partial class HomeViewModel : ObservableObject
{
    private readonly IAnnonceService _annonceService;

    [ObservableProperty]
    private ObservableCollection<AnnonceDto> _annonces = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private bool _hasMoreItems = true;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private string _recherche = string.Empty;

    private int _page = 1;
    private const int PageSize = 10;

    public HomeViewModel(IAnnonceService annonceService)
    {
        _annonceService = annonceService;
    }

    [RelayCommand]
    public async Task ChargerAnnoncesAsync(bool reset = false)
    {
        if (IsLoading) return;

        if (reset)
        {
            _page = 1;
            HasMoreItems = true;
            Annonces.Clear();
        }

        IsLoading = true;
        IsRefreshing = true;

        try
        {
            var (items, totalCount) = await _annonceService.GetAnnoncesAsync(
                page: _page,
                pageSize: PageSize,
                ville: string.IsNullOrWhiteSpace(Recherche) ? null : Recherche);

            TotalCount = totalCount;

            foreach (var annonce in items)
            {
                Annonces.Add(annonce);
            }

            HasMoreItems = Annonces.Count < TotalCount;
            _page++;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HomeViewModel] Erreur: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    public async Task ChargerPlusAsync()
    {
        if (HasMoreItems && !IsLoading)
        {
            await ChargerAnnoncesAsync(reset: false);
        }
    }

    [RelayCommand]
    public async Task RafraichirAsync()
    {
        await ChargerAnnoncesAsync(reset: true);
    }

    [RelayCommand]
    public async Task RechercherAsync()
    {
        await ChargerAnnoncesAsync(reset: true);
    }

    /// <summary>
    /// Charge les données initiales au démarrage de la page.
    /// </summary>
    public async Task InitialiserAsync()
    {
        await ChargerAnnoncesAsync(reset: true);
    }
}
