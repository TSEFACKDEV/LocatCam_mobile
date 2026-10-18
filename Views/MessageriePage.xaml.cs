using System.Collections.Specialized;
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

        // Charger les conversations à l'apparition de la page
        _ = _vm.ChargerConversationsAsync();

        // Faire défiler automatiquement vers le dernier message
        _vm.Messages.CollectionChanged += OnMessagesChanged;
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

    /// <summary>Bouton retour de la liste : revient à la page d'origine.</summary>
    private void OnRetourClicked(object sender, TappedEventArgs e)
        => NavigationShell.Revenir();
}
