using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocatCam_mobile.Models;
using LocatCam_mobile.Services.Interfaces;

namespace LocatCam_mobile.ViewModels;

/// <summary>
/// ViewModel de la page détail d'une annonce.
/// </summary>
public partial class DetailAnnonceViewModel : ObservableObject
{
    private readonly IAnnonceService _annonceService;
    private readonly IFavoriService _favoriService;

    [ObservableProperty]
    private AnnonceDto? _annonce;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _estFavori;

    [ObservableProperty]
    private bool _estEnChargementFavori;

    [ObservableProperty]
    private bool _isEnChargementFavori;

    public DetailAnnonceViewModel(IAnnonceService annonceService, IFavoriService favoriService)
    {
        _annonceService = annonceService;
        _favoriService = favoriService;
    }

    [RelayCommand]
    public async Task ChargerAsync(string annonceId)
    {
        if (string.IsNullOrWhiteSpace(annonceId)) return;

        IsLoading = true;

        try
        {
            Annonce = await _annonceService.GetAnnonceByIdAsync(annonceId);

            if (Annonce != null)
            {
                EstFavori = await _favoriService.IsFavoriAsync(Annonce.Id.ToString());
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DetailAnnonceViewModel] Erreur: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ToggleFavoriAsync()
    {
        if (Annonce == null || IsEnChargementFavori) return;

        IsEnChargementFavori = true;
        EstEnChargementFavori = true;

        try
        {
            bool succes;
            if (EstFavori)
            {
                succes = await _favoriService.RemoveFavoriAsync(Annonce.Id.ToString());
            }
            else
            {
                succes = await _favoriService.AddFavoriAsync(Annonce.Id.ToString());
            }

            if (succes)
            {
                EstFavori = !EstFavori;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DetailAnnonceViewModel] Erreur ToggleFavori: {ex.Message}");
        }
        finally
        {
            EstEnChargementFavori = false;
            IsEnChargementFavori = false;
        }
    }
}
