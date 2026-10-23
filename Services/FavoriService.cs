using LocatCam_mobile.Models;
using LocatCam_mobile.Services.Interfaces;
using Refit;
using System.Text.Json;

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

            if (!response.IsSuccessStatusCode || response.Content == null)
                return await GetFavorisLocauxAsync();

            // L'API renvoie des favoris contenant chacun l'annonce complète.
            var annonces = response.Content
                .Where(f => f.Annonce != null)
                .Select(f => f.Annonce!)
                .ToList();

            // Aligner le cache local (ajouts ET suppressions faites ailleurs).
            await _localDb.SynchroniserFavorisLocauxAsync(annonces);

            return annonces;
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FavoriService] Erreur API {ex.StatusCode}: {ex.Message}");
            return await GetFavorisLocauxAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FavoriService] Erreur inattendue: {ex.Message}");
            return await GetFavorisLocauxAsync();
        }
    }

    /// <inheritdoc/>
    public async Task SynchroniserFavorisAsync()
    {
        // GetFavorisAsync met à jour le cache SQLite ; le résultat n'est pas nécessaire ici.
        _ = await GetFavorisAsync();
    }

    public async Task<bool> AddFavoriAsync(string annonceId, AnnonceDto? annonce = null)
    {
        try
        {
            var response = await _apiClient.AddFavori(annonceId);

            if (response.IsSuccessStatusCode)
            {
                await MettreEnCacheAsync(annonceId, annonce);
                return true;
            }

            // L'API renvoie 400 quand l'annonce est déjà en favori : c'est un
            // succès idempotent (le cache local était simplement désynchronisé).
            if ((int)response.StatusCode == 400 &&
                (response.Error?.Content?.Contains("déjà", StringComparison.OrdinalIgnoreCase) ?? false))
            {
                await MettreEnCacheAsync(annonceId, annonce);
                return true;
            }

            return false;
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FavoriService] Erreur API {ex.StatusCode}: {ex.Message}");
            // En cas d'erreur réseau, on stocke quand même en local pour le mode hors-ligne.
            await MettreEnCacheAsync(annonceId, annonce);
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

            // Contournement anomalie M1 : accepter 200 et 204 comme succès.
            // Un 404 signifie que le favori n'existe déjà plus : succès idempotent.
            if (response.IsSuccessStatusCode || (int)response.StatusCode == 404)
            {
                await _localDb.RetirerFavoriLocalAsync(annonceId);
                return true;
            }

            return false;
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FavoriService] Erreur API {ex.StatusCode}: {ex.Message}");
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

    /// <summary>Enregistre un favori dans le cache local avec l'annonce complète.</summary>
    private async Task MettreEnCacheAsync(string annonceId, AnnonceDto? annonce)
    {
        if (annonce == null)
        {
            try
            {
                var detail = await _apiClient.GetAnnonceById(annonceId);
                if (detail.IsSuccessStatusCode && detail.Content != null)
                    annonce = detail.Content;
            }
            catch
            {
                // Cache dégradé : seul l'identifiant sera conservé.
            }
        }

        await _localDb.AjouterFavoriLocalAsync(
            annonceId,
            annonce ?? new AnnonceDto { Id = Guid.Parse(annonceId) });
    }

    /// <summary>Reconstruit les annonces favorites depuis le cache SQLite.</summary>
    private async Task<List<AnnonceDto>> GetFavorisLocauxAsync()
    {
        var favorisLocaux = await _localDb.GetFavorisLocauxAsync();
        var annonces = new List<AnnonceDto>();

        foreach (var favori in favorisLocaux)
        {
            if (string.IsNullOrWhiteSpace(favori.AnnonceJson))
                continue;

            try
            {
                var annonce = JsonSerializer.Deserialize<AnnonceDto>(favori.AnnonceJson);
                if (annonce != null)
                    annonces.Add(annonce);
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FavoriService] Cache favori illisible: {ex.Message}");
            }
        }

        return annonces;
    }
}
