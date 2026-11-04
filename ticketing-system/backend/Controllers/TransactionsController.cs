using System.Security.Claims;
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
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;

    public TransactionsController(AppDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransactionDetailDto>>> GetAll(
        [FromQuery] int? paymentMethodId,
        [FromQuery] int? categoryId,
        [FromQuery] string? search,
        [FromQuery] int take = 150)
    {
        take = Math.Clamp(take, 1, 400);

        var query = _db.Transactions
            .Include(t => t.Ticket).ThenInclude(t => t.TicketCategory)
            .Include(t => t.PaymentMethod)
            .Include(t => t.IssuedByUser)
            .Include(t => t.FieldValues).ThenInclude(v => v.CustomField)
            .AsQueryable();

        if (paymentMethodId.HasValue)
        {
            query = query.Where(t => t.PaymentMethodId == paymentMethodId.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(t => t.Ticket.TicketCategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(t =>
                t.Ticket.TicketNumber.Contains(term) ||
                t.CustomerName.Contains(term) ||
                t.CustomerPhone.Contains(term));
        }

        var rows = await query
            .OrderByDescending(t => t.IssuedAt)
            .Take(take)
            .ToListAsync();

        return Ok(rows.Select(DtoMapper.ToTransaction));
    }

    [HttpPost]
    public async Task<ActionResult<TransactionDetailDto>> Issue([FromBody] IssueTicketRequest request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var category = await _db.TicketCategories.FirstOrDefaultAsync(c => c.Id == request.TicketCategoryId);
        if (category is null || !category.IsActive)
        {
            return BadRequest(new { message = "Ticket category is not available." });
        }

        var method = await _db.PaymentMethods.FirstOrDefaultAsync(m => m.Id == request.PaymentMethodId);
        if (method is null || !method.IsActive)
        {
            return BadRequest(new { message = "Payment method is not available." });
        }

        var freeEntryEnabled = await IsFreeEntryEnabledAsync();
        var isComplimentary = category.IsVip || category.IsFreeEntry || !method.CollectsRevenue;
        if (isComplimentary && !freeEntryEnabled)
        {
            return BadRequest(new { message = "Free entry / VIP tickets are currently disabled in Settings." });
        }

        var fields = await _db.CustomFields.Where(f => f.IsActive).ToListAsync();
        foreach (var field in fields.Where(f => f.IsRequired))
        {
            var submitted = request.FieldValues.FirstOrDefault(v => v.CustomFieldId == field.Id)?.Value
                ?? (field.FieldKey is "customer_name" ? request.CustomerName
                    : field.FieldKey is "customer_phone" ? request.CustomerPhone
                    : string.Empty);

            if (string.IsNullOrWhiteSpace(submitted))
            {
                return BadRequest(new { message = $"{field.Label} is required." });
            }
        }

        await using var dbTx = await _db.Database.BeginTransactionAsync();

        Ticket? ticket;
        if (request.TicketId.HasValue)
        {
            ticket = await _db.Tickets.FirstOrDefaultAsync(t =>
                t.Id == request.TicketId.Value && t.TicketCategoryId == request.TicketCategoryId);
        }
        else
        {
            ticket = await _db.Tickets
                .Where(t => t.TicketCategoryId == request.TicketCategoryId && t.Status == TicketStatus.Available)
                .OrderBy(t => t.SequenceNumber)
                .FirstOrDefaultAsync();
        }

        if (ticket is null)
        {
            return NotFound(new { message = "The selected ticket was not found." });
        }

        if (ticket.Status != TicketStatus.Available)
        {
            return Conflict(new { message = $"Ticket {ticket.TicketNumber} has already been issued." });
        }

        var customerName = FirstNonEmpty(
            request.CustomerName,
            ValueOf(request, fields, "customer_name"));
        var customerPhone = FirstNonEmpty(
            request.CustomerPhone,
            ValueOf(request, fields, "customer_phone"));

        ticket.Status = TicketStatus.Issued;
        var transaction = new Transaction
        {
            TicketId = ticket.Id,
            PaymentMethodId = method.Id,
            IssuedByUserId = userId,
            IssuedAt = DateTime.UtcNow,
            AmountCollected = isComplimentary ? 0 : category.Price,
            Notes = request.Notes?.Trim(),
            CustomerName = customerName,
            CustomerPhone = customerPhone
        };

        foreach (var field in fields)
        {
            var value = request.FieldValues.FirstOrDefault(v => v.CustomFieldId == field.Id)?.Value;
            if (string.IsNullOrWhiteSpace(value) && field.FieldKey == "customer_name")
            {
                value = customerName;
            }

            if (string.IsNullOrWhiteSpace(value) && field.FieldKey == "customer_phone")
            {
                value = customerPhone;
            }

            if (!string.IsNullOrWhiteSpace(value))
            {
                transaction.FieldValues.Add(new TransactionFieldValue
                {
                    CustomFieldId = field.Id,
                    Value = value.Trim()
                });
            }
        }

        _db.Transactions.Add(transaction);
        _audit.Stage(
            AuditActions.TicketIssued,
            "ticket",
            ticket.TicketNumber,
            $"Issued {ticket.TicketNumber} in {category.Name} to {(string.IsNullOrWhiteSpace(customerName) ? "a guest" : customerName)}. Collected {transaction.AmountCollected:0.##} via {method.Name}.",
            string.IsNullOrWhiteSpace(customerPhone) ? request.Notes : $"Phone {customerPhone}. {request.Notes}".Trim());
        await _db.SaveChangesAsync();
        await dbTx.CommitAsync();

        var saved = await _db.Transactions
            .Include(t => t.Ticket).ThenInclude(t => t.TicketCategory)
            .Include(t => t.PaymentMethod)
            .Include(t => t.IssuedByUser)
            .Include(t => t.FieldValues).ThenInclude(v => v.CustomField)
            .FirstAsync(t => t.Id == transaction.Id);

        return Ok(DtoMapper.ToTransaction(saved));
    }

    private async Task<bool> IsFreeEntryEnabledAsync()
    {
        var setting = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Key == "FreeEntryEnabled");
        return setting is null || !string.Equals(setting.Value, "false", StringComparison.OrdinalIgnoreCase);
    }

    private static string ValueOf(IssueTicketRequest request, List<CustomField> fields, string key)
    {
        var field = fields.FirstOrDefault(f => f.FieldKey == key);
        if (field is null)
        {
            return string.Empty;
        }

        return request.FieldValues.FirstOrDefault(v => v.CustomFieldId == field.Id)?.Value ?? string.Empty;
    }

    private static string FirstNonEmpty(params string[] values) =>
        values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim() ?? string.Empty;
}
