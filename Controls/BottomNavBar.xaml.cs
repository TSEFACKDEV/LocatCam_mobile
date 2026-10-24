using CommunityToolkit.Mvvm.Messaging;
using LocatCam_mobile.Models;
using LocatCam_mobile.Services;
using LocatCam_mobile.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace LocatCam_mobile.Controls;

/// <summary>
/// Barre de navigation basse personnalisée (5 onglets).
/// L'onglet actif est mis en valeur par une pastille bleu clair + icône bleue.
/// </summary>
public partial class BottomNavBar : ContentView
{
    public static readonly BindableProperty EstActifProperty =
        BindableProperty.Create(nameof(EstActif), typeof(string), typeof(BottomNavBar), "HomePage",
            propertyChanged: OnEstActifChanged);

    public static readonly BindableProperty EstBailleurProperty =
        BindableProperty.Create(nameof(EstBailleur), typeof(bool), typeof(BottomNavBar), false,
            propertyChanged: OnEstBailleurChanged);

    public string EstActif
    {
        get => (string)GetValue(EstActifProperty);
        set => SetValue(EstActifProperty, value);
    }

    /// <summary>L'onglet Publier n'est affiché que pour les comptes Bailleur.</summary>
    public bool EstBailleur
    {
        get => (bool)GetValue(EstBailleurProperty);
        set => SetValue(EstBailleurProperty, value);
    }

    public BottomNavBar()
    {
        InitializeComponent();
        MettreAJourApparence(EstActif);

        // Mise à jour immédiate de l'avatar quand la photo change depuis le profil.
        WeakReferenceMessenger.Default.Register<PhotoProfilMessage>(this, (recipient, message) =>
        {
            if (recipient is BottomNavBar nav)
                nav.AppliquerPhotoProfil(message.PhotoUrl);
        });
    }

    /// <summary>Affiche la photo de profil dans l'onglet, ou l'icône par défaut.</summary>
    private void AppliquerPhotoProfil(string? photoUrl)
    {
        var aUnePhoto = !string.IsNullOrWhiteSpace(photoUrl);
        IconeProfil.IsVisible = !aUnePhoto;
        PhotoProfilNav.IsVisible = aUnePhoto;

        if (aUnePhoto)
            PhotoProfilNav.Source = ImageSource.FromUri(new Uri(photoUrl!));
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Application.Current != null)
            Application.Current.RequestedThemeChanged += OnThemeChanged;

        ChargerRoleAsync();
    }

    private async void ChargerRoleAsync()
    {
        try
        {
            var auth = App.ServiceProvider?.GetService<IAuthService>();
            if (auth == null) return;

            var user = await auth.GetCurrentUserAsync();
            EstBailleur = string.Equals(user?.Role, "Bailleur", StringComparison.OrdinalIgnoreCase);
            AppliquerPhotoProfil(user?.PhotoUrl);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[BottomNavBar] Rôle indisponible: {ex.Message}");
        }
    }

    private static void OnEstBailleurChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BottomNavBar nav)
            nav.MettreAJourApparence(nav.EstActif);
    }

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e)
        => MettreAJourApparence(EstActif);

    private static void OnEstActifChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BottomNavBar nav && newValue is string actif)
            nav.MettreAJourApparence(actif);
    }

    private void MettreAJourApparence(string actif)
    {
        var accent = RessourceTheme("Accent", "AccentDark");
        var inactif = RessourceTheme("InkMuted", "InkMutedDark");
        var pastilleActive = RessourceTheme("AccentSoft", "AccentSoftDark");
        var transparent = Colors.Transparent;

        SetOnglet(PillAccueil, IconeAccueil, LabelAccueil, actif == "HomePage", accent, inactif, pastilleActive, transparent);
        SetOnglet(PillCarte, IconeCarte, LabelCarte, actif == "CartePage", accent, inactif, pastilleActive, transparent);
        SetOnglet(PillFavoris, IconeFavoris, LabelFavoris, actif == "FavorisPage", accent, inactif, pastilleActive, transparent);
        SetOnglet(PillPublier, IconePublier, LabelPublier, actif == "PublierPage", accent, inactif, pastilleActive, transparent);
        SetOnglet(PillProfil, IconeProfil, LabelProfil, actif == "ProfilPage", accent, inactif, pastilleActive, transparent);

        // Masquer complètement l'onglet Publier pour les non-bailleurs et
        // redistribuer les colonnes (5 onglets -> 4 onglets).
        OngletPublier.IsVisible = EstBailleur;
        Grid.SetColumn(OngletProfil, EstBailleur ? 4 : 3);
        GrilleOnglets.ColumnDefinitions.Clear();
        var nbColonnes = EstBailleur ? 5 : 4;
        for (int i = 0; i < nbColonnes; i++)
            GrilleOnglets.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
    }

    private static void SetOnglet(Frame pill, Label icone, Label label, bool estActif,
        Color accent, Color inactif, Color pastilleActive, Color transparent)
    {
        pill.BackgroundColor = estActif ? pastilleActive : transparent;
        icone.TextColor = estActif ? accent : inactif;
        label.TextColor = estActif ? accent : inactif;
    }

    private static Color RessourceTheme(string cleClair, string cleSombre)
    {
        try
        {
            var cle = Application.Current?.RequestedTheme == AppTheme.Dark ? cleSombre : cleClair;
            if (Application.Current?.Resources.TryGetValue(cle, out var valeur) == true && valeur is Color couleur)
                return couleur;
        }
        catch
        {
            // Ignorer : couleur par défaut ci-dessous
        }

        return Colors.Gray;
    }

    private async void OnOngletTapped(object sender, TappedEventArgs e)
    {
        // Selon la plateforme, sender peut être la vue ou le geste : le paramètre
        // porté par le TapGestureRecognizer est la source fiable de la route.
        var route = e.Parameter as string;

        if (string.IsNullOrWhiteSpace(route))
            route = (sender as VisualElement)?.AutomationId;

        if (string.IsNullOrWhiteSpace(route))
            return;

        // L'état actif est porté par la barre de la page de destination :
        // on ne modifie pas celui de la page courante (sinon, en revenant
        // sur une page mise en cache, l'onglet actif restait faux).
        var view = sender as View;
        if (view != null)
        {
            await view.ScaleTo(0.94, 70, Easing.CubicOut);
            await view.ScaleTo(1.0, 70, Easing.CubicIn);
        }

        NavigationShell.AllerVers(route);
    }
}
