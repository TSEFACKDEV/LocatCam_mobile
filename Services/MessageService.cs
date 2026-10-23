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
            // côté mobile (une conversation par correspondant). Les noms et le
            // titre de l'annonce sont désormais fournis par l'API.
            var messages = response.Content;
            var moi = await _authService.GetCurrentUserAsync();
            if (moi == null)
                return new List<ConversationDto>();

            var conversations = new List<ConversationDto>();

            foreach (var groupe in messages.GroupBy(m =>
                         m.ExpediteurId == moi.Id ? m.DestinataireId : m.ExpediteurId))
            {
                var dernier = groupe.OrderByDescending(m => m.DateEnvoi).First();
                var jeSuisExpediteur = dernier.ExpediteurId == moi.Id;

                var nom = jeSuisExpediteur ? dernier.DestinataireNom : dernier.ExpediteurNom;

                conversations.Add(new ConversationDto
                {
                    UtilisateurId = groupe.Key,
                    UtilisateurNom = string.IsNullOrWhiteSpace(nom) ? "Contact" : nom,
                    AnnonceId = dernier.AnnonceId,
                    AnnonceTitre = dernier.AnnonceTitre ?? string.Empty,
                    DernierMessage = dernier.Contenu,
                    DateDernierMessage = dernier.DateEnvoi,
                    MessagesNonLus = groupe.Count(m => m.DestinataireId == moi.Id && !m.Lu)
                });
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

    public async Task<bool> MarkConversationAsReadAsync(string autreUserId)
    {
        try
        {
            var response = await _apiClient.MarkConversationAsRead(autreUserId);
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
