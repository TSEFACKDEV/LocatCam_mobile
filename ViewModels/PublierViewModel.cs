using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocatCam_mobile.Models;
using LocatCam_mobile.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Globalization;

namespace LocatCam_mobile.ViewModels;

/// <summary>
/// Étapes du formulaire de publication d'annonce (Phase F).
/// </summary>
public enum EtapePublier
{
    Infos = 0,      // titre, description, prix, type, quartier, ville, adresse
    Localisation,   // latitude / longitude (saisie ou carte)
    Options,        // meublé, nombre de pièces, disponible
    Photos,         // DÉSACTIVÉES — contournement anomalie B1
    Apercu,         // récapitulatif avant publication
    Termine         // annonce publiée avec succès
}

/// <summary>
/// ViewModel de la page de publication d'annonce (Phase F).
/// Formule étape par étape. L'upload de photos est désactivé tant que
/// l'anomalie API B1 (POST /api/photos/signature sans auth) n'est pas corrigée.
/// </summary>
public partial class PublierViewModel : ObservableObject
{
    private readonly IAnnonceService _annonceService;
    private readonly IFavoriService _favoriService;

    public PublierViewModel(IAnnonceService annonceService, IFavoriService favoriService)
    {
        _annonceService = annonceService;
        _favoriService = favoriService;
    }

    // ------------------------------------------------------------------
    // État du formulaire
    // ------------------------------------------------------------------

    [ObservableProperty]
    private EtapePublier _etapeCourante = EtapePublier.Infos;

    [ObservableProperty]
    private string _titre = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _prixTexte = string.Empty;

    [ObservableProperty]
    private string _type = string.Empty;

    [ObservableProperty]
    private string _quartier = string.Empty;

    [ObservableProperty]
    private string _ville = string.Empty;

    [ObservableProperty]
    private string _adresse = string.Empty;

    [ObservableProperty]
    private double _latitude;

    [ObservableProperty]
    private double _longitude;

    [ObservableProperty]
    private bool _estMeuble;

    [ObservableProperty]
    private int _nombrePieces = 1;

    [ObservableProperty]
    private bool _disponible = true;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _messageErreur = string.Empty;

    [ObservableProperty]
    private string _messageSucces = string.Empty;

    [ObservableProperty]
    private AnnonceDto? _annonceCreee;

    /// <summary>Types de bien proposés.</summary>
    public ObservableCollection<string> TypesBien { get; } = new()
    {
        "Appartement", "Maison", "Studio", "Chambre"
    };

    // ------------------------------------------------------------------
    // Visibilité des étapes (pour le XAML)
    // ------------------------------------------------------------------

    public bool EstEtapeInfos => EtapeCourante == EtapePublier.Infos;
    public bool EstEtapeLocalisation => EtapeCourante == EtapePublier.Localisation;
    public bool EstEtapeOptions => EtapeCourante == EtapePublier.Options;
    public bool EstEtapePhotos => EtapeCourante == EtapePublier.Photos;
    public bool EstEtapeApercu => EtapeCourante == EtapePublier.Apercu;
    public bool EstEtapeTermine => EtapeCourante == EtapePublier.Termine;

    private void NotifierEtapes()
    {
        OnPropertyChanged(nameof(EstEtapeInfos));
        OnPropertyChanged(nameof(EstEtapeLocalisation));
        OnPropertyChanged(nameof(EstEtapeOptions));
        OnPropertyChanged(nameof(EstEtapePhotos));
        OnPropertyChanged(nameof(EstEtapeApercu));
        OnPropertyChanged(nameof(EstEtapeTermine));
    }

    // ------------------------------------------------------------------
    // Navigation entre étapes
    // ------------------------------------------------------------------

    [RelayCommand]
    private void Suivant()
    {
        MessageErreur = string.Empty;

        // Validation de l'étape courante avant d'avancer
        if (!ValiderEtapeCourante())
            return;

        if (EtapeCourante < EtapePublier.Termine)
        {
            EtapeCourante++;
            NotifierEtapes();
        }
    }

    [RelayCommand]
    private void Precedent()
    {
        MessageErreur = string.Empty;
        if (EtapeCourante > EtapePublier.Infos)
        {
            EtapeCourante--;
            NotifierEtapes();
        }
    }

    /// <summary>Valide les champs de l'étape courante.</summary>
    private bool ValiderEtapeCourante()
    {
        switch (EtapeCourante)
        {
            case EtapePublier.Infos:
                if (string.IsNullOrWhiteSpace(Titre))
                {
                    MessageErreur = "Le titre est requis.";
                    return false;
                }
                if (Titre.Length < 5)
                {
                    MessageErreur = "Le titre doit contenir au moins 5 caractères.";
                    return false;
                }
                if (string.IsNullOrWhiteSpace(Description))
                {
                    MessageErreur = "La description est requise.";
                    return false;
                }
                if (Description.Length < 10)
                {
                    MessageErreur = "La description doit contenir au moins 10 caractères.";
                    return false;
                }
                if (!decimal.TryParse(PrixTexte, NumberStyles.Any, CultureInfo.InvariantCulture, out var prix) || prix <= 0)
                {
                    MessageErreur = "Le prix doit être un nombre supérieur à 0.";
                    return false;
                }
                if (string.IsNullOrWhiteSpace(Type))
                {
                    MessageErreur = "Le type de bien est requis.";
                    return false;
                }
                if (string.IsNullOrWhiteSpace(Quartier))
                {
                    MessageErreur = "Le quartier est requis.";
                    return false;
                }
                if (string.IsNullOrWhiteSpace(Ville))
                {
                    MessageErreur = "La ville est requise.";
                    return false;
                }
                if (string.IsNullOrWhiteSpace(Adresse))
                {
                    MessageErreur = "L'adresse est requise.";
                    return false;
                }
                break;

            case EtapePublier.Localisation:
                if (Latitude == 0 && Longitude == 0)
                {
                    MessageErreur = "Veuillez indiquer une localisation (latitude / longitude).";
                    return false;
                }
                break;

            case EtapePublier.Options:
                if (NombrePieces < 1)
                {
                    MessageErreur = "Le nombre de pièces doit être d'au moins 1.";
                    return false;
                }
                break;

            case EtapePublier.Photos:
                // Photos désactivées (contournement B1) — on passe directement
                break;
        }

        return true;
    }

    // ------------------------------------------------------------------
    // Publication
    // ------------------------------------------------------------------

    [RelayCommand]
    private async Task PublierAsync()
    {
        MessageErreur = string.Empty;
        MessageSucces = string.Empty;

        if (!ValiderEtapeCourante())
            return;

        IsLoading = true;

        try
        {
            var dto = new AnnonceCreateDto
            {
                Titre = Titre,
                Description = Description,
                Prix = decimal.Parse(PrixTexte, CultureInfo.InvariantCulture),
                Type = Type,
                Quartier = Quartier,
                Ville = Ville,
                Adresse = Adresse,
                Latitude = Latitude,
                Longitude = Longitude,
                EstMeuble = EstMeuble,
                NombrePieces = NombrePieces,
                Disponible = Disponible
            };

            var annonce = await _annonceService.CreateAnnonceAsync(dto);

            if (annonce == null)
            {
                MessageErreur = "Impossible de publier l'annonce. Réessayez plus tard.";
                return;
            }

            AnnonceCreee = annonce;
            EtapeCourante = EtapePublier.Termine;
            NotifierEtapes();

            MessageSucces = "Votre annonce a été publiée avec succès !";
        }
        catch (Exception ex)
        {
            MessageErreur = "Erreur lors de la publication : " + ex.Message;
            System.Diagnostics.Debug.WriteLine($"[PublierViewModel] Erreur: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Réinitialise le formulaire pour une nouvelle publication.</summary>
    [RelayCommand]
    private void NouvelleAnnonce()
    {
        Titre = string.Empty;
        Description = string.Empty;
        PrixTexte = string.Empty;
        Type = string.Empty;
        Quartier = string.Empty;
        Ville = string.Empty;
        Adresse = string.Empty;
        Latitude = 0;
        Longitude = 0;
        EstMeuble = false;
        NombrePieces = 1;
        Disponible = true;
        MessageErreur = string.Empty;
        MessageSucces = string.Empty;
        AnnonceCreee = null;
        EtapeCourante = EtapePublier.Infos;
        NotifierEtapes();
    }
}
