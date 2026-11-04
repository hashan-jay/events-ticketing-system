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
[Route("api/ticket-categories")]
public class TicketCategoriesController : ControllerBase
{
    private const int MaxRange = 10000;
    private readonly AppDbContext _db;
    private readonly AuditService _audit;

    public TicketCategoriesController(AppDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TicketCategoryDto>>> GetAll()
    {
        var categories = await _db.TicketCategories
            .Include(c => c.Tickets)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(categories.Select(c => DtoMapper.ToCategory(c)));
    }

    [HttpPost]
    public async Task<ActionResult<TicketCategoryDto>> Create([FromBody] UpsertTicketCategoryRequest request)
    {
        if (request.TicketCount < 1)
        {
            return BadRequest(new { message = "Enter how many tickets are available." });
        }

        if (request.StartNumber < 1)
        {
            return BadRequest(new { message = "Starting number must be at least 1." });
        }

        if (request.TicketCount > MaxRange)
        {
            return BadRequest(new { message = $"A section can hold at most {MaxRange} tickets." });
        }

        if (request.StartNumber > long.MaxValue - request.TicketCount + 1)
        {
            return BadRequest(new { message = "That starting number and ticket count do not fit in a ticket range." });
        }

        var category = new TicketCategory { CreatedAt = DateTime.UtcNow };
        ApplyMetadata(category, request);
        category.StartNumber = request.StartNumber;
        category.EndNumber = request.StartNumber + request.TicketCount - 1;

        var created = new List<Ticket>();
        var error = await AddMissingAsync(category, category.StartNumber, category.EndNumber, created);
        if (error is not null)
        {
            return Conflict(new { message = error });
        }

        var pad = TicketNumberFormatter.ResolvePad(category.EndNumber);
        var first = TicketNumberFormatter.Format(category.NumberPrefix, category.StartNumber, pad);
        var last = TicketNumberFormatter.Format(category.NumberPrefix, category.EndNumber, pad);

        _db.TicketCategories.Add(category);
        await using var createdTx = await _db.Database.BeginTransactionAsync();
        try
        {
            await _db.SaveChangesAsync();
            _audit.Stage(
                AuditActions.CategoryCreated,
                "ticket_category",
                category.Id.ToString(),
                $"Created section '{category.Name}' with {created.Count} tickets available ({first} to {last}).",
                $"Price {category.Price:0.##}. VIP {category.IsVip}. Free entry {category.IsFreeEntry}. Active {category.IsActive}.");
            await _db.SaveChangesAsync();
            await createdTx.CommitAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "A ticket number in this range is already in use." });
        }

        return Ok(await LoadCategoryAsync(category.Id));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TicketCategoryDto>> Update(int id, [FromBody] UpsertTicketCategoryRequest request)
    {
        var category = await _db.TicketCategories.FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
        {
            return NotFound();
        }

        if (request.AdditionalTickets < 0)
        {
            return BadRequest(new { message = "Additional tickets cannot be negative." });
        }

        var span = category.EndNumber - category.StartNumber + 1;
        if (span < 1)
        {
            return BadRequest(new { message = "This section has an invalid ticket range." });
        }

        if (request.AdditionalTickets > 0 && span + request.AdditionalTickets > MaxRange)
        {
            return BadRequest(new { message = $"A section can hold at most {MaxRange} tickets. This section already covers {span}." });
        }

        if (request.AdditionalTickets > 0 && category.EndNumber > long.MaxValue - request.AdditionalTickets)
        {
            return BadRequest(new { message = "That many extra tickets do not fit after the current ending number." });
        }

        ApplyMetadata(category, request);

        var created = new List<Ticket>();
        if (request.AdditionalTickets > 0)
        {
            var from = category.EndNumber + 1;
            var to = category.EndNumber + request.AdditionalTickets;
            category.EndNumber = to;
            var error = await AddMissingAsync(category, from, to, created);
            if (error is not null)
            {
                return Conflict(new { message = error });
            }
        }

        var alreadyAvailable = await _db.Tickets.CountAsync(t =>
            t.TicketCategoryId == category.Id && t.Status == TicketStatus.Available);
        var availableAfter = alreadyAvailable + created.Count;
        var summary = created.Count > 0
            ? $"Updated section '{category.Name}' and added {created.Count} tickets. {availableAfter} are now available."
            : $"Updated section '{category.Name}'.";

        _audit.Stage(
            AuditActions.CategoryUpdated,
            "ticket_category",
            category.Id.ToString(),
            summary,
            $"Price {category.Price:0.##}. Active {category.IsActive}. Range {category.StartNumber}-{category.EndNumber}.");

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "A ticket number in this range is already in use." });
        }

        return Ok(await LoadCategoryAsync(category.Id));
    }

    [HttpPost("{id:int}/generate")]
    public async Task<ActionResult<GenerateTicketsResponse>> Generate(int id)
    {
        var category = await _db.TicketCategories.FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
        {
            return NotFound();
        }

        if (category.EndNumber < category.StartNumber)
        {
            return BadRequest(new { message = "Invalid ticket number range." });
        }

        var expected = category.EndNumber - category.StartNumber + 1;
        if (expected > MaxRange)
        {
            return BadRequest(new { message = $"A section can hold at most {MaxRange} tickets." });
        }

        var created = new List<Ticket>();
        var error = await AddMissingAsync(category, category.StartNumber, category.EndNumber, created);
        if (error is not null)
        {
            return Conflict(new { message = error });
        }

        var skipped = (int)expected - created.Count;
        _audit.Stage(
            AuditActions.TicketsGenerated,
            "ticket_category",
            category.Id.ToString(),
            $"Generated {created.Count} missing tickets for '{category.Name}' ({skipped} already existed).");

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "A ticket number in this range is already in use." });
        }

        return Ok(new GenerateTicketsResponse
        {
            Created = created.Count,
            SkippedExisting = skipped,
            TotalInRange = (int)expected
        });
    }

    private async Task<TicketCategoryDto> LoadCategoryAsync(int id)
    {
        var category = await _db.TicketCategories
            .Include(c => c.Tickets)
            .FirstAsync(c => c.Id == id);
        return DtoMapper.ToCategory(category);
    }

    private async Task<string?> AddMissingAsync(TicketCategory category, long from, long to, List<Ticket> created)
    {
        if (to < from)
        {
            return "Ending ticket number must be greater than or equal to the starting number.";
        }

        var count = to - from + 1;
        if (count > MaxRange)
        {
            return $"A section can hold at most {MaxRange} tickets.";
        }

        var pad = TicketNumberFormatter.ResolvePad(to);
        var existingSequences = category.Id == 0
            ? new HashSet<long>()
            : (await _db.Tickets
                .Where(t => t.TicketCategoryId == category.Id)
                .Select(t => t.SequenceNumber)
                .ToListAsync()).ToHashSet();

        var usedNumbers = (await _db.Tickets
                .Select(t => t.TicketNumber)
                .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        for (var n = from; n <= to; n++)
        {
            if (existingSequences.Contains(n))
            {
                continue;
            }

            var number = TicketNumberFormatter.Format(category.NumberPrefix, n, pad);
            if (!usedNumbers.Add(number))
            {
                return $"Ticket number {number} already exists in another section.";
            }

            var ticket = new Ticket
            {
                TicketCategory = category,
                SequenceNumber = n,
                TicketNumber = number,
                Status = TicketStatus.Available,
                CreatedAt = DateTime.UtcNow
            };
            category.Tickets.Add(ticket);
            created.Add(ticket);
        }

        return null;
    }

    private static void ApplyMetadata(TicketCategory category, UpsertTicketCategoryRequest request)
    {
        category.Name = request.Name.Trim();
        category.Description = request.Description?.Trim() ?? string.Empty;
        category.Price = request.IsVip || request.IsFreeEntry ? 0 : request.Price;
        category.IsVip = request.IsVip;
        category.IsFreeEntry = request.IsFreeEntry || request.IsVip;
        category.NumberPrefix = request.NumberPrefix?.Trim() ?? string.Empty;
        category.Color = string.IsNullOrWhiteSpace(request.Color) ? "#6366f1" : request.Color.Trim();
        category.IsActive = request.IsActive;
    }
}
