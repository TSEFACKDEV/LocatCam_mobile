using CommunityToolkit.Maui;
using LocatCam_mobile.Services;
using LocatCam_mobile.Services.Interfaces;
using Microsoft.Extensions.Logging;
using SQLite;
using System.Threading.Tasks;

namespace LocatCam_mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				// Plus Jakarta Sans (variable font) — police principale du design system
				fonts.AddFont("PlusJakartaSans-Regular.ttf", "PlusJakartaSans");
				// Font Awesome — icônes vectorielles (solid + regular)
				fonts.AddFont("fa-solid-900.ttf", "FASolid");
				fonts.AddFont("fa-regular-400.ttf", "FARegular");
				// Fallback système
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// ====================================================================
		// Phase A & B & C — Injection de dépendances
		// ====================================================================

		// 1. TokenHandler (singleton) — gère le Bearer token et le refresh
		var tokenHandler = new TokenHandler();
		builder.Services.AddSingleton<TokenHandler>(tokenHandler);

		// 2. ApiClient (singleton) — fabrique Refit
		var apiClient = new ApiClient();
		builder.Services.AddSingleton<ApiClient>(apiClient);

		// 3. IApiClient (singleton) — instance typée Refit avec TokenHandler
		var apiClientInstance = apiClient.Create(tokenHandler);
		builder.Services.AddSingleton<IApiClient>(apiClientInstance);

		// 4. AuthService (singleton) — reçoit IApiClient et TokenHandler
		var authService = new AuthService(apiClientInstance, tokenHandler);
		tokenHandler.RefreshTokenFunc = authService.RefreshTokenAsync;
		builder.Services.AddSingleton<IAuthService>(authService);

		// ====================================================================
		// Phase C — Services métier + base SQLite
		// ====================================================================

		// Base SQLite locale (chemin dans le dossier des données de l'app)
		var dbPath = Path.Combine(FileSystem.AppDataDirectory, "locatcam.db3");
		var localDb = new LocalDbService(dbPath);
		builder.Services.AddSingleton<ILocalDbService>(localDb);

		// Services métier
		builder.Services.AddSingleton<IAnnonceService, AnnonceService>();
		builder.Services.AddSingleton<IFavoriService, FavoriService>();
		builder.Services.AddSingleton<IMessageService, MessageService>();

		// ViewModels (transient pour avoir une instance fraîche par page)
		builder.Services.AddTransient<ViewModels.HomeViewModel>();
		builder.Services.AddTransient<ViewModels.DetailAnnonceViewModel>();
		builder.Services.AddTransient<ViewModels.FavorisViewModel>();
		builder.Services.AddTransient<ViewModels.ProfilViewModel>();
		builder.Services.AddTransient<ViewModels.MapViewModel>();
		builder.Services.AddTransient<ViewModels.PublierViewModel>();
		builder.Services.AddTransient<ViewModels.MessagerieViewModel>();

		// ConnectivityService (singleton) — détection réseau pour le mode hors-ligne
		builder.Services.AddSingleton<ConnectivityService>();

		var app = builder.Build();
		App.ServiceProvider = app.Services;

		// Initialisation de la base SQLite locale au démarrage (mode hors-ligne)
		// Appel non bloquant pour éviter tout deadlock sur le thread UI Android
		_ = Task.Run(async () =>
		{
			try
			{
				var dbService = app.Services.GetRequiredService<ILocalDbService>();
				await dbService.InitializeAsync();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[MauiProgram] Erreur initialisation base SQLite: {ex.Message}");
			}
		});

		return app;
	}
}
