namespace LocatCam_mobile.Models;

/// <summary>
/// Message envoyé après la mise à jour de la photo de profil afin que les
/// barres de navigation déjà affichées remplacent l'icône par la nouvelle photo.
/// </summary>
public sealed record PhotoProfilMessage(string? PhotoUrl);
