using LocatCam_mobile.Models;
using LocatCam_mobile.Services.Interfaces;
using Refit;

namespace LocatCam_mobile.Services;

/// <summary>
/// Service métier pour la messagerie in-app.
/// </summary>
public class MessageService : IMessageService
{
    private readonly IApiClient _apiClient;
    private readonly IAuthService _authService;

    public MessageService(IApiClient apiClient, IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    public async Task<List<ConversationDto>> GetConversationsAsync()
    {
        try
        {
            var response = await _apiClient.GetConversations();

            if (!response.IsSuccessStatusCode || response.Content == null)
                return new List<ConversationDto>();

            // L'API renvoie les messages à plat : on reconstruit les conversations
            // côté mobile (une conversation par correspondant).
            var messages = response.Content;
            var moi = await _authService.GetCurrentUserAsync();
            if (moi == null)
                return new List<ConversationDto>();

            var conversations = new List<ConversationDto>();

            foreach (var groupe in messages.GroupBy(m =>
                         m.ExpediteurId == moi.Id ? m.DestinataireId : m.ExpediteurId))
            {
                var dernier = groupe.OrderByDescending(m => m.DateEnvoi).First();

                var conversation = new ConversationDto
                {
                    UtilisateurId = groupe.Key,
                    AnnonceId = dernier.AnnonceId,
                    DernierMessage = dernier.Contenu,
                    DateDernierMessage = dernier.DateEnvoi,
                    MessagesNonLus = groupe.Count(m => m.DestinataireId == moi.Id && !m.Lu)
                };

                // Compléter avec le titre de l'annonce et le nom du bailleur.
                var annonce = await _apiClient.GetAnnonceById(dernier.AnnonceId.ToString());
                if (annonce.IsSuccessStatusCode && annonce.Content != null)
                {
                    conversation.AnnonceTitre = annonce.Content.Titre;
                    conversation.UtilisateurNom = annonce.Content.UserId == groupe.Key
                        ? (annonce.Content.NomBailleur ?? "Bailleur")
                        : "Étudiant";
                }
                else
                {
                    conversation.UtilisateurNom = "Contact";
                }

                conversations.Add(conversation);
            }

            return conversations
                .OrderByDescending(c => c.DateDernierMessage)
                .ToList();
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MessageService] Erreur API {ex.StatusCode}: {ex.Message}");
            return new List<ConversationDto>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MessageService] Erreur inattendue: {ex.Message}");
            return new List<ConversationDto>();
        }
    }

    public async Task<List<MessageDto>> GetConversationAsync(string autreUserId)
    {
        try
        {
            var response = await _apiClient.GetConversation(autreUserId);

            if (!response.IsSuccessStatusCode || response.Content == null)
                return new List<MessageDto>();

            return response.Content;
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MessageService] Erreur API {ex.StatusCode}: {ex.Message}");
            return new List<MessageDto>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MessageService] Erreur inattendue: {ex.Message}");
            return new List<MessageDto>();
        }
    }

    public async Task<MessageDto?> SendMessageAsync(MessageCreateDto dto)
    {
        try
        {
            var response = await _apiClient.SendMessage(dto);

            if (!response.IsSuccessStatusCode || response.Content == null)
                return null;

            return response.Content;
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MessageService] Erreur API {ex.StatusCode}: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MessageService] Erreur inattendue: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> MarkAsReadAsync(string messageId)
    {
        try
        {
            var response = await _apiClient.MarkAsRead(messageId);
            return response.IsSuccessStatusCode;
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MessageService] Erreur API {ex.StatusCode}: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MessageService] Erreur inattendue: {ex.Message}");
            return false;
        }
    }
}
