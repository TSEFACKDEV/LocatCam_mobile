namespace LocatCam_mobile.Models;

public class MessageCreateDto
{
    public Guid AnnonceId { get; set; }
    public Guid DestinataireId { get; set; }
    public string Contenu { get; set; } = string.Empty;
}
