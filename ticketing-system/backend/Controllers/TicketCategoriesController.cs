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

    public TicketCategoriesController(AppDbContext db)
    {
        _db = db;
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
        if (request.EndNumber < request.StartNumber)
        {
            return BadRequest(new { message = "Ending ticket number must be greater than or equal to the starting number." });
        }

        if (request.EndNumber - request.StartNumber + 1 > MaxRange)
        {
            return BadRequest(new { message = $"A category can cover at most {MaxRange} ticket numbers." });
        }

        var category = Apply(new TicketCategory { CreatedAt = DateTime.UtcNow }, request);
        _db.TicketCategories.Add(category);
        await _db.SaveChangesAsync();
        return Ok(DtoMapper.ToCategory(category, Array.Empty<Ticket>()));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TicketCategoryDto>> Update(int id, [FromBody] UpsertTicketCategoryRequest request)
    {
        var category = await _db.TicketCategories.Include(c => c.Tickets).FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
        {
            return NotFound();
        }

        if (request.EndNumber < request.StartNumber)
        {
            return BadRequest(new { message = "Ending ticket number must be greater than or equal to the starting number." });
        }

        Apply(category, request);
        await _db.SaveChangesAsync();
        return Ok(DtoMapper.ToCategory(category));
    }

    [HttpPost("{id:int}/generate")]
    public async Task<ActionResult<GenerateTicketsResponse>> Generate(int id)
    {
        var category = await _db.TicketCategories.FindAsync(id);
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
            return BadRequest(new { message = $"A category can cover at most {MaxRange} ticket numbers." });
        }

        var pad = TicketNumberFormatter.ResolvePad(category.EndNumber);
        var existing = (await _db.Tickets
            .Where(t => t.TicketCategoryId == category.Id)
            .Select(t => t.SequenceNumber)
            .ToListAsync()).ToHashSet();

        var created = new List<Ticket>();
        var skipped = 0;

        for (var n = category.StartNumber; n <= category.EndNumber; n++)
        {
            if (existing.Contains(n))
            {
                skipped++;
                continue;
            }

            var number = TicketNumberFormatter.Format(category.NumberPrefix, n, pad);
            if (await _db.Tickets.AnyAsync(t => t.TicketNumber == number))
            {
                return Conflict(new { message = $"Ticket number {number} already exists in another category." });
            }

            created.Add(new Ticket
            {
                TicketCategoryId = category.Id,
                SequenceNumber = n,
                TicketNumber = number,
                Status = TicketStatus.Available,
                CreatedAt = DateTime.UtcNow
            });
        }

        _db.Tickets.AddRange(created);
        await _db.SaveChangesAsync();

        return Ok(new GenerateTicketsResponse
        {
            Created = created.Count,
            SkippedExisting = skipped,
            TotalInRange = (int)expected
        });
    }

    private static TicketCategory Apply(TicketCategory category, UpsertTicketCategoryRequest request)
    {
        category.Name = request.Name.Trim();
        category.Description = request.Description.Trim();
        category.Price = request.IsVip || request.IsFreeEntry ? 0 : request.Price;
        category.IsVip = request.IsVip;
        category.IsFreeEntry = request.IsFreeEntry || request.IsVip;
        category.NumberPrefix = request.NumberPrefix.Trim();
        category.StartNumber = request.StartNumber;
        category.EndNumber = request.EndNumber;
        category.Color = request.Color;
        category.IsActive = request.IsActive;
        return category;
    }
}
