using LocatCam_mobile.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace LocatCam_mobile.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://169.58.69.36:5206/"),
            Timeout = TimeSpan.FromSeconds(30)
        };

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };
    }

    /// <summary>
    /// Définit le token Bearer pour les requêtes authentifiées.
    /// </summary>
    public void SetBearerToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    /// <summary>
    /// Connexion utilisateur. Retourne les tokens et les infos utilisateur.
    /// </summary>
    public async Task<LoginResponse?> LoginAsync(string email, string motDePasse)
    {
        var request = new LoginRequest { Email = email, MotDePasse = motDePasse };
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request, _jsonOptions);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
    }

    /// <summary>
    /// Récupère la liste paginée des annonces.
    /// </summary>
    public async Task<PagedResult<AnnonceDto>?> GetAnnoncesAsync(int page = 1, int pageSize = 10)
    {
        var url = $"api/annonces?page={page}&pageSize={pageSize}";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<PagedResult<AnnonceDto>>(_jsonOptions);
    }

    /// <summary>
    /// Récupère le détail d'une annonce par son identifiant.
    /// </summary>
    public async Task<AnnonceDto?> GetAnnonceAsync(string id)
    {
        var response = await _httpClient.GetAsync($"api/annonces/{id}");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<AnnonceDto>(_jsonOptions);
    }
}
