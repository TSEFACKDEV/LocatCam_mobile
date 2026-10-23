using System.Collections.Specialized;
using LocatCam_mobile.Models;
using LocatCam_mobile.Services;
using LocatCam_mobile.ViewModels;
using Microsoft.Maui.Controls;

namespace LocatCam_mobile.Views;

public partial class MessageriePage : ContentPage
{
    private readonly MessagerieViewModel _vm;

    public MessageriePage()
    {
        InitializeComponent();

        _vm = App.ServiceProvider?.GetRequiredService<MessagerieViewModel>()
            ?? throw new InvalidOperationException("MessagerieViewModel n'est pas enregistré dans le DI.");

        BindingContext = _vm;

        // La page reste en mémoire dans le Shell : OnAppearing n'est pas
        // rejoué à chaque ouverture. On se resynchronise donc explicitement.
        NavigationShell.PageAffichee += OnPageAffichee;

        // Faire défiler automatiquement vers le dernier message
        _vm.Messages.CollectionChanged += OnMessagesChanged;
    }

    private async void OnPageAffichee(string route)
    {
        if (!string.Equals(route, "MessageriePage", StringComparison.OrdinalIgnoreCase))
            return;

        await _vm.InitialiserAsync();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.InitialiserAsync();
    }

    private void OnMessagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != NotifyCollectionChangedAction.Add) return;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await Task.Delay(80);
                await ScrollMessages.ScrollToAsync(0, ScrollMessages.ContentSize.Height, true);
            }
            catch
            {
                // Défilement non critique
            }
        });
    }

    /// <summary>Ouvre la conversation sélectionnée (tap explicite, fiable sur Android).</summary>
    private void OnConversationTapped(object sender, TappedEventArgs e)
    {
        var conversation = e.Parameter as ConversationDto
            ?? (sender as BindableObject)?.BindingContext as ConversationDto;

        if (conversation != null)
            _vm.ConversationSelectionnee = conversation;
    }

    /// <summary>Bouton retour de la liste : revient à la page d'origine.</summary>
    private void OnRetourClicked(object sender, TappedEventArgs e)
        => NavigationShell.Revenir();

    /// <summary>
    /// Bouton retour Android : referme d'abord la conversation ouverte,
    /// puis revient à la page d'origine au lieu de quitter l'application.
    /// </summary>
    protected override bool OnBackButtonPressed()
    {
        if (_vm.ConversationOuverte)
        {
            _vm.FermerConversation();
            return true;
        }

        NavigationShell.Revenir();
        return true;
    }
}
