using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.NetworkInformation;
using Microsoft.Maui.ApplicationModel;

namespace LocatCam_mobile.Services;

/// <summary>
/// Service de détection de l'état du réseau (Phase G).
/// Surveille la connectivité et notifie les abonnés quand l'app passe en
/// ligne / hors-ligne, afin d'afficher le bandeau "Hors-ligne" et de basculer
/// sur les données locales (favoris, dernières recherches).
/// </summary>
public class ConnectivityService : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _isConnected;

    public bool IsConnected
    {
        get => _isConnected;
        private set
        {
            if (_isConnected != value)
            {
                _isConnected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnected)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsOffline)));
            }
        }
    }

    public bool IsOffline => !IsConnected;

    public ConnectivityService()
    {
        // État initial
        IsConnected = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

        // Surveillance des changements de connectivité
        Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        IsConnected = e.NetworkAccess == NetworkAccess.Internet;
    }

    /// <summary>Vérifie ponctuellement si le réseau est disponible.</summary>
    public Task<bool> CheckNetworkAccessAsync()
    {
        var access = Connectivity.Current.NetworkAccess;
        IsConnected = access == NetworkAccess.Internet;
        return Task.FromResult(IsConnected);
    }
}
