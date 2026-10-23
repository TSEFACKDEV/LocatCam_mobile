namespace LocatCam_mobile.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Données envoyées au serveur pour déclarer une photo Cloudinary liée à une annonce.
/// </summary>
public class PhotoCreateDto
{
    public string AnnonceId { get; set; } = string.Empty;
    public string UrlCloudinary { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;
    public int Ordre { get; set; }
    public bool EstPrincipale { get; set; }
}

/// <summary>
/// Réponse simplifiée de l'upload Cloudinary (secure_url + public_id).
/// Cloudinary retourne du snake_case (<c>secure_url</c>, <c>public_id</c>).
/// </summary>
public class CloudinaryUploadResult
{
    [JsonPropertyName("secure_url")]
    public string SecureUrl { get; set; } = string.Empty;

    [JsonPropertyName("public_id")]
    public string PublicId { get; set; } = string.Empty;
}
