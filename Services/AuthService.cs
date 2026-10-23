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
    private const string SessionActiveKey = "locatcam_session_active";

    public string? DerniereErreur { get; private set; }

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
        DerniereErreur = null;

        try
        {
            var response = await _apiClient.Login(new LoginRequest
            {
                Email = email,
                MotDePasse = motDePasse
            });

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                // Stocker les tokens
                await SecureStorage.SetAsync(AccessTokenKey, response.Content.AccessToken);
                await SecureStorage.SetAsync(RefreshTokenKey, response.Content.RefreshToken);
                Preferences.Set(SessionActiveKey, true);

                // Injecter le token dans le handler
                _tokenHandler.SetAccessToken(response.Content.AccessToken);

                // Stocker l'utilisateur sérialisé
                var userJson = JsonSerializer.Serialize(response.Content.User);
                await SecureStorage.SetAsync(UserKey, userJson);

                return response.Content;
            }

            DerniereErreur = (int)response.StatusCode switch
            {
                401 => "Email ou mot de passe incorrect.",
                429 => "Trop de tentatives de connexion. Réessayez dans une minute.",
                _ => $"Le serveur LocatCam est indisponible ({(int)response.StatusCode}). Réessayez plus tard."
            };

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthService] Login: {ex.Message}");
            DerniereErreur = "Impossible de joindre le serveur LocatCam. Vérifiez votre connexion Internet.";
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

            return response.IsSuccessStatusCode;
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
        Preferences.Set(SessionActiveKey, false);
        _tokenHandler.SetAccessToken(null);
    }

    /// <summary>
    /// Indique si un utilisateur est actuellement connecté (token présent).
    /// </summary>
    public async Task<bool> IsLoggedInAsync()
    {
        // Drapeau non sensible : évite d'interroger le keystore Android à chaque
        // démarrage (SecureStorage peut être lent au premier accès).
        if (!Preferences.Get(SessionActiveKey, false))
            return false;

        try
        {
            var token = await SecureStorage.GetAsync(AccessTokenKey);
            if (string.IsNullOrWhiteSpace(token))
            {
                Preferences.Set(SessionActiveKey, false);
                return false;
            }

            // Restaurer le Bearer token dans le handler après un redémarrage :
            // sinon les appels authentifiés (favoris, messages, profil) partent
            // sans Authorization et l'API répond 401.
            _tokenHandler.SetAccessToken(token);

            return true;
        }
        catch
        {
            // SecureStorage peut être indisponible (keystore réinitialisé, etc.).
            return false;
        }
    }

    /// <summary>
    /// Récupère le profil de l'utilisateur connecté depuis le SecureStorage.
    /// </summary>
    public async Task<UserDto?> GetCurrentUserAsync()
    {
        try
        {
            var userJson = await SecureStorage.GetAsync(UserKey);
            if (string.IsNullOrWhiteSpace(userJson))
                return null;

            return JsonSerializer.Deserialize<UserDto>(userJson);
        }
        catch
        {
            return null;
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
            var refreshToken = await SecureStorage.GetAsync(RefreshTokenKey);
            if (string.IsNullOrWhiteSpace(refreshToken))
                return false;

            var response = await _apiClient.Refresh(new RefreshRequest
            {
                RefreshToken = refreshToken
            });

            if (!response.IsSuccessStatusCode || response.Content == null)
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

    /// <summary>
    /// Met à jour l'URL de la photo de profil sur le serveur et dans le SecureStorage.
    /// </summary>
    public async Task<bool> UpdatePhotoAsync(string photoUrl)
    {
        try
        {
            var response = await _apiClient.UpdatePhoto(new UpdatePhotoDto
            {
                PhotoUrl = photoUrl
            });

            if (!response.IsSuccessStatusCode || response.Content == null)
                return false;

            // Mettre à jour l'utilisateur stocké
            var userJson = await SecureStorage.GetAsync(UserKey);
            if (!string.IsNullOrWhiteSpace(userJson))
            {
                var user = System.Text.Json.JsonSerializer.Deserialize<UserDto>(userJson);
                if (user != null)
                {
                    user.PhotoUrl = photoUrl;
                    await SecureStorage.SetAsync(UserKey, System.Text.Json.JsonSerializer.Serialize(user));
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthService] UpdatePhoto: {ex.Message}");
            return false;
        }
    }
}
