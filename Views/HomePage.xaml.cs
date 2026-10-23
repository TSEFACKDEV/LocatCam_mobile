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

		NavigationShell.PageAffichee += OnPageAffichee;
	}

	private async void OnPageAffichee(string route)
	{
		if (string.Equals(route, "HomePage", StringComparison.OrdinalIgnoreCase) &&
			BindingContext is ViewModels.HomeViewModel vm)
		{
			await vm.InitialiserAsync();
		}
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

	/// <summary>Ouvre la messagerie depuis l'icône de l'en-tête.</summary>
	private void OnMessagesClicked(object sender, TappedEventArgs e)
		=> NavigationShell.AllerVersAvecRetour("MessageriePage", "HomePage");

	private static void OuvrirDetail(AnnonceDto annonce)
	{
		AnnonceSelection.Annonce = annonce;
		NavigationShell.AllerVersAvecRetour("DetailAnnoncePage", "HomePage");
	}
}
