using LocatCam_mobile.ViewModels;

namespace LocatCam_mobile.Views;

public partial class MessageriePage : ContentPage
{
    private readonly MessagerieViewModel _vm;

    public MessageriePage()
    {
        InitializeComponent();

        _vm = App.ServiceProvider?.GetRequiredService<MessagerieViewModel>()
            ?? throw new InvalidOperationException("MessagerieViewModel n'est pas enregistré dans le DI.");

        BindingContext = _vm;

        // Charger les conversations à l'apparition de la page
        _ = _vm.ChargerConversationsAsync();
    }
}
