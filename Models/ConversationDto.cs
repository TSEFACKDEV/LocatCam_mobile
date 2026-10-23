using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LocatCam_mobile.Models;

/// <summary>
/// Conversation agrégée affichée dans la messagerie.
/// Implémente INotifyPropertyChanged pour que le dernier message et le compteur
/// de non-lus se mettent à jour sans reconstruire toute la liste.
/// </summary>
public class ConversationDto : INotifyPropertyChanged
{
    private string? _dernierMessage;
    private DateTime _dateDernierMessage;
    private int _messagesNonLus;

    public Guid UtilisateurId { get; set; }

    public string UtilisateurNom { get; set; } = string.Empty;

    public string UtilisateurEmail { get; set; } = string.Empty;

    public Guid AnnonceId { get; set; }

    public string AnnonceTitre { get; set; } = string.Empty;

    public string? DernierMessage
    {
        get => _dernierMessage;
        set
        {
            if (_dernierMessage == value) return;
            _dernierMessage = value;
            OnPropertyChanged();
        }
    }

    public DateTime DateDernierMessage
    {
        get => _dateDernierMessage;
        set
        {
            if (_dateDernierMessage == value) return;
            _dateDernierMessage = value;
            OnPropertyChanged();
        }
    }

    public int MessagesNonLus
    {
        get => _messagesNonLus;
        set
        {
            if (_messagesNonLus == value) return;
            _messagesNonLus = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? nom = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nom));
}
