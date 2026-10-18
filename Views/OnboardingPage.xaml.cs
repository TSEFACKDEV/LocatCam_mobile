using LocatCam_mobile.Services;
using LocatCam_mobile.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace LocatCam_mobile.Views;

/// <summary>
/// Données d'une étape d'onboarding.
/// </summary>
public class OnboardingSlide
{
    /// <summary>Illustration unDraw (fichier de Resources/Images).</summary>
    public string Illustration { get; set; } = string.Empty;

    /// <summary>Petite étiquette affichée au-dessus du titre.</summary>
    public string Etiquette { get; set; } = string.Empty;

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
            if (_position == value)
                return;

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
            Illustration = "onboarding_house.svg",
            Etiquette = "Recherche",
            Titre = "Trouvez\u00A0votre chez-vous",
            SousTitre = "Des chambres, studios et appartements proches de votre université, à Douala et à Yaoundé."
        },
        new OnboardingSlide
        {
            Illustration = "onboarding_rent.svg",
            Etiquette = "Filtres",
            Titre = "Le bon logement, au bon prix",
            SousTitre = "Affinez par budget, quartier, type et équipements. Gardez vos favoris pour comparer sereinement."
        },
        new OnboardingSlide
        {
            Illustration = "onboarding_chat.svg",
            Etiquette = "Contact",
            Titre = "Discutez avec le bailleur",
            SousTitre = "Posez vos questions et planifiez une visite directement depuis l'application, en toute simplicité."
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
    /// Marque l'onboarding comme terminé puis ouvre l'accueil (si connecté)
    /// ou la page de connexion.
    /// </summary>
    private async void TerminerOnboarding()
    {
        Preferences.Set("OnboardingDone", true);

        bool connecte;
        try
        {
            var authService = App.ServiceProvider?.GetService<IAuthService>();
            connecte = authService != null && await authService.IsLoggedInAsync();
        }
        catch
        {
            connecte = false;
        }

        NavigationShell.AllerVers(connecte ? "HomePage" : "LoginPage");
    }
}
