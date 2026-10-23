using LocatCam_mobile.Models;
using SQLite;

namespace LocatCam_mobile.Services.Interfaces;

/// <summary>
/// Contrat du service de base de données locale SQLite (mode hors-ligne).
/// </summary>
public interface ILocalDbService
{
    /// <summary>Initialise la base de données et crée les tables si nécessaire.</summary>
    Task InitializeAsync();

    // --- Favoris locaux ---
    Task<List<FavoriLocal>> GetFavorisLocauxAsync();
    Task<bool> IsFavoriLocalAsync(string annonceId);
    Task AjouterFavoriLocalAsync(string annonceId, AnnonceDto annonce);
    Task RetirerFavoriLocalAsync(string annonceId);

    /// <summary>Aligne le cache local sur la liste de favoris renvoyée par l'API.</summary>
    Task SynchroniserFavorisLocauxAsync(IEnumerable<AnnonceDto> annonces);

    // --- Recherches récentes ---
    Task<List<RechercheRecente>> GetRecherchesRecentesAsync();
    Task AjouterRechercheRecenteAsync(string query, int nbResultats);
    Task SupprimerRechercheRecenteAsync(int id);
}

/// <summary>
/// Représente un favori stocké localement dans SQLite.
/// </summary>
public class FavoriLocal
{
    /// <summary>Identifiant de l'annonce : clé primaire du cache local.</summary>
    [PrimaryKey]
    public string AnnonceId { get; set; } = string.Empty;

    public DateTime DateAjout { get; set; } = DateTime.UtcNow;

    /// <summary>Annonce complète sérialisée pour l'affichage hors-ligne.</summary>
    public string? AnnonceJson { get; set; }
}

/// <summary>
/// Représente une recherche récente stockée localement dans SQLite.
/// </summary>
public class RechercheRecente
{
    public int Id { get; set; }
    public string Query { get; set; } = string.Empty;
    public int NbResultats { get; set; }
    public DateTime DateRecherche { get; set; } = DateTime.UtcNow;
}
