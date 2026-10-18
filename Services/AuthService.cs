using LocatCam_mobile.Models;
using LocatCam_mobile.Services.Interfaces;
using System.Text.Json;

namespace LocatCam_mobile.Services;

/// <summary>
/// Service d'authentification — implémentation Phase B.
///
/// Gère le login, le register, le logout, le stockage des tokens dans
/// SecureStorage, et le refresh automatique du token (contournement anomalie B2).
/// </summary>
public class AuthService : IAuthService
{
    private readonly IApiClient _apiClient;
    private readonly TokenHandler _tokenHandler;

    private const string AccessTokenKey = "locatcam_access_token";
    private const string RefreshTokenKey = "locatcam_refresh_token";
    private const string UserKey = "locatcam_user";

    public AuthService(IApiClient apiClient, TokenHandler tokenHandler)
    {
        _apiClient = apiClient;
        _tokenHandler = tokenHandler;
    }

    /// <summary>
    /// Connecte un utilisateur et stocke les tokens dans SecureStorage.
    /// </summary>
    public async Task<LoginResponse?> LoginAsync(string email, string motDePasse)
    {
        try
        {
            var response = await _apiClient.Login(new LoginRequest
            {
                Email = email,
                MotDePasse = motDePasse
            });

            if (!response.IsSuccess || response.Content == null)
                return null;

            // Stocker les tokens
            await SecureStorage.SetAsync(AccessTokenKey, response.Content.AccessToken);
            await SecureStorage.SetAsync(RefreshTokenKey, response.Content.RefreshToken);

            // Injecter le token dans le handler
            _tokenHandler.SetAccessToken(response.Content.AccessToken);

            // Stocker l'utilisateur sérialisé
            var userJson = JsonSerializer.Serialize(response.Content.User);
            await SecureStorage.SetAsync(UserKey, userJson);

            return response.Content;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Inscrit un nouvel utilisateur.
    /// </summary>
    public async Task<bool> RegisterAsync(string nom, string email, string telephone, string motDePasse, string role)
    {
        try
        {
            var response = await _apiClient.Register(new RegisterRequest
            {
                Nom = nom,
                Email = email,
                Telephone = telephone,
                MotDePasse = motDePasse,
                Role = role
            });

            return response.IsSuccess;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Déconnecte l'utilisateur et supprime les tokens du SecureStorage.
    /// </summary>
    public async Task LogoutAsync()
    {
        SecureStorage.Remove(AccessTokenKey);
        SecureStorage.Remove(RefreshTokenKey);
        SecureStorage.Remove(UserKey);
        _tokenHandler.SetAccessToken(null);
    }

    /// <summary>
    /// Indique si un utilisateur est actuellement connecté (token présent).
    /// </summary>
    public Task<bool> IsLoggedInAsync()
    {
        var token = SecureStorage.GetAsync(AccessTokenKey);
        return Task.FromResult(!string.IsNullOrWhiteSpace(token.Result));
    }

    /// <summary>
    /// Récupère le profil de l'utilisateur connecté depuis le SecureStorage.
    /// </summary>
    public Task<UserDto?> GetCurrentUserAsync()
    {
        try
        {
            var userJson = SecureStorage.GetAsync(UserKey).Result;
            if (string.IsNullOrWhiteSpace(userJson))
                return Task.FromResult<UserDto?>(null);

            var user = JsonSerializer.Deserialize<UserDto>(userJson);
            return Task.FromResult(user);
        }
        catch
        {
            return Task.FromResult<UserDto?>(null);
        }
    }

    /// <summary>
    /// Renouvelle l'access token via le refresh token.
    /// Retourne true si le renouvellement a réussi, false sinon.
    /// </summary>
    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var refreshToken = SecureStorage.GetAsync(RefreshTokenKey).Result;
            if (string.IsNullOrWhiteSpace(refreshToken))
                return false;

            var response = await _apiClient.Refresh(refreshToken);

            if (!response.IsSuccess || response.Content == null)
                return false;

            // Mettre à jour les tokens
            await SecureStorage.SetAsync(AccessTokenKey, response.Content.AccessToken);
            await SecureStorage.SetAsync(RefreshTokenKey, response.Content.RefreshToken);

            _tokenHandler.SetAccessToken(response.Content.AccessToken);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
