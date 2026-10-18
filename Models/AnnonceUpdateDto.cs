namespace LocatCam_mobile.Models;

public class AnnonceUpdateDto
{
    public string? Titre { get; set; }
    public string? Description { get; set; }
    public decimal? Prix { get; set; }
    public string? Type { get; set; }
    public string? Quartier { get; set; }
    public string? Ville { get; set; }
    public string? Adresse { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool? EstMeuble { get; set; }
    public int? NombrePieces { get; set; }
    public bool? Disponible { get; set; }
}
