namespace LocatCam_mobile.Models;

public class AnnonceCreateDto
{
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
    public bool Disponible { get; set; } = true;
}
