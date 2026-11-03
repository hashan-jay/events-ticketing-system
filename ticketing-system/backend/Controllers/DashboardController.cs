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
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get()
    {
        var settings = await _db.SystemSettings.ToListAsync();
        string Setting(string key, string fallback) =>
            settings.FirstOrDefault(s => s.Key == key)?.Value ?? fallback;

        var tickets = await _db.Tickets.AsNoTracking().ToListAsync();
        var categories = await _db.TicketCategories.AsNoTracking().ToListAsync();
        var methods = await _db.PaymentMethods
            .AsNoTracking()
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync();

        var transactions = await _db.Transactions
            .AsNoTracking()
            .Include(t => t.Ticket).ThenInclude(t => t.TicketCategory)
            .Include(t => t.PaymentMethod)
            .Include(t => t.IssuedByUser)
            .Include(t => t.FieldValues).ThenInclude(v => v.CustomField)
            .ToListAsync();

        var today = DateTime.UtcNow.Date;
        var todayRows = transactions.Where(t => t.IssuedAt.Date == today).ToList();

        var byMethod = methods.Select(method =>
        {
            var rows = transactions.Where(t => t.PaymentMethodId == method.Id).ToList();
            return new PaymentMethodStatDto
            {
                Id = method.Id,
                Name = method.Name,
                Color = method.Color,
                CollectsRevenue = method.CollectsRevenue,
                TicketCount = rows.Count,
                Revenue = rows.Sum(t => t.AmountCollected)
            };
        }).ToList();

        var byCategory = categories.Select(category =>
        {
            var catTickets = tickets.Where(t => t.TicketCategoryId == category.Id).ToList();
            var rows = transactions.Where(t => t.Ticket.TicketCategoryId == category.Id).ToList();
            return new CategoryStatDto
            {
                Id = category.Id,
                Name = category.Name,
                Color = category.Color,
                Price = category.Price,
                IsVip = category.IsVip,
                Total = catTickets.Count,
                Sold = catTickets.Count(t => t.Status == TicketStatus.Issued),
                Remaining = catTickets.Count(t => t.Status == TicketStatus.Available),
                Revenue = rows.Sum(t => t.AmountCollected)
            };
        }).ToList();

        var hourly = Enumerable.Range(0, 24).Select(hour =>
        {
            var rows = todayRows.Where(t => t.IssuedAt.Hour == hour).ToList();
            return new HourlyStatDto
            {
                Hour = hour,
                Tickets = rows.Count,
                Revenue = rows.Sum(t => t.AmountCollected)
            };
        }).ToList();

        return Ok(new DashboardDto
        {
            EventName = Setting("EventName", "EventsLab Live 2026"),
            Currency = Setting("Currency", "LKR"),
            FreeEntryEnabled = !string.Equals(Setting("FreeEntryEnabled", "true"), "false", StringComparison.OrdinalIgnoreCase),
            TotalTickets = tickets.Count,
            SoldTickets = tickets.Count(t => t.Status == TicketStatus.Issued),
            RemainingTickets = tickets.Count(t => t.Status == TicketStatus.Available),
            VoidedTickets = tickets.Count(t => t.Status == TicketStatus.Voided),
            FreePassTickets = transactions.Count(t => t.AmountCollected == 0),
            TotalRevenue = transactions.Sum(t => t.AmountCollected),
            TodayRevenue = todayRows.Sum(t => t.AmountCollected),
            TodaySold = todayRows.Count,
            ByPaymentMethod = byMethod,
            ByCategory = byCategory,
            HourlyToday = hourly,
            RecentTransactions = transactions
                .OrderByDescending(t => t.IssuedAt)
                .Take(8)
                .Select(DtoMapper.ToTransaction)
                .ToList()
        });
    }
}
