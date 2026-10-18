namespace LocatCam_mobile.Models;

/// <summary>
/// Favori renvoyé par l'API : le favori contient l'annonce complète dans
/// la propriété <see cref="Annonce"/>.
/// </summary>
public class FavoriDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AnnonceId { get; set; }
    public DateTime DateAjout { get; set; }
    public AnnonceDto? Annonce { get; set; }
}
