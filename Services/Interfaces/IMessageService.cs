using LocatCam_mobile.Models;

namespace LocatCam_mobile.Services.Interfaces;

/// <summary>
/// Contrat du service métier pour la messagerie in-app.
/// </summary>
public interface IMessageService
{
    /// <summary>Récupère la liste des conversations de l'utilisateur connecté.</summary>
    Task<List<ConversationDto>> GetConversationsAsync();

    /// <summary>Récupère l'historique d'une conversation avec un autre utilisateur.</summary>
    Task<List<MessageDto>> GetConversationAsync(string autreUserId);

    /// <summary>Envoie un message au destinataire d'une annonce.</summary>
    Task<MessageDto?> SendMessageAsync(MessageCreateDto dto);

    /// <summary>Marque un message comme lu.</summary>
    Task<bool> MarkAsReadAsync(string messageId);
}
