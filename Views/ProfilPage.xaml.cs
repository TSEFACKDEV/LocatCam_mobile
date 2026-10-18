using LocatCam_mobile.ViewModels;

namespace LocatCam_mobile.Views;

public partial class ProfilPage : ContentPage
{
	public ProfilPage()
	{
		InitializeComponent();
		var services = App.ServiceProvider;
		var vm = services?.GetRequiredService<ViewModels.ProfilViewModel>();
		BindingContext = vm;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		if (BindingContext is ViewModels.ProfilViewModel vm)
		{
			await vm.ChargerProfilAsync();
		}
	}
}
