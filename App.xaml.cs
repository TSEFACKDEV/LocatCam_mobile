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
		// Le Shell est la racine de la fenêtre : indispensable pour que toutes
		// les navigations (GoToAsync, sélection de page) fonctionnent.
		var shell = new AppShell();

		// Premier lancement : afficher l'onboarding (sélection directe, sans flash).
		if (!Preferences.Get("OnboardingDone", false))
			SelectionnerPage(shell, "OnboardingPage");

		return new Window(shell);
	}

	/// <summary>Sélectionne la page racine du Shell portant cette route.</summary>
	private static void SelectionnerPage(Shell shell, string route)
	{
		foreach (var item in shell.Items)
		{
			foreach (var section in item.Items)
			{
				foreach (var content in section.Items)
				{
					if (string.Equals(content.Route, route, StringComparison.OrdinalIgnoreCase))
					{
						section.CurrentItem = content;
						item.CurrentItem = section;
						shell.CurrentItem = item;
						return;
					}
				}
			}
		}
	}
}
