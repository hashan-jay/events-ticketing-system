using System.ComponentModel.DataAnnotations;

namespace EventsLab.Api.DTOs;

public class LoginRequest
{
    [Required, MaxLength(80)]
    public string Username { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
