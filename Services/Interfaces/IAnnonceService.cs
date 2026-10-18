using LocatCam_mobile.Models;

namespace LocatCam_mobile.Services.Interfaces;

/// <summary>
/// Contrat du service métier pour les annonces immobilières.
/// </summary>
public interface IAnnonceService
{
    /// <summary>Récupère la liste paginée des annonces avec filtres.</summary>
    Task<(List<AnnonceDto> Items, int TotalCount)> GetAnnoncesAsync(
        int page = 1, int pageSize = 10,
        decimal? prixMin = null, decimal? prixMax = null,
        string? quartier = null, string? ville = null, string? type = null,
        bool? estMeuble = null, bool? disponible = null);

    /// <summary>Récupère le détail d'une annonce par son identifiant.</summary>
    Task<AnnonceDto?> GetAnnonceByIdAsync(string id);

    /// <summary>Récupère les annonces du bailleur connecté.</summary>
    Task<List<AnnonceDto>> GetMesAnnoncesAsync();

    /// <summary>Crée une nouvelle annonce (bailleur uniquement).</summary>
    Task<AnnonceDto?> CreateAnnonceAsync(AnnonceCreateDto dto);

    /// <summary>Met à jour une annonce existante.</summary>
    Task<AnnonceDto?> UpdateAnnonceAsync(string id, AnnonceUpdateDto dto);

    /// <summary>Supprime (soft delete) une annonce.</summary>
    Task<bool> DeleteAnnonceAsync(string id);

    /// <summary>Récupère les annonces dans une bounding box pour la carte.</summary>
    Task<List<AnnonceDto>> GetAnnoncesMapAsync(
        double minLat, double maxLat, double minLng, double maxLng);
}
