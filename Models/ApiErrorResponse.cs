namespace LocatCam_mobile.Models;

/// <summary>
/// Réponse d'erreur standardisée de l'API LocatCam.
/// Format : { status, message, errors? }
/// </summary>
public class ApiErrorResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public IDictionary<string, string[]>? Errors { get; set; }
}
