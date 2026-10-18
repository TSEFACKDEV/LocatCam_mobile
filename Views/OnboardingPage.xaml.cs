using LocatCam_mobile.Models;
using Microsoft.Maui.Controls;

namespace LocatCam_mobile.Views;

/// <summary>
/// Données d'un slide d'onboarding.
/// </summary>
public class OnboardingSlide
{
    public string Titre { get; set; } = string.Empty;
    public string SousTitre { get; set; } = string.Empty;
    public View Illustration { get; set; } = new ContentView();
}

public partial class OnboardingPage : ContentPage
{
    private int _position;

    public int Position
    {
        get => _position;
        set
        {
            _position = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(EstPremierSlide));
            OnPropertyChanged(nameof(EstDernierSlide));
        }
    }

    public bool EstPremierSlide => Position == 0;
    public bool EstDernierSlide => Position == Slides.Count - 1;

    public List<OnboardingSlide> Slides { get; } = new()
    {
        new OnboardingSlide
        {
            Titre = "Trouvez votre logement \u00e9tudiant",
            SousTitre = "Cherchez une chambre, un studio ou un appartement pr\u00e8s de votre universit\u00e9 au Cameroun.",
            Illustration = CreerIllustrationMaison()
        },
        new OnboardingSlide
        {
            Titre = "Filtrez par budget, quartier et type",
            SousTitre = "Affinez votre recherche par prix, meubl\u00e9, nombre de pi\u00e8ces et disponibilit\u00e9.",
            Illustration = CreerIllustrationFiltres()
        },
        new OnboardingSlide
        {
            Titre = "Contactez directement le bailleur",
            SousTitre = "Discutez avec le propri\u00e9taire via la messagerie in-app ou appelez-le en un clic.",
            Illustration = CreerIllustrationMessage()
        }
    };

    public OnboardingPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private void OnPasserClicked(object sender, EventArgs e)
    {
        TerminerOnboarding();
    }

    private void OnPrecedentClicked(object sender, EventArgs e)
    {
        if (Position > 0)
        {
            Position--;
            OnboardingCarousel.Position = Position;
        }
    }

    private void OnSuivantClicked(object sender, EventArgs e)
    {
        if (Position < Slides.Count - 1)
        {
            Position++;
            OnboardingCarousel.Position = Position;
        }
    }

    private void OnCommencerClicked(object sender, EventArgs e)
    {
        TerminerOnboarding();
    }

    private void TerminerOnboarding()
    {
        Preferences.Set("OnboardingDone", true);
        OuvrirPageSelonEtat();
    }

    private static void OuvrirPageSelonEtat()
    {
        var isLoggedIn = Preferences.Get("locatcam_is_logged_in", false);
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (isLoggedIn)
                await Shell.Current.GoToAsync("//HomePage");
            else
                await Shell.Current.GoToAsync("//LoginPage");
        });
    }

    // ------------------------------------------------------------------
    // Illustrations flat (compositions de formes MAUI, pas d'image externe)
    // ------------------------------------------------------------------

    private static View CreerIllustrationMaison()
    {
        var toit = new Border
        {
            WidthRequest = 100, HeightRequest = 50,
            BackgroundColor = (Color)App.Current.Resources["Accent"],
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        var corps = new Border
        {
            WidthRequest = 100, HeightRequest = 90,
            BackgroundColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 20, 0, 0)
        };
        var porte = new Border
        {
            WidthRequest = 24, HeightRequest = 36,
            BackgroundColor = (Color)App.Current.Resources["SurfaceSoft"],
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(0, 0, 0, 12)
        };

        var grid = new Grid { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        grid.Add(toit);
        grid.Add(corps);
        grid.Add(porte);
        return grid;
    }

    private static View CreerIllustrationFiltres()
    {
        var barre1 = new Border { WidthRequest = 70, HeightRequest = 10, BackgroundColor = (Color)App.Current.Resources["Accent"], HorizontalOptions = LayoutOptions.Center };
        var barre2 = new Border { WidthRequest = 110, HeightRequest = 10, BackgroundColor = (Color)App.Current.Resources["SurfaceSoft"], HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 8, 0, 0) };
        var barre3 = new Border { WidthRequest = 90, HeightRequest = 10, BackgroundColor = (Color)App.Current.Resources["Success"], HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 8, 0, 0) };

        var grid = new Grid { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        grid.Add(barre1);
        grid.Add(barre2);
        grid.Add(barre3);
        return grid;
    }

    private static View CreerIllustrationMessage()
    {
        var bulle1 = new Border { WidthRequest = 90, HeightRequest = 44, BackgroundColor = (Color)App.Current.Resources["Accent"], HorizontalOptions = LayoutOptions.Start };
        var bulle2 = new Border { WidthRequest = 110, HeightRequest = 44, BackgroundColor = (Color)App.Current.Resources["SurfaceSoft"], HorizontalOptions = LayoutOptions.End, Margin = new Thickness(0, 12, 0, 0) };

        var grid = new Grid { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        grid.Add(bulle1);
        grid.Add(bulle2);
        return grid;
    }
}
