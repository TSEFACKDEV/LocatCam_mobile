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

    public MessageService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<ConversationDto>> GetConversationsAsync()
    {
        try
        {
            var response = await _apiClient.GetConversations();

            if (!response.IsSuccess || response.Content == null)
                return new List<ConversationDto>();

            return response.Content;
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

            if (!response.IsSuccess || response.Content == null)
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

            if (!response.IsSuccess || response.Content == null)
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
            return response.IsSuccess;
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
