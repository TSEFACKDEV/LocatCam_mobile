using LocatCam_mobile.Models;

namespace LocatCam_mobile.Services.Interfaces;

/// <summary>
/// Contrat du service métier pour les favoris.
/// Gère la synchronisation entre l'API et la base SQLite locale (mode hors-ligne).
/// </summary>
public interface IFavoriService
{
    /// <summary>Récupère les favoris de l'utilisateur connecté (depuis l'API ou le cache local).</summary>
    Task<List<AnnonceDto>> GetFavorisAsync();

    /// <summary>Ajoute une annonce aux favoris (API + SQLite).</summary>
    Task<bool> AddFavoriAsync(string annonceId);

    /// <summary>Retire une annonce des favoris (API + SQLite).</summary>
    Task<bool> RemoveFavoriAsync(string annonceId);

    /// <summary>Vérifie si une annonce est dans les favoris (cache local).</summary>
    Task<bool> IsFavoriAsync(string annonceId);
}
