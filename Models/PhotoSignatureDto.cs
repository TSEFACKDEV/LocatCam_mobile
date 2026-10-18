namespace LocatCam_mobile.Models;

/// <summary>
/// Signature Cloudinary retournée par l'API pour l'upload direct depuis le mobile.
/// </summary>
public class PhotoSignatureDto
{
    public long Timestamp { get; set; }
    public string Folder { get; set; } = "locatcam/annonces";
    public string UploadPreset { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}
