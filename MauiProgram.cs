using CommunityToolkit.Maui;
using LocatCam_mobile.Services;
using LocatCam_mobile.Services.Interfaces;
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
		// Phase A & B — Injection de dépendances
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
		// Casser la dépendance circulaire : TokenHandler appelle AuthService pour le refresh
		tokenHandler.RefreshTokenFunc = authService.RefreshTokenAsync;
		builder.Services.AddSingleton<IAuthService>(authService);

		var app = builder.Build();
		App.ServiceProvider = app.Services;
		return app;
	}
}
