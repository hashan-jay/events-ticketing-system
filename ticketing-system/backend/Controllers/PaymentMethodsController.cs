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
[Route("api/payment-methods")]
public class PaymentMethodsController : ControllerBase
{
    private readonly AppDbContext _db;

    public PaymentMethodsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetAll()
    {
        var methods = await _db.PaymentMethods
            .OrderBy(m => m.DisplayOrder)
            .ThenBy(m => m.Name)
            .ToListAsync();

        return Ok(methods.Select(DtoMapper.ToPaymentMethod));
    }

    [HttpPost]
    public async Task<ActionResult<PaymentMethodDto>> Create([FromBody] UpsertPaymentMethodRequest request)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        if (await _db.PaymentMethods.AnyAsync(m => m.Code == code))
        {
            return Conflict(new { message = "A payment method with this code already exists." });
        }

        var method = new PaymentMethod
        {
            Name = request.Name.Trim(),
            Code = code,
            Description = request.Description.Trim(),
            CollectsRevenue = request.CollectsRevenue,
            IsActive = request.IsActive,
            Color = request.Color,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTime.UtcNow
        };

        _db.PaymentMethods.Add(method);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), DtoMapper.ToPaymentMethod(method));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PaymentMethodDto>> Update(int id, [FromBody] UpsertPaymentMethodRequest request)
    {
        var method = await _db.PaymentMethods.FindAsync(id);
        if (method is null)
        {
            return NotFound();
        }

        var code = request.Code.Trim().ToUpperInvariant();
        if (await _db.PaymentMethods.AnyAsync(m => m.Code == code && m.Id != id))
        {
            return Conflict(new { message = "A payment method with this code already exists." });
        }

        method.Name = request.Name.Trim();
        method.Code = code;
        method.Description = request.Description.Trim();
        method.CollectsRevenue = request.CollectsRevenue;
        method.IsActive = request.IsActive;
        method.Color = request.Color;
        method.DisplayOrder = request.DisplayOrder;

        await _db.SaveChangesAsync();
        return Ok(DtoMapper.ToPaymentMethod(method));
    }
}
