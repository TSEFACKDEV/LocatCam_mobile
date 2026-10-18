using LocatCam_mobile.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using System.Globalization;
using System.Text.Json;

namespace LocatCam_mobile.Views;

public partial class CartePage : ContentPage
{
    private readonly MapViewModel _vm;
    private bool _htmlLoaded = false;

    public CartePage()
    {
        InitializeComponent();

        _vm = App.ServiceProvider?.GetRequiredService<MapViewModel>()
            ?? throw new InvalidOperationException("MapViewModel n'est pas enregistré dans le DI.");

        BindingContext = _vm;

        _ = LoadMapHtmlAsync();

        // Bridge JS &rarr; C# : interception des URL "locatcam://..."
        MapWebView.Navigating += OnWebViewNavigating;
    }

    private async Task LoadMapHtmlAsync()
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("map.html");
            using var reader = new StreamReader(stream);
            var html = await reader.ReadToEndAsync();

            MapWebView.Source = new HtmlWebViewSource { Html = html };
            _htmlLoaded = true;

            // Chargement initial autour de Douala (Cameroun)
            await _vm.LoadAnnouncementsAsync(3.5, 4.6, 9.2, 10.3);
            await InjectMarkersAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CartePage] Erreur chargement de la carte: {ex.Message}");
        }
    }

    private async Task InjectMarkersAsync()
    {
        if (!_htmlLoaded) return;

        var json = _vm.MarkersJson ?? "[]";
        var js = $"addMarkers({json})";
        await MapWebView.EvaluateJavaScriptAsync(js);
    }

    private void OnWebViewNavigating(object sender, WebNavigatingEventArgs e)
    {
        var url = e.Url ?? string.Empty;
        if (!url.StartsWith("locatcam://", StringComparison.OrdinalIgnoreCase))
            return;

        // On annule la navigation : il s'agit d'un message JS, pas d'une vraie URL
        e.Cancel = true;

        if (url.StartsWith("locatcam://marker?id=", StringComparison.OrdinalIgnoreCase))
        {
            var id = url.Substring("locatcam://marker?id=".Length);
            _ = SelectAnnonceAsync(id);
        }
        else if (url.StartsWith("locatcam://bounds?", StringComparison.OrdinalIgnoreCase))
        {
            _ = OnBoundsChanged(url);
        }
    }

    private async Task SelectAnnonceAsync(string id)
    {
        var annonce = _vm.LastAnnounces?.FirstOrDefault(a => a.Id.ToString() == id);
        if (annonce != null)
        {
            _vm.SelectedAnnonce = annonce;
        }
    }

    private async Task OnBoundsChanged(string url)
    {
        var query = url.Substring(url.IndexOf('?') + 1);
        var minLat = GetQueryParam(query, "minLat");
        var maxLat = GetQueryParam(query, "maxLat");
        var minLng = GetQueryParam(query, "minLng");
        var maxLng = GetQueryParam(query, "maxLng");

        if (minLat.HasValue && maxLat.HasValue && minLng.HasValue && maxLng.HasValue)
        {
            await _vm.LoadAnnouncementsAsync(minLat.Value, maxLat.Value, minLng.Value, maxLng.Value);
            await InjectMarkersAsync();
        }
    }

    private static double? GetQueryParam(string query, string key)
    {
        foreach (var part in query.Split('&'))
        {
            var kv = part.Split('=');
            if (kv.Length == 2 && kv[0] == key
                && double.TryParse(kv[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }
        }
        return null;
    }

    private async void OnLocateClicked(object sender, EventArgs e)
    {
        try
        {
            var location = await Geolocation.Default.GetLastKnownLocationAsync();
            if (location == null) return;

            var lat = location.Latitude;
            var lng = location.Longitude;

            // Centrer la carte sur la position
            var latStr = lat.ToString(CultureInfo.InvariantCulture);
            var lngStr = lng.ToString(CultureInfo.InvariantCulture);
            await MapWebView.EvaluateJavaScriptAsync($"centerOn({latStr}, {lngStr}, 15)");

            // Recharger les annonces autour de la position
            var margin = 0.05;
            await _vm.LoadAnnouncementsAsync(lat - margin, lat + margin, lng - margin, lng + margin);
            await InjectMarkersAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CartePage] Géolocalisation indisponible: {ex.Message}");
        }
    }
}
