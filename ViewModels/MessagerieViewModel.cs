using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocatCam_mobile.Models;
using LocatCam_mobile.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Linq;

namespace LocatCam_mobile.ViewModels;

/// <summary>
/// ViewModel de la messagerie in-app.
/// Affiche la liste des conversations et permet d'échanger des messages
/// avec le destinataire d'une annonce (POST /api/messages).
/// </summary>
public partial class MessagerieViewModel : ObservableObject
{
    private readonly IMessageService _messageService;
    private readonly IAuthService _authService;

    private bool _chargementMessagesEnCours;

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

    [ObservableProperty]
    private string _messageErreur = string.Empty;

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
        if (IsLoadingConversations) return;

        IsLoadingConversations = true;
        var selectionPrecedente = ConversationSelectionnee?.UtilisateurId;

        try
        {
            var liste = await _messageService.GetConversationsAsync() ?? new List<ConversationDto>();

            // Conversation demandée depuis « Contacter le bailleur ».
            var demandee = Services.ConversationSelection.Conversation;
            if (demandee != null)
            {
                Services.ConversationSelection.Conversation = null;

                var existante = liste.FirstOrDefault(c => c.UtilisateurId == demandee.UtilisateurId);
                if (existante == null)
                {
                    liste.Insert(0, demandee);
                    selectionPrecedente = demandee.UtilisateurId;
                }
                else if (selectionPrecedente == null)
                {
                    selectionPrecedente = existante.UtilisateurId;
                }
            }

            Conversations.Clear();
            foreach (var conversation in liste.OrderByDescending(c => c.DateDernierMessage))
                Conversations.Add(conversation);

            OnPropertyChanged(nameof(TotalNonLus));

            // Conserver la conversation ouverte après un rafraîchissement.
            if (selectionPrecedente.HasValue)
            {
                var aRouvrir = Conversations.FirstOrDefault(c => c.UtilisateurId == selectionPrecedente.Value);
                if (aRouvrir != null && !ReferenceEquals(aRouvrir, ConversationSelectionnee))
                    ConversationSelectionnee = aRouvrir;
                else if (aRouvrir == null)
                    ConversationSelectionnee = null;
            }
        }
        catch (Exception ex)
        {
            MessageErreur = "Impossible de charger les conversations.";
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
        OnPropertyChanged(nameof(ConversationOuverte));
        OnPropertyChanged(nameof(AnnonceIdCourante));
        OnPropertyChanged(nameof(DestinataireIdCourant));
        OnPropertyChanged(nameof(TotalNonLus));

        if (value == null)
        {
            Messages.Clear();
            return;
        }

        _ = ChargerMessagesAsync();
    }

    [RelayCommand]
    private async Task ChargerMessagesAsync()
    {
        if (ConversationSelectionnee == null || _chargementMessagesEnCours) return;

        _chargementMessagesEnCours = true;
        IsLoadingMessages = true;
        MessageErreur = string.Empty;

        var destinataire = DestinataireIdCourant;
        var messages = new List<MessageDto>();

        try
        {
            messages = await _messageService.GetConversationAsync(destinataire.ToString())
                       ?? new List<MessageDto>();

            Messages.Clear();
            foreach (var m in messages.OrderBy(m => m.DateEnvoi))
            {
                m.EstMoi = m.ExpediteurId == CurrentUserId;
                Messages.Add(m);
            }

            // Accusés de lecture : l'utilisateur vient d'ouvrir la conversation.
            if (messages.Any(m => m.DestinataireId == CurrentUserId && !m.Lu))
            {
                var marque = await _messageService.MarkConversationAsReadAsync(destinataire.ToString());
                if (marque)
                {
                    foreach (var m in Messages.Where(m => m.DestinataireId == CurrentUserId))
                        m.Lu = true;

                    if (ConversationSelectionnee != null)
                        ConversationSelectionnee.MessagesNonLus = 0;
                    OnPropertyChanged(nameof(TotalNonLus));
                }
            }
        }
        catch (Exception ex)
        {
            MessageErreur = "Impossible de charger les messages.";
            System.Diagnostics.Debug.WriteLine($"[MessagerieViewModel] Erreur messages: {ex.Message}");
        }
        finally
        {
            IsLoadingMessages = false;
            _chargementMessagesEnCours = false;
        }
    }

    /// <summary>Identifiant de l'utilisateur connecté (mis en cache).</summary>
    private Guid CurrentUserId { get; set; }

    /// <summary>
    /// Prépare le ViewModel : identifie l'utilisateur puis charge les conversations.
    /// </summary>
    public async Task InitialiserAsync()
    {
        var moi = await _authService.GetCurrentUserAsync();
        CurrentUserId = moi?.Id ?? Guid.Empty;
        await ChargerConversationsAsync();
    }

    // ------------------------------------------------------------------
    // Envoi de message
    // ------------------------------------------------------------------

    [RelayCommand]
    private async Task EnvoyerMessageAsync()
    {
        if (ConversationSelectionnee == null) return;

        var contenu = ContenuNouveauMessage?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(contenu)) return;

        IsLoadingMessages = true;
        MessageErreur = string.Empty;

        try
        {
            var dto = new MessageCreateDto
            {
                AnnonceId = AnnonceIdCourante,
                DestinataireId = DestinataireIdCourant,
                Contenu = contenu
            };

            var envoye = await _messageService.SendMessageAsync(dto);

            if (envoye == null)
            {
                MessageErreur = "Le message n'a pas pu être envoyé. Vérifiez votre connexion.";
                return;
            }

            envoye.EstMoi = true;
            Messages.Add(envoye);
            ContenuNouveauMessage = string.Empty;

            // Mettre à jour la conversation ouverte sans recharger toute la liste
            // (un rechargement faisait perdre la sélection et refermait le chat).
            ConversationSelectionnee.DernierMessage = envoye.Contenu;
            ConversationSelectionnee.DateDernierMessage = envoye.DateEnvoi;
            ConversationSelectionnee.MessagesNonLus = 0;

            var item = Conversations.FirstOrDefault(c => c.UtilisateurId == ConversationSelectionnee.UtilisateurId);
            if (item != null && !ReferenceEquals(item, ConversationSelectionnee))
                Conversations.Remove(item);

            OnPropertyChanged(nameof(TotalNonLus));
        }
        catch (Exception ex)
        {
            MessageErreur = "Erreur lors de l'envoi du message.";
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
        MessageErreur = string.Empty;
    }
}
