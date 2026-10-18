using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocatCam_mobile.Models;
using LocatCam_mobile.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Linq;

namespace LocatCam_mobile.ViewModels;

/// <summary>
/// ViewModel de la messagerie in-app (Phase F).
/// Affiche la liste des conversations et permet d'échanger des messages
/// avec le destinataire d'une annonce (POST /api/messages).
/// </summary>
public partial class MessagerieViewModel : ObservableObject
{
    private readonly IMessageService _messageService;
    private readonly IAuthService _authService;

    public MessagerieViewModel(IMessageService messageService, IAuthService authService)
    {
        _messageService = messageService;
        _authService = authService;
    }

    // ------------------------------------------------------------------
    // Liste des conversations
    // ------------------------------------------------------------------

    [ObservableProperty]
    private ObservableCollection<ConversationDto> _conversations = new();

    [ObservableProperty]
    private ConversationDto? _conversationSelectionnee;

    [ObservableProperty]
    private bool _isLoadingConversations;

    /// <summary>Nombre total de messages non lus (badge onglet).</summary>
    public int TotalNonLus => Conversations.Sum(c => c.MessagesNonLus);

    // ------------------------------------------------------------------
    // Détail d'une conversation
    // ------------------------------------------------------------------

    [ObservableProperty]
    private ObservableCollection<MessageDto> _messages = new();

    [ObservableProperty]
    private string _contenuNouveauMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoadingMessages;

    /// <summary>ID de l'annonce concernée par la conversation courante.</summary>
    public Guid AnnonceIdCourante => ConversationSelectionnee?.AnnonceId ?? Guid.Empty;

    /// <summary>ID du destinataire de la conversation courante.</summary>
    public Guid DestinataireIdCourant => ConversationSelectionnee?.UtilisateurId ?? Guid.Empty;

    /// <summary>Indique si une conversation est sélectionnée (détail affiché).</summary>
    public bool ConversationOuverte => ConversationSelectionnee != null;

    // ------------------------------------------------------------------
    // Chargement des conversations
    // ------------------------------------------------------------------

    [RelayCommand]
    public async Task ChargerConversationsAsync()
    {
        IsLoadingConversations = true;
        try
        {
            var liste = await _messageService.GetConversationsAsync();
            Conversations = new ObservableCollection<ConversationDto>(liste ?? new List<ConversationDto>());
            OnPropertyChanged(nameof(TotalNonLus));

            // Conversation demandée depuis « Contacter le bailleur ».
            var demandee = Services.ConversationSelection.Conversation;
            if (demandee != null)
            {
                Services.ConversationSelection.Conversation = null;

                var existante = Conversations.FirstOrDefault(c => c.UtilisateurId == demandee.UtilisateurId);
                if (existante == null)
                {
                    Conversations.Insert(0, demandee);
                    existante = demandee;
                }

                ConversationSelectionnee = existante;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MessagerieViewModel] Erreur conversations: {ex.Message}");
        }
        finally
        {
            IsLoadingConversations = false;
        }
    }

    // ------------------------------------------------------------------
    // Sélection d'une conversation
    // ------------------------------------------------------------------

    partial void OnConversationSelectionneeChanged(ConversationDto? value)
    {
        if (value == null) return;

        OnPropertyChanged(nameof(ConversationOuverte));
        OnPropertyChanged(nameof(AnnonceIdCourante));
        OnPropertyChanged(nameof(DestinataireIdCourant));

        _ = ChargerMessagesAsync();
    }

    [RelayCommand]
    private async Task ChargerMessagesAsync()
    {
        if (ConversationSelectionnee == null) return;

        IsLoadingMessages = true;
        Messages.Clear();

        try
        {
            var historique = await _messageService.GetConversationAsync(DestinataireIdCourant.ToString());
            foreach (var m in historique ?? new List<MessageDto>())
                Messages.Add(m);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MessagerieViewModel] Erreur messages: {ex.Message}");
        }
        finally
        {
            IsLoadingMessages = false;
        }
    }

    // ------------------------------------------------------------------
    // Envoi de message
    // ------------------------------------------------------------------

    [RelayCommand]
    private async Task EnvoyerMessageAsync()
    {
        if (ConversationSelectionnee == null) return;
        if (string.IsNullOrWhiteSpace(ContenuNouveauMessage)) return;

        IsLoadingMessages = true;

        try
        {
            var dto = new MessageCreateDto
            {
                AnnonceId = AnnonceIdCourante,
                DestinataireId = DestinataireIdCourant,
                Contenu = ContenuNouveauMessage.Trim()
            };

            var envoie = await _messageService.SendMessageAsync(dto);

            if (envoie != null)
            {
                Messages.Add(envoie);
                ContenuNouveauMessage = string.Empty;

                // Rafraîchir la liste des conversations (compteur non lus)
                await ChargerConversationsAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MessagerieViewModel] Erreur envoi: {ex.Message}");
        }
        finally
        {
            IsLoadingMessages = false;
        }
    }

    /// <summary>Ferme la conversation courante et revient à la liste.</summary>
    [RelayCommand]
    public void FermerConversation()
    {
        ConversationSelectionnee = null;
        Messages.Clear();
        ContenuNouveauMessage = string.Empty;
    }
}
