namespace LocatCam_mobile.Views;

using LocatCam_mobile.Services.Interfaces;
using LocatCam_mobile.ViewModels;
using Microsoft.Extensions.DependencyInjection;

public partial class LoginPage : ContentPage
{
	private const string IconeOeilOuvert = "\uf06e";
	private const string IconeOeilBarre = "\uf070";

	public LoginPage()
	{
		InitializeComponent();
		var services = App.ServiceProvider;
		var authService = services?.GetRequiredService<IAuthService>();
		BindingContext = new LoginViewModel(authService!);
	}

	/// <summary>Affiche ou masque le mot de passe saisi.</summary>
	private void OnToggleMotDePasse(object sender, TappedEventArgs e)
	{
		MotDePasseEntry.IsPassword = !MotDePasseEntry.IsPassword;
		ToggleMotDePasse.Text = MotDePasseEntry.IsPassword ? IconeOeilOuvert : IconeOeilBarre;
	}
}
