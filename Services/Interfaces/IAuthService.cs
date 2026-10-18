using LocatCam_mobile.Models;

namespace LocatCam_mobile.Services.Interfaces;

/// <summary>
/// Contrat du service d'authentification.
///
/// Implémentation complète en Phase B. Cette interface est définie dès la Phase A
/// pour permettre l'enregistrement dans l'injection de dépendances.
/// </summary>
public interface IAuthService
{
    /// <summary>Dernier message d'erreur lisible (connexion, serveur, identifiants).</summary>
    string? DerniereErreur { get; }

    /// <summary>Connecte un utilisateur et retourne les tokens + profil.</summary>
    Task<LoginResponse?> LoginAsync(string email, string motDePasse);

    /// <summary>Inscrit un nouvel utilisateur.</summary>
    Task<bool> RegisterAsync(string nom, string email, string telephone, string motDePasse, string role);

    /// <summary>Déconnecte l'utilisateur et supprime les tokens du SecureStorage.</summary>
    Task LogoutAsync();

    /// <summary>Indique si un utilisateur est actuellement connecté.</summary>
    Task<bool> IsLoggedInAsync();

    /// <summary>Récupère le profil de l'utilisateur connecté.</summary>
    Task<UserDto?> GetCurrentUserAsync();

    /// <summary>Renouvelle l'access token via le refresh token.</summary>
    Task<bool> RefreshTokenAsync();
}
