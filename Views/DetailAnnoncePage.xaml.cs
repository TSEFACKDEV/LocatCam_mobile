using LocatCam_mobile.Models;
using LocatCam_mobile.Services;
using LocatCam_mobile.ViewModels;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

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

		// Charger l'annonce sélectionnée depuis l'accueil, les favoris ou la carte
		if (BindingContext is DetailAnnonceViewModel vm && AnnonceSelection.Annonce is { } annonce)
		{
			await vm.ChargerAsync(annonce.Id.ToString());
		}
	}

	/// <summary>Bouton retour (revient à la page d'origine).</summary>
	private void OnRetourClicked(object sender, TappedEventArgs e)
		=> NavigationShell.Revenir();

	/// <summary>Ouvre la messagerie pour contacter le bailleur.</summary>
	private void OnContacterClicked(object sender, EventArgs e)
	{
		if (BindingContext is DetailAnnonceViewModel vm && vm.Annonce is { } annonce)
		{
			ConversationSelection.Conversation = new ConversationDto
			{
				UtilisateurId = annonce.UserId,
				UtilisateurNom = string.IsNullOrWhiteSpace(annonce.NomBailleur) ? "Bailleur" : annonce.NomBailleur,
				AnnonceId = annonce.Id,
				AnnonceTitre = annonce.Titre
			};
		}

		NavigationShell.AllerVersAvecRetour("MessageriePage", "DetailAnnoncePage");
	}

	/// <summary>Appelle le bailleur (si son numéro est disponible).</summary>
	private void OnAppelerClicked(object sender, EventArgs e)
	{
		if (BindingContext is not DetailAnnonceViewModel vm)
			return;

		var telephone = vm.Annonce?.TelephoneBailleur;
		if (string.IsNullOrWhiteSpace(telephone))
			return;

		try
		{
			if (PhoneDialer.Default.IsSupported)
				PhoneDialer.Default.Open(telephone);
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[DetailAnnoncePage] Appel impossible: {ex.Message}");
		}
	}
}
