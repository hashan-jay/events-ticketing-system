using EventsLab.Api.Data;
using EventsLab.Api.DTOs;
using EventsLab.Api.Models;
using EventsLab.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventsLab.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/custom-fields")]
public class CustomFieldsController : ControllerBase
{
    private readonly AppDbContext _db;

    public CustomFieldsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomFieldDto>>> GetAll()
    {
        var fields = await _db.CustomFields
            .OrderBy(f => f.DisplayOrder)
            .ThenBy(f => f.Label)
            .ToListAsync();

        return Ok(fields.Select(DtoMapper.ToCustomField));
    }

    [HttpPost]
    public async Task<ActionResult<CustomFieldDto>> Create([FromBody] UpsertCustomFieldRequest request)
    {
        var key = NormalizeKey(request.FieldKey);
        if (await _db.CustomFields.AnyAsync(f => f.FieldKey == key))
        {
            return Conflict(new { message = "A field with this key already exists." });
        }

        var field = new CustomField
        {
            Label = request.Label.Trim(),
            FieldKey = key,
            FieldType = request.FieldType.Trim().ToLowerInvariant(),
            IsRequired = request.IsRequired,
            IsActive = request.IsActive,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTime.UtcNow
        };

        _db.CustomFields.Add(field);
        await _db.SaveChangesAsync();
        return Ok(DtoMapper.ToCustomField(field));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CustomFieldDto>> Update(int id, [FromBody] UpsertCustomFieldRequest request)
    {
        var field = await _db.CustomFields.FindAsync(id);
        if (field is null)
        {
            return NotFound();
        }

        var key = NormalizeKey(request.FieldKey);
        if (await _db.CustomFields.AnyAsync(f => f.FieldKey == key && f.Id != id))
        {
            return Conflict(new { message = "A field with this key already exists." });
        }

        field.Label = request.Label.Trim();
        field.FieldKey = key;
        field.FieldType = request.FieldType.Trim().ToLowerInvariant();
        field.IsRequired = request.IsRequired;
        field.IsActive = request.IsActive;
        field.DisplayOrder = request.DisplayOrder;

        await _db.SaveChangesAsync();
        return Ok(DtoMapper.ToCustomField(field));
    }

    private static string NormalizeKey(string key) =>
        key.Trim().ToLowerInvariant().Replace(' ', '_');
}
