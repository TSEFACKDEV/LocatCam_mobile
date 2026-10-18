namespace LocatCam_mobile.Models;

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public UserDto User { get; set; } = new();
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public bool EstVerifie { get; set; }
    public DateTime DateCreation { get; set; }
}
