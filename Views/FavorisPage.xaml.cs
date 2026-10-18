using LocatCam_mobile.ViewModels;

namespace LocatCam_mobile.Views;

public partial class FavorisPage : ContentPage
{
	public FavorisPage()
	{
		InitializeComponent();
		var services = App.ServiceProvider;
		var vm = services?.GetRequiredService<ViewModels.FavorisViewModel>();
		BindingContext = vm;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		if (BindingContext is ViewModels.FavorisViewModel vm)
		{
			await vm.InitialiserAsync();
		}
	}
}
