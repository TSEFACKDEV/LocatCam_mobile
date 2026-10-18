using LocatCam_mobile.ViewModels;

namespace LocatCam_mobile.Views;

public partial class PublierPage : ContentPage
{
    private readonly PublierViewModel _vm;

    public PublierPage()
    {
        InitializeComponent();

        _vm = App.ServiceProvider?.GetRequiredService<PublierViewModel>()
            ?? throw new InvalidOperationException("PublierViewModel n'est pas enregistré dans le DI.");

        BindingContext = _vm;

        // Mettre à jour le titre de l'étape selon l'étape courante
        _vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(PublierViewModel.EtapeCourante))
                MettreAJourTitreEtape();
        };
        MettreAJourTitreEtape();
    }

    private void MettreAJourTitreEtape()
    {
        TitreEtapeLabel.Text = _vm.EtapeCourante switch
        {
            EtapePublier.Infos => "Informations",
            EtapePublier.Localisation => "Localisation",
            EtapePublier.Options => "Options",
            EtapePublier.Photos => "Photos",
            EtapePublier.Apercu => "Aperçu",
            EtapePublier.Termine => "Terminé",
            _ => "Publier"
        };
    }
}
