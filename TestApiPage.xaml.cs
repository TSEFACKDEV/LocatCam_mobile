using LocatCam_mobile.Models;
using LocatCam_mobile.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace LocatCam_mobile;

public partial class TestApiPage : ContentPage
{
    private readonly ApiService _apiService = new();
    private readonly ObservableCollection<AnnonceDto> _annonces = new();

    public TestApiPage()
    {
        InitializeComponent();
        AnnoncesCollection.ItemsSource = _annonces;

        // Pré-remplir avec le compte seedé
        EmailEntry.Text = "etudiant1@locatcam.cm";
        PasswordEntry.Text = "Etudiant@123";
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        LoginButton.IsEnabled = false;
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;

        try
        {
            var email = EmailEntry.Text?.Trim();
            var password = PasswordEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ResultLabel.Text = "Veuillez saisir l'email et le mot de passe.";
                ResultLabel.TextColor = Colors.Red;
                return;
            }

            var result = await _apiService.LoginAsync(email, password);

            if (result == null)
            {
                ResultLabel.Text = "Échec de la connexion (identifiants invalides ou erreur serveur).";
                ResultLabel.TextColor = Colors.Red;
                return;
            }

            // Stocker le token pour les appels suivants
            _apiService.SetBearerToken(result.AccessToken);

            ResultLabel.Text = $"Connexion OK ! Utilisateur : {result.User.Nom} ({result.User.Role}) - Token expirant le {result.Expiration:yyyy-MM-dd HH:mm}";
            ResultLabel.TextColor = Colors.Green;

            // Pré-remplir pour le test suivant
            LoadAnnoncesButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            ResultLabel.Text = $"Erreur : {ex.Message}";
            ResultLabel.TextColor = Colors.Red;
        }
        finally
        {
            LoginButton.IsEnabled = true;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnLoadAnnoncesClicked(object sender, EventArgs e)
    {
        LoadAnnoncesButton.IsEnabled = false;
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;

        try
        {
            var result = await _apiService.GetAnnoncesAsync(page: 1, pageSize: 10);

            if (result == null)
            {
                ResultLabel.Text = "Échec du chargement des annonces (token manquant ou expiré ?).";
                ResultLabel.TextColor = Colors.Red;
                return;
            }

            _annonces.Clear();
            var top3 = result.Items.Take(3).ToList();
            foreach (var annonce in top3)
            {
                _annonces.Add(annonce);
            }

            ResultLabel.Text = $"Annonces chargées : {result.TotalCount} au total (page {result.Page}/{result.TotalPages}). {top3.Count} affichées.";
            ResultLabel.TextColor = Colors.Green;
        }
        catch (Exception ex)
        {
            ResultLabel.Text = $"Erreur : {ex.Message}";
            ResultLabel.TextColor = Colors.Red;
        }
        finally
        {
            LoadAnnoncesButton.IsEnabled = false;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}
