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
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		if (BindingContext is ViewModels.FavorisViewModel vm)
		{
			await vm.InitialiserAsync();
		}
	}

	/// <summary>Ouvre le d\u00e9tail de l'annonce favorite s\u00e9lectionn\u00e9e.</summary>
	private void OnFavoriSelectionne(object sender, SelectionChangedEventArgs e)
	{
		if (e.CurrentSelection.FirstOrDefault() is AnnonceDto annonce)
		{
			AnnonceSelection.Annonce = annonce;
			NavigationShell.AllerVersAvecRetour("DetailAnnoncePage");

			if (sender is CollectionView collection)
				collection.SelectedItem = null;
		}
	}
}
