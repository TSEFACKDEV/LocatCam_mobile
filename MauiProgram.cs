using LocatCam_mobile.Services;
using LocatCam_mobile.Services.Interfaces;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

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
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// ====================================================================
		// Phase A — Injection de dépendances (infrastructure de base)
		// ====================================================================

		// Client API Refit configuré sur http://169.58.69.36:5206/
		builder.Services.AddSingleton<ApiClient>();

		// Instance typée IApiClient (sans TokenHandler encore — ajouté en Phase B)
		builder.Services.AddSingleton<IApiClient>(sp =>
		{
			var apiClient = sp.GetRequiredService<ApiClient>();
			return apiClient.Create();
		});

		// AuthService (implémentation Phase B — enregistrement préparatoire)
		builder.Services.AddSingleton<IAuthService, AuthService>();

		return builder.Build();
	}
}
