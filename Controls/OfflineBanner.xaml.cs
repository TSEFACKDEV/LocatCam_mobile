using LocatCam_mobile.Services;

namespace LocatCam_mobile.Controls;

public partial class OfflineBanner : ContentView
{
    public static readonly BindableProperty IsOfflineProperty =
        BindableProperty.Create(nameof(IsOffline), typeof(bool), typeof(OfflineBanner), false,
            propertyChanged: OnIsOfflineChanged);

    public bool IsOffline
    {
        get => (bool)GetValue(IsOfflineProperty);
        set => SetValue(IsOfflineProperty, value);
    }

    private readonly ConnectivityService _connectivityService;

    public OfflineBanner()
    {
        InitializeComponent();

        _connectivityService = App.ServiceProvider?.GetRequiredService<ConnectivityService>()
            ?? throw new InvalidOperationException("ConnectivityService n'est pas enregistré dans le DI.");

        IsOffline = _connectivityService.IsOffline;
        IsVisible = IsOffline;
        _connectivityService.PropertyChanged += OnConnectivityChanged;
    }

    private static void OnIsOfflineChanged(BindableObject bindable, object oldValue, object newValue)
    {
        // Masquer aussi le conteneur : une Frame invisible peut continuer à
        // réserver de la hauteur dans une ligne Auto, ce qui créait un vide
        // en haut des pages (Accueil, Profil, Carte...).
        if (bindable is OfflineBanner banner)
            banner.IsVisible = newValue is true;
    }

    private void OnConnectivityChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ConnectivityService.IsOffline))
            IsOffline = _connectivityService.IsOffline;
    }
}
