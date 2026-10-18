namespace LocatCam_mobile.Models;

public class SignalementDto
{
    public Guid Id { get; set; }
    public Guid AnnonceId { get; set; }
    public Guid UserId { get; set; }
    public string Raison { get; set; } = string.Empty;
    public DateTime DateSignalement { get; set; }
    public bool Traite { get; set; }
}
