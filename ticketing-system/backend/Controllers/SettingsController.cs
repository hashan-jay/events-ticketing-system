using EventsLab.Api.Data;
using EventsLab.Api.DTOs;
using EventsLab.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventsLab.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/settings")]
public class SettingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SettingsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<SettingsDto>> Get()
    {
        return Ok(await ReadSettingsAsync());
    }

    [HttpPut]
    public async Task<ActionResult<SettingsDto>> Update([FromBody] UpdateSettingsRequest request)
    {
        await UpsertAsync("EventName", request.EventName.Trim());
        await UpsertAsync("Currency", request.Currency.Trim().ToUpperInvariant());
        await UpsertAsync("FreeEntryEnabled", request.FreeEntryEnabled ? "true" : "false");
        await _db.SaveChangesAsync();
        return Ok(await ReadSettingsAsync());
    }

    private async Task<SettingsDto> ReadSettingsAsync()
    {
        var rows = await _db.SystemSettings.ToListAsync();
        string Get(string key, string fallback) =>
            rows.FirstOrDefault(s => s.Key == key)?.Value ?? fallback;

        return new SettingsDto
        {
            EventName = Get("EventName", "EventsLab Live 2026"),
            Currency = Get("Currency", "LKR"),
            FreeEntryEnabled = string.Equals(Get("FreeEntryEnabled", "true"), "true", StringComparison.OrdinalIgnoreCase)
        };
    }

    private async Task UpsertAsync(string key, string value)
    {
        var row = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (row is null)
        {
            _db.SystemSettings.Add(new SystemSetting { Key = key, Value = value });
        }
        else
        {
            row.Value = value;
        }
    }
}
