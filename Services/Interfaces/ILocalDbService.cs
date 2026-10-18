using LocatCam_mobile.Models;

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
    public int Id { get; set; }
    public string AnnonceId { get; set; } = string.Empty;
    public DateTime DateAjout { get; set; } = DateTime.UtcNow;
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
