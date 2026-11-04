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
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;

    public TicketsController(AppDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TicketListItemDto>>> GetAll(
        [FromQuery] int? categoryId,
        [FromQuery] TicketStatus? status,
        [FromQuery] string? search,
        [FromQuery] int take = 200)
    {
        take = Math.Clamp(take, 1, 500);

        var query = _db.Tickets.Include(t => t.TicketCategory).AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(t => t.TicketCategoryId == categoryId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(t => t.TicketNumber.Contains(term));
        }

        var tickets = await query
            .OrderBy(t => t.TicketCategory.Name)
            .ThenBy(t => t.SequenceNumber)
            .Take(take)
            .ToListAsync();

        return Ok(tickets.Select(t => new TicketListItemDto
        {
            Id = t.Id,
            TicketNumber = t.TicketNumber,
            SequenceNumber = t.SequenceNumber,
            Status = t.Status,
            TicketCategoryId = t.TicketCategoryId,
            CategoryName = t.TicketCategory.Name,
            Price = t.TicketCategory.Price,
            IsVip = t.TicketCategory.IsVip
        }));
    }

    [HttpGet("next")]
    public async Task<ActionResult<TicketListItemDto>> NextAvailable([FromQuery] int categoryId)
    {
        var ticket = await _db.Tickets
            .Include(t => t.TicketCategory)
            .Where(t => t.TicketCategoryId == categoryId && t.Status == TicketStatus.Available)
            .OrderBy(t => t.SequenceNumber)
            .FirstOrDefaultAsync();

        if (ticket is null)
        {
            return NotFound(new { message = "No available tickets remain in this category." });
        }

        return Ok(new TicketListItemDto
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            SequenceNumber = ticket.SequenceNumber,
            Status = ticket.Status,
            TicketCategoryId = ticket.TicketCategoryId,
            CategoryName = ticket.TicketCategory.Name,
            Price = ticket.TicketCategory.Price,
            IsVip = ticket.TicketCategory.IsVip
        });
    }

    [HttpGet("{ticketNumber}")]
    public async Task<ActionResult<TicketAuditDto>> Lookup(string ticketNumber)
    {
        var ticket = await _db.Tickets
            .Include(t => t.TicketCategory)
            .Include(t => t.Transaction)
                .ThenInclude(tx => tx!.PaymentMethod)
            .Include(t => t.Transaction)
                .ThenInclude(tx => tx!.IssuedByUser)
            .Include(t => t.Transaction)
                .ThenInclude(tx => tx!.FieldValues)
                    .ThenInclude(v => v.CustomField)
            .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber);

        if (ticket is null)
        {
            await _audit.WriteAsync(
                AuditActions.TicketLookedUp,
                "ticket",
                ticketNumber,
                $"Looked up ticket {ticketNumber}, which does not exist.");
            return NotFound(new { message = "Ticket not found." });
        }

        await _audit.WriteAsync(
            AuditActions.TicketLookedUp,
            "ticket",
            ticket.TicketNumber,
            $"Looked up ticket {ticket.TicketNumber} ({ticket.Status}).");

        return Ok(new TicketAuditDto
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            SequenceNumber = ticket.SequenceNumber,
            Status = ticket.Status,
            CreatedAt = ticket.CreatedAt,
            Category = DtoMapper.ToCategory(ticket.TicketCategory, new[] { ticket }),
            Transaction = ticket.Transaction is null ? null : DtoMapper.ToTransaction(ticket.Transaction)
        });
    }
}
