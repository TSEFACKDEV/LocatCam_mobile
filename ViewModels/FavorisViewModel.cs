using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocatCam_mobile.Models;
using LocatCam_mobile.Services.Interfaces;
using System.Collections.ObjectModel;

namespace LocatCam_mobile.ViewModels;

/// <summary>
/// ViewModel de la page des favoris.
/// </summary>
public partial class FavorisViewModel : ObservableObject
{
    private readonly IFavoriService _favoriService;

    [ObservableProperty]
    private ObservableCollection<AnnonceDto> _favoris = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private bool _estVide;

    public FavorisViewModel(IFavoriService favoriService)
    {
        _favoriService = favoriService;
    }

    [RelayCommand]
    public async Task ChargerFavorisAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        IsRefreshing = true;

        try
        {
            var liste = await _favoriService.GetFavorisAsync();

            Favoris.Clear();
            foreach (var favori in liste)
            {
                Favoris.Add(favori);
            }

            EstVide = Favoris.Count == 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FavorisViewModel] Erreur: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    public async Task SupprimerFavoriAsync(string annonceId)
    {
        var succes = await _favoriService.RemoveFavoriAsync(annonceId);
        if (succes)
        {
            var annonce = Favoris.FirstOrDefault(a => a.Id.ToString() == annonceId);
            if (annonce != null)
            {
                Favoris.Remove(annonce);
            }
            EstVide = Favoris.Count == 0;
        }
    }

    /// <summary>
    /// Charge les favoris au démarrage de la page.
    /// </summary>
    public async Task InitialiserAsync()
    {
        await ChargerFavorisAsync();
    }
}
