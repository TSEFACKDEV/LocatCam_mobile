using Microsoft.Maui.Controls;

namespace LocatCam_mobile.Controls;

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
        MettreAJourApparence("HomePage");
    }

    private static void OnEstActifChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BottomNavBar nav)
            nav.MettreAJourApparence((string)newValue);
    }

    private void MettreAJourApparence(string actif)
    {
        var actifColor = (Color)App.Current.Resources["Accent"];
        var inactifColor = (Color)App.Current.Resources["InkMuted"];

        SetCouleurOnglet(IconeAccueil, LabelAccueil, actif == "HomePage", actifColor, inactifColor);
        SetCouleurOnglet(IconeCarte, LabelCarte, actif == "CartePage", actifColor, inactifColor);
        SetCouleurOnglet(IconeFavoris, LabelFavoris, actif == "FavorisPage", actifColor, inactifColor);
        SetCouleurOnglet(IconePublier, LabelPublier, actif == "PublierPage", actifColor, inactifColor);
        SetCouleurOnglet(IconeProfil, LabelProfil, actif == "ProfilPage", actifColor, inactifColor);
    }

    private static void SetCouleurOnglet(Label icone, Label label, bool estActif, Color actif, Color inactif)
    {
        icone.TextColor = estActif ? actif : inactif;
        label.TextColor = estActif ? actif : inactif;
    }

    private async void OnOngletTapped(object sender, TappedEventArgs e)
    {
        if (sender is TapGestureRecognizer tgr && tgr.AutomationId is string route)
        {
            // Trouver le grid parent pour l'animation scale
            var view = tgr.Parent as View;
            if (view != null)
            {
                await view.ScaleTo(0.92, 80, Easing.CubicOut);
                await view.ScaleTo(1.0, 80, Easing.CubicIn);
            }

            EstActif = route;

            if (route == "HomePage")
                await Shell.Current.GoToAsync("//HomePage");
            else if (route == "CartePage")
                await Shell.Current.GoToAsync("//CartePage");
            else if (route == "FavorisPage")
                await Shell.Current.GoToAsync("//FavorisPage");
            else if (route == "PublierPage")
                await Shell.Current.GoToAsync("//PublierPage");
            else if (route == "ProfilPage")
                await Shell.Current.GoToAsync("//ProfilPage");
        }
    }
}
