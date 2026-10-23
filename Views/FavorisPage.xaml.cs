using LocatCam_mobile.Models;
using LocatCam_mobile.Services;

namespace LocatCam_mobile.Views;

public partial class FavorisPage : ContentPage
{
	public FavorisPage()
	{
		InitializeComponent();
		var services = App.ServiceProvider;
		var vm = services?.GetRequiredService<ViewModels.FavorisViewModel>();
		BindingContext = vm;

		NavigationShell.PageAffichee += OnPageAffichee;
	}

	private async void OnPageAffichee(string route)
	{
		if (string.Equals(route, "FavorisPage", StringComparison.OrdinalIgnoreCase) &&
			BindingContext is ViewModels.FavorisViewModel vm)
		{
			await vm.InitialiserAsync();
		}
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		if (BindingContext is ViewModels.FavorisViewModel vm)
		{
			await vm.InitialiserAsync();
		}
	}

	/// <summary>Ouvre le détail de l'annonce favorite sélectionnée.</summary>
	private void OnFavoriTapped(object sender, EventArgs e)
	{
		if (sender is Controls.AnnonceCard card && card.BindingContext is AnnonceDto annonce)
		{
			AnnonceSelection.Annonce = annonce;
			NavigationShell.AllerVersAvecRetour("DetailAnnoncePage", "FavorisPage");
		}
	}
}
