using System.ComponentModel;

namespace LocatCam_mobile.Controls;

public partial class AnnonceCard : ContentView, INotifyPropertyChanged
{
    public static readonly BindableProperty TitreProperty =
        BindableProperty.Create(nameof(Titre), typeof(string), typeof(AnnonceCard), string.Empty);

    public static readonly BindableProperty PrixProperty =
        BindableProperty.Create(nameof(Prix), typeof(decimal), typeof(AnnonceCard), 0m);

    public static readonly BindableProperty VilleProperty =
        BindableProperty.Create(nameof(Ville), typeof(string), typeof(AnnonceCard), string.Empty);

    public static readonly BindableProperty QuartierProperty =
        BindableProperty.Create(nameof(Quartier), typeof(string), typeof(AnnonceCard), string.Empty);

    public static readonly BindableProperty TypeProperty =
        BindableProperty.Create(nameof(Type), typeof(string), typeof(AnnonceCard), string.Empty);

    public static readonly BindableProperty StatutProperty =
        BindableProperty.Create(nameof(Statut), typeof(string), typeof(AnnonceCard), string.Empty);

    public static readonly BindableProperty PhotoUrlProperty =
        BindableProperty.Create(nameof(PhotoUrl), typeof(string), typeof(AnnonceCard), string.Empty);

    public static readonly BindableProperty EstVerifieProperty =
        BindableProperty.Create(nameof(EstVerifie), typeof(bool), typeof(AnnonceCard), false);

    public string Titre
    {
        get => (string)GetValue(TitreProperty);
        set => SetValue(TitreProperty, value);
    }

    public decimal Prix
    {
        get => (decimal)GetValue(PrixProperty);
        set => SetValue(PrixProperty, value);
    }

    public string Ville
    {
        get => (string)GetValue(VilleProperty);
        set => SetValue(VilleProperty, value);
    }

    public string Quartier
    {
        get => (string)GetValue(QuartierProperty);
        set => SetValue(QuartierProperty, value);
    }

    public string Type
    {
        get => (string)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public string Statut
    {
        get => (string)GetValue(StatutProperty);
        set => SetValue(StatutProperty, value);
    }

    /// <summary>URL de la photo principale (optionnelle).</summary>
    public string PhotoUrl
    {
        get => (string)GetValue(PhotoUrlProperty);
        set => SetValue(PhotoUrlProperty, value);
    }

    /// <summary>Badge "Vérifié" affiché sur la photo.</summary>
    public bool EstVerifie
    {
        get => (bool)GetValue(EstVerifieProperty);
        set => SetValue(EstVerifieProperty, value);
    }

    public AnnonceCard()
    {
        InitializeComponent();
    }
}
