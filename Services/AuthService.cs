using LocatCam_mobile.Models;
using LocatCam_mobile.Services.Interfaces;

namespace LocatCam_mobile.Services;

/// <summary>
/// Service d'authentification — squelette Phase A.
///
/// L'implémentation complète est développée en Phase B (Login, Register, Logout,
/// SecureStorage, TokenHandler avec refresh auto). Cette classe stub permet au
/// projet de compiler dès la Phase A.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IApiClient _apiClient;

    public AuthService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<LoginResponse?> LoginAsync(string email, string motDePasse)
    {
        // Implémentation Phase B
        throw new NotImplementedException("LoginAsync sera implémenté en Phase B.");
    }

    public Task<bool> RegisterAsync(string nom, string email, string telephone, string motDePasse, string role)
    {
        throw new NotImplementedException("RegisterAsync sera implémenté en Phase B.");
    }

    public Task LogoutAsync()
    {
        throw new NotImplementedException("LogoutAsync sera implémenté en Phase B.");
    }

    public Task<bool> IsLoggedInAsync()
    {
        return Task.FromResult(false);
    }

    public Task<UserDto?> GetCurrentUserAsync()
    {
        return Task.FromResult<UserDto?>(null);
    }

    public Task<bool> RefreshTokenAsync()
    {
        throw new NotImplementedException("RefreshTokenAsync sera implémenté en Phase B.");
    }
}
