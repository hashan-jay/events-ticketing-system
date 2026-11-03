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

    public AuthController(AppDbContext db, JwtTokenService tokens)
    {
        _db = db;
        _tokens = tokens;
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
            return Unauthorized(new { message = "Invalid username or password." });
        }

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
}
