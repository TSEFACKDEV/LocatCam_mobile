namespace LocatCam_mobile.Models;

/// <summary>
/// Requête de renouvellement de jeton d'accès (refresh) envoyée à l'API.
/// </summary>
public class RefreshRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
