namespace LocatCam_mobile.Views;

using LocatCam_mobile.ViewModels;

public partial class RegisterPage : ContentPage
{
	public RegisterPage()
	{
		InitializeComponent();
		var services = App.ServiceProvider;
		var authService = services?.GetRequiredService<LocatCam_mobile.Services.Interfaces.IAuthService>();
		BindingContext = new RegisterViewModel(authService!);
	}
}
