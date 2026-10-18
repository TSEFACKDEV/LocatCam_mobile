using LocatCam_mobile.ViewModels;

namespace LocatCam_mobile.Views;

public partial class HomePage : ContentPage
{
	public HomePage()
	{
		InitializeComponent();
		var services = App.ServiceProvider;
		var vm = services?.GetRequiredService<ViewModels.HomeViewModel>();
		BindingContext = vm;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		if (BindingContext is ViewModels.HomeViewModel vm)
		{
			await vm.InitialiserAsync();
		}
	}
}
