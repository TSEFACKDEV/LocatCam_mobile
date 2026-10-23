using System.Text.Json.Serialization;

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

    /// <summary>Vrai si le message a été envoyé par l'utilisateur connecté.</summary>
    [JsonIgnore]
    public bool EstMoi { get; set; }

    /// <summary>Affiche « Lu » sous les messages sortants déjà lus.</summary>
    [JsonIgnore]
    public bool AfficherAccuseLecture => EstMoi && Lu;

    /// <summary>Nom de l'expéditeur (fourni par l'API sur les listes).</summary>
    public string? ExpediteurNom { get; set; }

    /// <summary>Nom du destinataire (fourni par l'API sur les listes).</summary>
    public string? DestinataireNom { get; set; }

    /// <summary>Titre de l'annonce concernée (fourni par l'API).</summary>
    public string? AnnonceTitre { get; set; }
}
