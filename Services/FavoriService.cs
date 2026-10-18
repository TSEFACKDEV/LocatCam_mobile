using LocatCam_mobile.Models;
using LocatCam_mobile.Services.Interfaces;
using Refit;
using SQLite;

namespace LocatCam_mobile.Services;

/// <summary>
/// Service métier pour les favoris.
/// Synchronise l'état des favoris entre l'API et la base SQLite locale (mode hors-ligne).
/// Contournement anomalie M1 : accepte 200 ET 204 comme succès pour DELETE.
/// </summary>
public class FavoriService : IFavoriService
{
    private readonly IApiClient _apiClient;
    private readonly ILocalDbService _localDb;

    public FavoriService(IApiClient apiClient, ILocalDbService localDb)
    {
        _apiClient = apiClient;
        _localDb = localDb;
    }

    public async Task<List<AnnonceDto>> GetFavorisAsync()
    {
        try
        {
            var response = await _apiClient.GetFavoris();

            if (!response.IsSuccess || response.Content == null)
            {
                // Hors-ligne ou erreur : retour du cache SQLite
                var favorisLocaux = await _localDb.GetFavorisLocauxAsync();
                return favorisLocaux.Select(f => new AnnonceDto { Id = Guid.Parse(f.AnnonceId) }).ToList();
            }

            // Mettre à jour le cache local avec les IDs de favoris serveur
            var annonces = response.Content;
            foreach (var annonce in annonces)
            {
                var estDejaLocal = await _localDb.IsFavoriLocalAsync(annonce.Id.ToString());
                if (!estDejaLocal)
                {
                    await _localDb.AjouterFavoriLocalAsync(annonce.Id.ToString(), annonce);
                }
            }

            return annonces;
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FavoriService] Erreur API {ex.StatusCode}: {ex.Message}");
            // Fallback sur le cache local
            var favorisLocaux = await _localDb.GetFavorisLocauxAsync();
            return favorisLocaux.Select(f => new AnnonceDto { Id = Guid.Parse(f.AnnonceId) }).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FavoriService] Erreur inattendue: {ex.Message}");
            return new List<AnnonceDto>();
        }
    }

    public async Task<bool> AddFavoriAsync(string annonceId)
    {
        try
        {
            var response = await _apiClient.AddFavori(annonceId);

            if (response.IsSuccess)
            {
                // Mise à jour du cache local
                var estDejaLocal = await _localDb.IsFavoriLocalAsync(annonceId);
                if (!estDejaLocal)
                {
                    // Récupérer l'annonce pour la stocker localement (simplifié)
                    await _localDb.AjouterFavoriLocalAsync(annonceId, new AnnonceDto { Id = Guid.Parse(annonceId) });
                }
                return true;
            }

            return false;
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FavoriService] Erreur API {ex.StatusCode}: {ex.Message}");
            // En cas d'erreur réseau, on stocke quand même en local pour mode hors-ligne
            await _localDb.AjouterFavoriLocalAsync(annonceId, new AnnonceDto { Id = Guid.Parse(annonceId) });
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FavoriService] Erreur inattendue: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> RemoveFavoriAsync(string annonceId)
    {
        try
        {
            var response = await _apiClient.RemoveFavori(annonceId);

            // Contournement anomalie M1 : accepter 200 et 204 comme succès
            if (response.IsSuccess)
            {
                await _localDb.RetirerFavoriLocalAsync(annonceId);
                return true;
            }

            return false;
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FavoriService] Erreur API {ex.StatusCode}: {ex.Message}");
            // En cas d'erreur réseau, on retire quand même du cache local
            await _localDb.RetirerFavoriLocalAsync(annonceId);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FavoriService] Erreur inattendue: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> IsFavoriAsync(string annonceId)
    {
        try
        {
            return await _localDb.IsFavoriLocalAsync(annonceId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FavoriService] Erreur IsFavoriAsync: {ex.Message}");
            return false;
        }
    }
}
