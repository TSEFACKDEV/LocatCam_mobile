using LocatCam_mobile.Models;

namespace LocatCam_mobile.Services;

/// <summary>
/// Passerelle de navigation : mémorise la conversation demandée depuis le
/// détail d'une annonce afin que la messagerie l'ouvre directement.
/// </summary>
public static class ConversationSelection
{
    public static ConversationDto? Conversation { get; set; }
}
