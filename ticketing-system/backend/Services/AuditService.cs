using System.Security.Claims;
using EventsLab.Api.Data;
using EventsLab.Api.Models;

namespace EventsLab.Api.Services;

public class AuditService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public AuditService(AppDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public void Stage(
        string action,
        string entityType,
        string? entityId,
        string summary,
        string? details = null,
        int? userId = null,
        string? username = null)
    {
        var principal = _http.HttpContext?.User;
        if (userId is null && principal?.Identity?.IsAuthenticated == true)
        {
            var idValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(idValue, out var parsed))
            {
                userId = parsed;
            }
        }

        if (string.IsNullOrWhiteSpace(username) && principal?.Identity?.IsAuthenticated == true)
        {
            username = principal.FindFirstValue(ClaimTypes.Name);
        }

        _db.AuditLogs.Add(new AuditLog
        {
            OccurredAt = DateTime.UtcNow,
            UserId = userId,
            Username = Clip(string.IsNullOrWhiteSpace(username) ? "anonymous" : username.Trim(), 80),
            Action = Clip(action, 80),
            EntityType = Clip(entityType, 80),
            EntityId = string.IsNullOrWhiteSpace(entityId) ? null : Clip(entityId.Trim(), 40),
            Summary = Clip(summary.Trim(), 400),
            Details = string.IsNullOrWhiteSpace(details) ? null : Clip(details.Trim(), 2000)
        });
    }

    public async Task WriteAsync(
        string action,
        string entityType,
        string? entityId,
        string summary,
        string? details = null,
        int? userId = null,
        string? username = null)
    {
        Stage(action, entityType, entityId, summary, details, userId, username);
        await _db.SaveChangesAsync();
    }

    private static string Clip(string value, int max) =>
        value.Length <= max ? value : value[..max];
}
