using LocatCam_mobile.ViewModels;

namespace LocatCam_mobile.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
		var services = Application.Current?.Handler?.MauiContext?.Services;
		var authService = services?.GetRequiredService<LocatCam_mobile.Services.Interfaces.IAuthService>();
		BindingContext = new LoginViewModel(authService!);
	}
}
