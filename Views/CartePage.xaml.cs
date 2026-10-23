using LocatCam_mobile.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using System.Globalization;
using System.Linq;

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

        // Quand le ViewModel met à jour les marqueurs (nouveau rayon, nouveau centre),
        // on les réinjecte dans la WebView avec la position et le rayon utilisateur.
        _vm.PropertyChanged += async (s, e) =>
        {
            if (e.PropertyName == nameof(MapViewModel.MarkersJson))
            {
                await InjectMarkersAsync();
                await InjectUserPositionAsync();
                await InjectUserRadiusAsync();
            }
        };

        _ = LoadMapHtmlAsync();

        // Bridge JS → C# : interception des URL "locatcam://..."
        MapWebView.Navigating += OnWebViewNavigating;
    }

    /// <summary>Initialise la position et les marqueurs une fois la carte HTML prête.</summary>
    private async Task InitialiserCarteAsync()
    {
        await _vm.InitialiserPositionAsync();
        await InjectUserPositionAsync();
        await InjectUserRadiusAsync();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Géolocalisation automatique à chaque apparition de la page carte
        if (_htmlLoaded)
        {
            await InitialiserCarteAsync();
        }
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

            // Géolocalisation automatique dès que la carte HTML est prête
            _ = InitialiserCarteAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CartePage] Erreur chargement de la carte: {ex.Message}");
        }
    }

    /// <summary>Envoie la position utilisateur détectée vers la carte JS.</summary>
    private async Task InjectUserPositionAsync()
    {
        if (!_htmlLoaded || !_vm.HasLocation) return;

        var lat = _vm.CurrentLatitude.ToString(CultureInfo.InvariantCulture);
        var lng = _vm.CurrentLongitude.ToString(CultureInfo.InvariantCulture);
        await MapWebView.EvaluateJavaScriptAsync($"setUserPosition({lat}, {lng})");
    }

    /// <summary>Envoie le rayon de recherche sélectionné vers la carte JS.</summary>
    private async Task InjectUserRadiusAsync()
    {
        if (!_htmlLoaded) return;
        await MapWebView.EvaluateJavaScriptAsync($"setUserRadius({_vm.UserRadiusMeters})");
    }

    private void OnWebViewNavigating(object sender, WebNavigatingEventArgs e)
    {
        var url = e.Url ?? string.Empty;
        if (!url.StartsWith("locatcam://", StringComparison.OrdinalIgnoreCase))
            return;

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
            // Recharger les annonces dans la nouvelle bounding box, puis réinjecter
            // la position utilisateur et le rayon pour les conserver à l'écran
            await _vm.LoadAnnouncementsAsync(minLat.Value, maxLat.Value, minLng.Value, maxLng.Value);
            await InjectMarkersAsync();
            await InjectUserPositionAsync();
            await InjectUserRadiusAsync();
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

    private async Task InjectMarkersAsync()
    {
        if (!_htmlLoaded) return;

        var json = _vm.MarkersJson ?? "[]";
        var js = $"addMarkers({json})";
        await MapWebView.EvaluateJavaScriptAsync(js);
    }
}
