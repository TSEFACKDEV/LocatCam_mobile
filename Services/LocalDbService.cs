using LocatCam_mobile.Models;
using LocatCam_mobile.Services.Interfaces;
using SQLite;
using System.Text.Json;

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
            DateAjout = DateTime.UtcNow,
            AnnonceJson = JsonSerializer.Serialize(annonce)
        });
    }

    public async Task RetirerFavoriLocalAsync(string annonceId)
    {
        await _db.ExecuteAsync("DELETE FROM FavoriLocal WHERE AnnonceId = ?", annonceId);
    }

    /// <inheritdoc/>
    public async Task SynchroniserFavorisLocauxAsync(IEnumerable<AnnonceDto> annonces)
    {
        var liste = annonces
            .Where(a => a.Id != Guid.Empty)
            .GroupBy(a => a.Id.ToString())
            .Select(g => g.First())
            .ToList();

        var idsServeur = liste.Select(a => a.Id.ToString()).ToHashSet();
        var locaux = await _db.Table<FavoriLocal>().ToListAsync();

        // Retirer du cache les favoris supprimés côté serveur. On utilise du
        // SQL explicite pour rester compatible avec les bases créées avant
        // l'ajout de la clé primaire sur AnnonceId.
        foreach (var local in locaux.Where(l => !idsServeur.Contains(l.AnnonceId)))
        {
            await _db.ExecuteAsync(
                "DELETE FROM FavoriLocal WHERE AnnonceId = ?", local.AnnonceId);
        }

        // Ajouter / rafraîchir les favoris du serveur.
        foreach (var annonce in liste)
        {
            var id = annonce.Id.ToString();
            var json = JsonSerializer.Serialize(annonce);
            var existant = locaux.FirstOrDefault(l => l.AnnonceId == id);

            if (existant == null)
            {
                await _db.InsertAsync(new FavoriLocal
                {
                    AnnonceId = id,
                    DateAjout = DateTime.UtcNow,
                    AnnonceJson = json
                });
            }
            else
            {
                await _db.ExecuteAsync(
                    "UPDATE FavoriLocal SET AnnonceJson = ? WHERE AnnonceId = ?",
                    json, id);
            }
        }
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
