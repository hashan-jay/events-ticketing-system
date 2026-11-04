using EventsLab.Api.Data;
using EventsLab.Api.DTOs;
using EventsLab.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventsLab.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtTokenService _tokens;
    private readonly AuditService _audit;

    public AuthController(AppDbContext db, JwtTokenService tokens, AuditService audit)
    {
        _db = db;
        _tokens = tokens;
        _audit = audit;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var username = request.Username.Trim();
        var password = request.Password.Trim();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        if (user is null || !user.IsActive || !PasswordHasher.Verify(password, user.PasswordHash))
        {
            await _audit.WriteAsync(
                AuditActions.LoginFailed,
                "user",
                user?.Id.ToString(),
                $"Failed sign-in attempt for '{username}'.",
                username: username);
            return Unauthorized(new { message = "Invalid username or password." });
        }

        await _audit.WriteAsync(
            AuditActions.LoginSucceeded,
            "user",
            user.Id.ToString(),
            $"{user.FullName} signed in.",
            userId: user.Id,
            username: user.Username);

        return Ok(new AuthResponse
        {
            Token = _tokens.CreateToken(user),
            User = DtoMapper.ToUser(user)
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Me()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idValue, out var userId))
        {
            return Unauthorized();
        }

        var user = await _db.Users.FindAsync(userId);
        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(DtoMapper.ToUser(user));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _audit.WriteAsync(AuditActions.Logout, "user", null, "Signed out.");
        return NoContent();
    }
}
