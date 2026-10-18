using System.Text.Json.Serialization;

namespace LocatCam_mobile.Models;

public class AnnoncePhotoDto
{
    public Guid Id { get; set; }
    public string UrlCloudinary { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;
    public int Ordre { get; set; }
    public bool EstPrincipale { get; set; }
}

public class AnnonceDto
{
    public Guid Id { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Prix { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Quartier { get; set; } = string.Empty;
    public string Ville { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool EstMeuble { get; set; }
    public int NombrePieces { get; set; }
    public bool Disponible { get; set; }
    public Guid UserId { get; set; }
    public string? NomBailleur { get; set; }
    public string? TelephoneBailleur { get; set; }
    public bool EstVerifie { get; set; }
    public string Statut { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; }
    public List<AnnoncePhotoDto> Photos { get; set; } = new();

    /// <summary>
    /// URL de la photo principale (ou de la premi\u00e8re photo disponible).
    /// Propri\u00e9t\u00e9 calcul\u00e9e, jamais s\u00e9rialis\u00e9e.
    /// </summary>
    [JsonIgnore]
    public string PhotoPrincipaleUrl =>
        Photos?.FirstOrDefault(p => p.EstPrincipale)?.UrlCloudinary
        ?? Photos?.FirstOrDefault()?.UrlCloudinary
        ?? string.Empty;
}
