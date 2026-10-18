using LocatCam_mobile.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace LocatCam_mobile.Views;

/// <summary>
/// Donn\u00e9es d'un slide d'onboarding.
/// </summary>
public class OnboardingSlide
{
    /// <summary>Glyphe Font Awesome affich\u00e9 dans la pastille.</summary>
    public string Icone { get; set; } = string.Empty;

    public string Titre { get; set; } = string.Empty;
    public string SousTitre { get; set; } = string.Empty;
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
            Icone = "\uf015", // house
            Titre = "Trouvez votre logement \u00e9tudiant",
            SousTitre = "Cherchez une chambre, un studio ou un appartement pr\u00e8s de votre universit\u00e9 au Cameroun."
        },
        new OnboardingSlide
        {
            Icone = "\uf1de", // sliders
            Titre = "Filtrez par budget, quartier et type",
            SousTitre = "Affinez votre recherche par prix, meubl\u00e9, nombre de pi\u00e8ces et disponibilit\u00e9."
        },
        new OnboardingSlide
        {
            Icone = "\uf4ad", // comment-dots
            Titre = "Contactez directement le bailleur",
            SousTitre = "Discutez avec le propri\u00e9taire via la messagerie in-app ou appelez-le en un clic."
        }
    };

    public OnboardingPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private void OnPasserClicked(object sender, EventArgs e) => TerminerOnboarding();

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

    private void OnCommencerClicked(object sender, EventArgs e) => TerminerOnboarding();

    /// <summary>
    /// Marque l'onboarding comme termin\u00e9 puis ouvre l'accueil (si connect\u00e9)
    /// ou la page de connexion.
    /// </summary>
    private async void TerminerOnboarding()
    {
        Preferences.Set("OnboardingDone", true);

        string route;
        try
        {
            var token = await SecureStorage.GetAsync("locatcam_access_token");
            route = string.IsNullOrWhiteSpace(token) ? "LoginPage" : "HomePage";
        }
        catch
        {
            route = "LoginPage";
        }

        NavigationShell.AllerVers(route);
    }
}
