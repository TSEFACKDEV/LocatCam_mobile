using LocatCam_mobile.Services;
using Microsoft.Maui.Controls;

namespace LocatCam_mobile.Controls;

/// <summary>
/// Barre de navigation basse personnalis\u00e9e (5 onglets).
/// L'onglet actif est mis en valeur par une pastille bleu clair + ic\u00f4ne bleue.
/// </summary>
public partial class BottomNavBar : ContentView
{
    public static readonly BindableProperty EstActifProperty =
        BindableProperty.Create(nameof(EstActif), typeof(string), typeof(BottomNavBar), "HomePage",
            propertyChanged: OnEstActifChanged);

    public string EstActif
    {
        get => (string)GetValue(EstActifProperty);
        set => SetValue(EstActifProperty, value);
    }

    public BottomNavBar()
    {
        InitializeComponent();
        MettreAJourApparence(EstActif);
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Application.Current != null)
            Application.Current.RequestedThemeChanged += OnThemeChanged;
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
            // Ignorer : couleur par d\u00e9faut ci-dessous
        }

        return Colors.Gray;
    }

    private async void OnOngletTapped(object sender, TappedEventArgs e)
    {
        if (sender is not TapGestureRecognizer tgr || tgr.Parent is not VisualElement element)
            return;

        var route = element.AutomationId;
        if (string.IsNullOrWhiteSpace(route))
            return;

        // Animation scale (80 ms aller/retour) puis navigation
        EstActif = route;

        var view = element as View;
        if (view != null)
        {
            await view.ScaleTo(0.94, 70, Easing.CubicOut);
            await view.ScaleTo(1.0, 70, Easing.CubicIn);
        }

        NavigationShell.AllerVers(route);
    }
}
