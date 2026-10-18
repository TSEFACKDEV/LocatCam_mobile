using System.Net.Http;

namespace LocatCam_mobile.Services;

/// <summary>
/// Gestionnaire de token JWT pour les requêtes HTTP authentifiées.
///
/// Phase A : squelette — aucune injection de Bearer token, pas de refresh.
///
/// Phase B (à venir) :
///   - Injection du header Authorization: Bearer {accessToken}
///   - Détection des réponses 401
///   - Appel automatique de POST /api/auth/refresh
///   - Redirection vers Login si le refresh échoue (contournement anomalie B2)
/// </summary>
public class TokenHandler : DelegatingHandler
{
    public TokenHandler() : base(new HttpClientHandler()) { }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Phase A : pas de token injecté ici.
        // Le header Authorization sera ajouté en Phase B via IAuthService.
        return await base.SendAsync(request, cancellationToken);
    }
}
