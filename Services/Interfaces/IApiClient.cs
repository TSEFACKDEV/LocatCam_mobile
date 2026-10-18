using LocatCam_mobile.Models;
using Refit;
using System.Collections.Generic;

namespace LocatCam_mobile.Services.Interfaces;

/// <summary>
/// Client Refit typé pour l'API LocatCam.
/// Tous les endpoints exposés par l'API sont déclarés ici.
/// Les implémentations sont générées automatiquement par Refit.
/// </summary>
[Headers("Accept: application/json")]
public interface IApiClient
{
    // ========================================================================
    // AUTHENTIFICATION
    // ========================================================================

    [Post("/api/auth/login")]
    Task<ApiResponse<LoginResponse>> Login([Body] LoginRequest request);

    [Post("/api/auth/register")]
    Task<ApiResponse<string>> Register([Body] RegisterRequest request);

    [Post("/api/auth/refresh")]
    Task<ApiResponse<LoginResponse>> Refresh([Body] string refreshToken);

    [Get("/api/auth/me")]
    Task<ApiResponse<UserDto>> Me();

    // ========================================================================
    // ANNONCES
    // ========================================================================

    [Get("/api/annonces")]
    Task<ApiResponse<PagedResult<AnnonceDto>>> GetAnnonces(
        [Query] decimal? prixMin = null,
        [Query] decimal? prixMax = null,
        [Query] string? quartier = null,
        [Query] string? ville = null,
        [Query] string? type = null,
        [Query] bool? estMeuble = null,
        [Query] bool? disponible = null,
        [Query] int page = 1,
        [Query] int pageSize = 10);

    [Get("/api/annonces/{id}")]
    Task<ApiResponse<AnnonceDto>> GetAnnonceById(string id);

    [Post("/api/annonces")]
    Task<ApiResponse<AnnonceDto>> CreateAnnonce([Body] AnnonceCreateDto dto);

    [Put("/api/annonces/{id}")]
    Task<ApiResponse<AnnonceDto>> UpdateAnnonce(string id, [Body] AnnonceUpdateDto dto);

    [Delete("/api/annonces/{id}")]
    Task<ApiResponse<string>> DeleteAnnonce(string id);

    [Get("/api/annonces/map")]
    Task<ApiResponse<List<AnnonceDto>>> GetAnnoncesMap(
        [Query] double minLat,
        [Query] double maxLat,
        [Query] double minLng,
        [Query] double maxLng);

    [Get("/api/annonces/mes-annonces")]
    Task<ApiResponse<List<AnnonceDto>>> GetMesAnnonces();

    // ========================================================================
    // FAVORIS
    // ========================================================================

    [Get("/api/favoris")]
    Task<ApiResponse<List<FavoriDto>>> GetFavoris();

    [Post("/api/favoris/{annonceId}")]
    Task<ApiResponse<string>> AddFavori(string annonceId);

    [Delete("/api/favoris/{annonceId}")]
    Task<ApiResponse<string>> RemoveFavori(string annonceId);

    // ========================================================================
    // MESSAGERIE
    // ========================================================================

    [Post("/api/messages")]
    Task<ApiResponse<MessageDto>> SendMessage([Body] MessageCreateDto dto);

    [Get("/api/messages/conversations")]
    Task<ApiResponse<List<MessageDto>>> GetConversations();

    [Get("/api/messages/conversation/{autreUserId}")]
    Task<ApiResponse<List<MessageDto>>> GetConversation(string autreUserId);

    [Put("/api/messages/{id}/lu")]
    Task<ApiResponse<string>> MarkAsRead(string id);

    // ========================================================================
    // PHOTOS (Cloudinary signature)
    // ========================================================================

    [Post("/api/photos/signature")]
    Task<ApiResponse<PhotoSignatureDto>> GetSignature();

    // ========================================================================
    // SIGNALEMENTS
    // ========================================================================

    [Post("/api/signalements")]
    Task<ApiResponse<SignalementDto>> CreateSignalement([Body] SignalementCreateDto dto);

    // ========================================================================
    // ADMIN
    // ========================================================================

    [Put("/api/admin/annonces/{id}/verifier")]
    Task<ApiResponse<string>> VerifierAnnonce(string id, [Query] bool verifier);

    [Put("/api/admin/users/{id}/bannir")]
    Task<ApiResponse<string>> BannirUser(string id, [Query] bool bannir);
}
