using LocatCam_mobile.ViewModels;

namespace LocatCam_mobile.Views;

public partial class DetailAnnoncePage : ContentPage
{
	public DetailAnnoncePage()
	{
		InitializeComponent();
		var services = App.ServiceProvider;
		var vm = services?.GetRequiredService<ViewModels.DetailAnnonceViewModel>();
		BindingContext = vm;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		// L'ID de l'annonce peut être passé via les paramètres de navigation Shell
		if (BindingContext is ViewModels.DetailAnnonceViewModel vm)
		{
			// En attendant le support complet des paramètres Shell, on essaie de récupérer l'ID
			// depuis les query params de la route actuelle
			var currentRoute = Shell.Current.CurrentState.Location;
			System.Diagnostics.Debug.WriteLine($"[DetailAnnoncePage] Route actuelle: {currentRoute}");
			
			// Pour l'instant, on charge une annonce par défaut si aucun ID n'est fourni
			// Cela sera remplacé par la navigation paramétrée complète en Phase D finale
		}
	}
}
