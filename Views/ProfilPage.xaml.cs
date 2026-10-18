using LocatCam_mobile.ViewModels;
using Microsoft.Maui.Controls;

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
			MettreAJourAvatar(vm.Nom);
		}

		// Synchroniser l'interrupteur avec le th\u00e8me actuel
		ThemeSwitch.IsToggled = Application.Current?.RequestedTheme == AppTheme.Dark;
	}

	/// <summary>Affiche l'initiale du nom dans l'avatar.</summary>
	private void MettreAJourAvatar(string? nom)
	{
		AvatarInitiale.Text = string.IsNullOrWhiteSpace(nom)
			? "U"
			: nom.Trim()[0].ToString().ToUpperInvariant();
	}

	/// <summary>Bascule r\u00e9ellement le th\u00e8me clair / sombre de l'application.</summary>
	private void OnThemeToggled(object sender, ToggledEventArgs e)
	{
		if (Application.Current != null)
			Application.Current.UserAppTheme = e.Value ? AppTheme.Dark : AppTheme.Light;
	}
}
