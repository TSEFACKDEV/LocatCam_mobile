using System.Windows.Input;

namespace LocatCam_mobile.Controls;

public partial class EmptyStateView : ContentView
{
    public static readonly BindableProperty IconeProperty =
        BindableProperty.Create(nameof(Icone), typeof(string), typeof(EmptyStateView), "📭");

    public static readonly BindableProperty TitreProperty =
        BindableProperty.Create(nameof(Titre), typeof(string), typeof(EmptyStateView), string.Empty);

    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(EmptyStateView), string.Empty);

    public static readonly BindableProperty BoutonTexteProperty =
        BindableProperty.Create(nameof(BoutonTexte), typeof(string), typeof(EmptyStateView), string.Empty);

    public static readonly BindableProperty BoutonCommandeProperty =
        BindableProperty.Create(nameof(BoutonCommande), typeof(ICommand), typeof(EmptyStateView), null);

    public static readonly BindableProperty AfficherBoutonProperty =
        BindableProperty.Create(nameof(AfficherBouton), typeof(bool), typeof(EmptyStateView), false);

    public string Icone
    {
        get => (string)GetValue(IconeProperty);
        set => SetValue(IconeProperty, value);
    }

    public string Titre
    {
        get => (string)GetValue(TitreProperty);
        set => SetValue(TitreProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public string BoutonTexte
    {
        get => (string)GetValue(BoutonTexteProperty);
        set => SetValue(BoutonTexteProperty, value);
    }

    public ICommand BoutonCommande
    {
        get => (ICommand)GetValue(BoutonCommandeProperty);
        set => SetValue(BoutonCommandeProperty, value);
    }

    public bool AfficherBouton
    {
        get => (bool)GetValue(AfficherBoutonProperty);
        set => SetValue(AfficherBoutonProperty, value);
    }

    public EmptyStateView()
    {
        InitializeComponent();
    }
}
