using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace LocatCam_mobile;

public partial class App : Application
{
	public static IServiceProvider? ServiceProvider { get; set; }

	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var onboardingDone = Preferences.Get("OnboardingDone", false);
		var token = SecureStorage.GetAsync("locatcam_access_token").Result;

		Page startPage;
		if (!onboardingDone)
		{
			startPage = new Views.OnboardingPage();
		}
		else if (!string.IsNullOrWhiteSpace(token))
		{
			startPage = new Views.HomePage();
		}
		else
		{
			startPage = new Views.LoginPage();
		}

		return new Window(startPage);
	}
}
