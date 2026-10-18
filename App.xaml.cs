using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

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
		// Le Shell est la racine de la fenêtre. Sa première page est la page de
		// démarrage (StartupPage), qui choisit ensuite la bonne destination :
		// onboarding, connexion ou accueil.
		return new Window(new AppShell());
	}
}
