using LocatCam_mobile.Models;
using LocatCam_mobile.Services.Interfaces;
using SQLite;

namespace LocatCam_mobile.Services;

/// <summary>
/// Service de base de données locale SQLite pour le mode hors-ligne.
/// Gère les favoris locaux et les recherches récentes.
/// </summary>
public class LocalDbService : ILocalDbService, IDisposable
{
    private readonly SQLiteAsyncConnection _db;
    private bool _disposed;

    public LocalDbService(string dbPath)
    {
        _db = new SQLiteAsyncConnection(dbPath);
    }

    public async Task InitializeAsync()
    {
        await _db.CreateTableAsync<FavoriLocal>();
        await _db.CreateTableAsync<RechercheRecente>();
    }

    // --- Favoris locaux ---

    public async Task<List<FavoriLocal>> GetFavorisLocauxAsync()
    {
        return await _db.Table<FavoriLocal>().ToListAsync();
    }

    public async Task<bool> IsFavoriLocalAsync(string annonceId)
    {
        var count = await _db.ExecuteScalarAsync<long>(
            "SELECT COUNT(*) FROM FavoriLocal WHERE AnnonceId = ?", annonceId);
        return count > 0;
    }

    public async Task AjouterFavoriLocalAsync(string annonceId, AnnonceDto annonce)
    {
        var exist = await IsFavoriLocalAsync(annonceId);
        if (exist) return;

        await _db.InsertAsync(new FavoriLocal
        {
            AnnonceId = annonceId,
            DateAjout = DateTime.UtcNow
        });
    }

    public async Task RetirerFavoriLocalAsync(string annonceId)
    {
        await _db.ExecuteAsync("DELETE FROM FavoriLocal WHERE AnnonceId = ?", annonceId);
    }

    // --- Recherches récentes ---

    public async Task<List<RechercheRecente>> GetRecherchesRecentesAsync()
    {
        return await _db.Table<RechercheRecente>()
            .OrderByDescending(r => r.DateRecherche)
            .Take(20)
            .ToListAsync();
    }

    public async Task AjouterRechercheRecenteAsync(string query, int nbResultats)
    {
        await _db.InsertAsync(new RechercheRecente
        {
            Query = query,
            NbResultats = nbResultats,
            DateRecherche = DateTime.UtcNow
        });

        // Garder uniquement les 20 dernières recherches
        var toutes = await GetRecherchesRecentesAsync();
        if (toutes.Count > 20)
        {
            var aSupprimer = toutes.Skip(20).Select(r => r.Id).ToList();
            foreach (var id in aSupprimer)
            {
                await SupprimerRechercheRecenteAsync(id);
            }
        }
    }

    public async Task SupprimerRechercheRecenteAsync(int id)
    {
        await _db.DeleteAsync<RechercheRecente>(id);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _db.CloseAsync().Wait();
            _disposed = true;
        }
    }
}
