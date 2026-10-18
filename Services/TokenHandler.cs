using System.Net;
using LocatCam_mobile.Models;

namespace LocatCam_mobile.Services;

/// <summary>
/// Gestionnaire de token JWT pour les requêtes HTTP authentifiées.
///
/// Phase B : implémentation complète.
///   - Injection du header Authorization: Bearer {accessToken}
///   - Détection des réponses 401
///   - Appel du callback de refresh (défini par AuthService pour casser la dépendance circulaire)
///   - Si le refresh échoue, la réponse 401 remonte pour redirection vers Login
/// </summary>
public class TokenHandler : DelegatingHandler
{
    /// <summary>
    /// Callback de rafraîchissement de token, assigné par <see cref="AuthService"/>
    /// après construction pour éviter la dépendance circulaire.
    /// </summary>
    public Func<Task<bool>>? RefreshTokenFunc { get; set; }

    public TokenHandler() : base(new HttpClientHandler()) { }

    /// <summary>
    /// Définit le token d'accès à injecter dans les requêtes.
    /// Appelé par <see cref="AuthService"/> après un login ou un refresh réussi.
    /// </summary>
    public void SetAccessToken(string? accessToken)
    {
        AccessToken = accessToken;
    }

    public string? AccessToken { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Injecter le Bearer token si disponible
        if (!string.IsNullOrWhiteSpace(AccessToken))
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // Si 401 et que ce n'est pas déjà une requête de refresh, tenter le refresh
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized &&
            !request.RequestUri?.AbsolutePath.Contains("/auth/refresh") == true &&
            RefreshTokenFunc != null)
        {
            var refreshed = await RefreshTokenFunc.Invoke();

            if (refreshed && !string.IsNullOrWhiteSpace(AccessToken))
            {
                // Rejouer la requête avec le nouveau token
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
                return await base.SendAsync(request, cancellationToken);
            }

            // Refresh échoué : laisser la réponse 401 remonter
            // (l'UI redirigera vers Login - contournement anomalie B2)
        }

        return response;
    }
}
