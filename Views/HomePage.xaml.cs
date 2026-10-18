using LocatCam_mobile.Models;
using LocatCam_mobile.Services;
using LocatCam_mobile.ViewModels;

namespace LocatCam_mobile.Views;

public partial class HomePage : ContentPage
{
	public HomePage()
	{
		InitializeComponent();
		var services = App.ServiceProvider;
		var vm = services?.GetRequiredService<ViewModels.HomeViewModel>();
		BindingContext = vm;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		if (BindingContext is ViewModels.HomeViewModel vm)
		{
			await vm.InitialiserAsync();
		}
	}

	/// <summary>
	/// Ouvre le détail de l'annonce sélectionnée (tap sur une carte).
	/// </summary>
	private void OnAnnonceTapped(object sender, EventArgs e)
	{
		if (sender is Controls.AnnonceCard card && card.BindingContext is AnnonceDto annonce)
			OuvrirDetail(annonce);
	}

	private static void OuvrirDetail(AnnonceDto annonce)
	{
		AnnonceSelection.Annonce = annonce;
		NavigationShell.AllerVersAvecRetour("DetailAnnoncePage", "HomePage");
	}
}
