using LocatCam_mobile.Services.Interfaces;
using Refit;
using System.Net.Http.Headers;

namespace LocatCam_mobile.Services;

/// <summary>
/// Fabrique et configuration du client Refit pour l'API LocatCam.
/// </summary>
public class ApiClient
{
    private const string BaseUrl = "http://169.58.69.36:5206/";

    /// <summary>
    /// Crée une instance typée de <see cref="IApiClient"/> configurée avec l'URL de base
    /// et les headers par défaut. Le handler optionnel permet d'injecter un
    /// <see cref="TokenHandler"/> (Phase B) pour la gestion du Bearer token.
    /// </summary>
    public IApiClient Create(HttpMessageHandler? handler = null)
    {
        var httpClient = handler != null
            ? new HttpClient(handler)
            : new HttpClient();

        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.Timeout = TimeSpan.FromSeconds(30);
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        return RestService.For<IApiClient>(httpClient);
    }
}
