using LocatCam_mobile.Models;

namespace LocatCam_mobile.Services;

/// <summary>
/// Passerelle de navigation : mémorise l'annonce choisie (accueil, favoris,
/// carte) afin que la page Détail puisse l'afficher sans API de paramètres
/// de route.
/// </summary>
public static class AnnonceSelection
{
    /// <summary>Dernière annonce sélectionnée par l'utilisateur.</summary>
    public static AnnonceDto? Annonce { get; set; }
}
