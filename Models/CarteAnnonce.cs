namespace LocatCam_mobile.Models;

/// <summary>
/// Annonce enrichie avec sa distance par rapport à la position utilisateur
/// (uniquement pour l'affichage carte, non retournée par l'API).
/// </summary>
public class CarteAnnonce : AnnonceDto
{
    /// <summary>Distance en mètres par rapport à la position de l'utilisateur.</summary>
    public double? DistanceMeters { get; set; }

    /// <summary>Distance lisible (ex: "450 m" ou "2,3 km").</summary>
    public string? DistanceTexte { get; set; }

    /// <summary>Classe CSS pour le badge de proximité.</summary>
    public string DistanceClasse
    {
        get
        {
            if (!DistanceMeters.HasValue) return "dist-mid";
            if (DistanceMeters <= 800) return "dist-close";
            if (DistanceMeters <= 2500) return "dist-mid";
            return "dist-far";
        }
    }

    /// <summary>Indique si l'annonce est dans le rayon sélectionné.</summary>
    public bool EstDansRayon => !DistanceMeters.HasValue || DistanceMeters <= 2500;
}
