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
	/// Ouvre le d\u00e9tail de l'annonce s\u00e9lectionn\u00e9e (tap sur une carte).
	/// </summary>
	private void OnAnnonceSelectionnee(object sender, SelectionChangedEventArgs e)
	{
		if (e.CurrentSelection.FirstOrDefault() is AnnonceDto annonce)
		{
			// M\u00e9moriser l'annonce choisie puis ouvrir le d\u00e9tail
			AnnonceSelection.Annonce = annonce;
			NavigationShell.AllerVersAvecRetour("DetailAnnoncePage");

			// Retirer la s\u00e9lection (visuel) sans re-d\u00e9clencher l'\u00e9v\u00e9nement
			if (sender is CollectionView collection)
				collection.SelectedItem = null;
		}
	}
}
