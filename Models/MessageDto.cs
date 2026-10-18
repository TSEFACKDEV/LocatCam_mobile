namespace LocatCam_mobile.Models;

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid AnnonceId { get; set; }
    public Guid ExpediteurId { get; set; }
    public Guid DestinataireId { get; set; }
    public string Contenu { get; set; } = string.Empty;
    public DateTime DateEnvoi { get; set; }
    public bool Lu { get; set; }
}
