namespace LocatCam_mobile.Models;

/// <summary>
/// Requête de mise à jour de la photo de profil envoyée à l'API.
/// </summary>
public class UpdatePhotoDto
{
    public string PhotoUrl { get; set; } = string.Empty;
}
