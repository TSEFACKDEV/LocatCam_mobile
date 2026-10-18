using LocatCam_mobile.ViewModels;

namespace LocatCam_mobile.Views;

public partial class RegisterPage : ContentPage
{
	public RegisterPage()
	{
		InitializeComponent();
		var services = Application.Current?.Handler?.MauiContext?.Services;
		var authService = services?.GetRequiredService<LocatCam_mobile.Services.Interfaces.IAuthService>();
		BindingContext = new RegisterViewModel(authService!);
	}
}
