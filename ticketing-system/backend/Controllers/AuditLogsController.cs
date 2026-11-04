using EventsLab.Api.Data;
using EventsLab.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventsLab.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/audit-logs")]
public class AuditLogsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuditLogsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? action,
        [FromQuery] int take = 200)
    {
        take = Math.Clamp(take, 1, 500);

        var query = _db.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(action))
        {
            var selected = action.Trim();
            query = query.Where(a => a.Action == selected);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(a =>
                a.Summary.Contains(term) ||
                a.Username.Contains(term) ||
                a.Action.Contains(term) ||
                (a.Details != null && a.Details.Contains(term)) ||
                (a.EntityId != null && a.EntityId.Contains(term)));
        }

        var rows = await query
            .OrderByDescending(a => a.OccurredAt)
            .ThenByDescending(a => a.Id)
            .Take(take)
            .ToListAsync();

        return Ok(rows.Select(a => new AuditLogDto
        {
            Id = a.Id,
            OccurredAt = a.OccurredAt,
            UserId = a.UserId,
            Username = a.Username,
            Action = a.Action,
            EntityType = a.EntityType,
            EntityId = a.EntityId,
            Summary = a.Summary,
            Details = a.Details
        }));
    }
}
