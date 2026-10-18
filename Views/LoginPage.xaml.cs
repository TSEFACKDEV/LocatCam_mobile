namespace LocatCam_mobile.Views;

using LocatCam_mobile.ViewModels;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
		var services = App.ServiceProvider;
		var authService = services?.GetRequiredService<LocatCam_mobile.Services.Interfaces.IAuthService>();
		BindingContext = new LoginViewModel(authService!);
	}
}
