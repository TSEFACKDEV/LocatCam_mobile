using LocatCam_mobile.Models;
using LocatCam_mobile.Services.Interfaces;
using Refit;
using System.Collections.Generic;

namespace LocatCam_mobile.Services;

/// <summary>
/// Service métier pour les annonces immobilières.
/// Wrapper autour de l'API Refit avec gestion d'erreurs typées (Result pattern).
/// </summary>
public class AnnonceService : IAnnonceService
{
    private readonly IApiClient _apiClient;

    /// <summary>Dernier message d'erreur lisible (statut HTTP + corps de réponse).</summary>
    public string? DerniereErreur { get; private set; }

    public AnnonceService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<(List<AnnonceDto> Items, int TotalCount)> GetAnnoncesAsync(
        int page = 1, int pageSize = 10,
        decimal? prixMin = null, decimal? prixMax = null,
        string? quartier = null, string? ville = null, string? type = null,
        bool? estMeuble = null, bool? disponible = null)
    {
        try
        {
            var response = await _apiClient.GetAnnonces(
                prixMin, prixMax, quartier, ville, type, estMeuble, disponible, page, pageSize);

            if (!response.IsSuccessStatusCode || response.Content == null)
                return (new List<AnnonceDto>(), 0);

            var items = response.Content.Items?.ToList() ?? new List<AnnonceDto>();
            return (items, response.Content.TotalCount);
        }
        catch (ApiException ex)
        {
            // Erreur API (401, 403, 500, etc.) — loguer et retourner vide
            System.Diagnostics.Debug.WriteLine($"[AnnonceService] Erreur API {ex.StatusCode}: {ex.Message}");
            return (new List<AnnonceDto>(), 0);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AnnonceService] Erreur inattendue: {ex.Message}");
            return (new List<AnnonceDto>(), 0);
        }
    }

    public async Task<AnnonceDto?> GetAnnonceByIdAsync(string id)
    {
        try
        {
            var response = await _apiClient.GetAnnonceById(id);

            if (!response.IsSuccessStatusCode || response.Content == null)
                return null;

            return response.Content;
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AnnonceService] Erreur API {ex.StatusCode}: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AnnonceService] Erreur inattendue: {ex.Message}");
            return null;
        }
    }

    public async Task<List<AnnonceDto>> GetMesAnnoncesAsync()
    {
        try
        {
            var response = await _apiClient.GetMesAnnonces();

            if (!response.IsSuccessStatusCode || response.Content == null)
                return new List<AnnonceDto>();

            return response.Content;
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AnnonceService] Erreur API {ex.StatusCode}: {ex.Message}");
            return new List<AnnonceDto>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AnnonceService] Erreur inattendue: {ex.Message}");
            return new List<AnnonceDto>();
        }
    }

    public async Task<AnnonceDto?> CreateAnnonceAsync(AnnonceCreateDto dto)
    {
        DerniereErreur = null;
        try
        {
            var response = await _apiClient.CreateAnnonce(dto);

            if (!response.IsSuccessStatusCode || response.Content == null)
            {
                DerniereErreur = $"HTTP {(int)response.StatusCode} : {response.Error?.Content ?? response.StatusCode.ToString()}";
                System.Diagnostics.Debug.WriteLine($"[AnnonceService] Création annonce refusée {DerniereErreur}");
                return null;
            }

            return response.Content;
        }
        catch (ApiException ex)
        {
            DerniereErreur = $"HTTP {(int)ex.StatusCode} : {ex.Content}";
            System.Diagnostics.Debug.WriteLine($"[AnnonceService] Erreur API {ex.StatusCode}: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            DerniereErreur = ex.Message;
            System.Diagnostics.Debug.WriteLine($"[AnnonceService] Erreur inattendue: {ex.Message}");
            return null;
        }
    }

    public async Task<AnnonceDto?> UpdateAnnonceAsync(string id, AnnonceUpdateDto dto)
    {
        try
        {
            var response = await _apiClient.UpdateAnnonce(id, dto);

            if (!response.IsSuccessStatusCode || response.Content == null)
                return null;

            return response.Content;
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AnnonceService] Erreur API {ex.StatusCode}: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AnnonceService] Erreur inattendue: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DeleteAnnonceAsync(string id)
    {
        try
        {
            var response = await _apiClient.DeleteAnnonce(id);
            return response.IsSuccessStatusCode;
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AnnonceService] Erreur API {ex.StatusCode}: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AnnonceService] Erreur inattendue: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> AddPhotosAsync(string annonceId, List<PhotoCreateDto> photos)
    {
        DerniereErreur = null;
        try
        {
            var response = await _apiClient.AddPhotos(annonceId, photos);
            if (!response.IsSuccessStatusCode)
                DerniereErreur = $"HTTP {(int)response.StatusCode} : {response.Error?.Content}";
            return response.IsSuccessStatusCode;
        }
        catch (ApiException ex)
        {
            DerniereErreur = $"HTTP {(int)ex.StatusCode} : {ex.Content}";
            System.Diagnostics.Debug.WriteLine($"[AnnonceService] Erreur API {ex.StatusCode}: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AnnonceService] Erreur inattendue: {ex.Message}");
            return false;
        }
    }

    public async Task<PhotoSignatureDto?> GetSignatureAsync()
    {
        DerniereErreur = null;
        try
        {
            var response = await _apiClient.GetSignature();
            if (!response.IsSuccessStatusCode || response.Content == null)
            {
                DerniereErreur = $"HTTP {(int)response.StatusCode} : {response.Error?.Content}";
                return null;
            }
            return response.Content;
        }
        catch (ApiException ex)
        {
            DerniereErreur = $"HTTP {(int)ex.StatusCode} : {ex.Content}";
            System.Diagnostics.Debug.WriteLine($"[AnnonceService] Erreur signature: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            DerniereErreur = ex.Message;
            System.Diagnostics.Debug.WriteLine($"[AnnonceService] Erreur signature: {ex.Message}");
            return null;
        }
    }

    public async Task<List<AnnonceDto>> GetAnnoncesMapAsync(
        double minLat, double maxLat, double minLng, double maxLng)
    {
        try
        {
            var response = await _apiClient.GetAnnoncesMap(minLat, maxLat, minLng, maxLng);

            if (!response.IsSuccessStatusCode || response.Content == null)
                return new List<AnnonceDto>();

            return response.Content;
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AnnonceService] Erreur API {ex.StatusCode}: {ex.Message}");
            return new List<AnnonceDto>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AnnonceService] Erreur inattendue: {ex.Message}");
            return new List<AnnonceDto>();
        }
    }
}
