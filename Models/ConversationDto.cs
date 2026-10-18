namespace LocatCam_mobile.Models;

public class ConversationDto
{
    public Guid UtilisateurId { get; set; }
    public string UtilisateurNom { get; set; } = string.Empty;
    public string UtilisateurEmail { get; set; } = string.Empty;
    public Guid AnnonceId { get; set; }
    public string AnnonceTitre { get; set; } = string.Empty;
    public string? DernierMessage { get; set; }
    public DateTime DateDernierMessage { get; set; }
    public int MessagesNonLus { get; set; }
}
