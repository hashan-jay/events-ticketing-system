using EventsLab.Api.Models;
using EventsLab.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace EventsLab.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.Users.AnyAsync())
        {
            db.Users.Add(new User
            {
                Username = "admin",
                PasswordHash = PasswordHasher.Hash("Admin@123"),
                FullName = "EventsLab Admin",
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (!await db.PaymentMethods.AnyAsync())
        {
            db.PaymentMethods.AddRange(
                new PaymentMethod
                {
                    Name = "At Gate CASH Payment",
                    Code = "GATE_CASH",
                    Description = "Cash collected at the venue gate.",
                    CollectsRevenue = true,
                    Color = "#22c55e",
                    DisplayOrder = 1
                },
                new PaymentMethod
                {
                    Name = "Pre-reserved ONLINE Payment",
                    Code = "ONLINE_PRE",
                    Description = "Tickets reserved and paid online before the event.",
                    CollectsRevenue = true,
                    Color = "#38bdf8",
                    DisplayOrder = 2
                },
                new PaymentMethod
                {
                    Name = "VIP - FREE PASS",
                    Code = "VIP_FREE",
                    Description = "Complimentary VIP / free-pass tickets. No revenue collected.",
                    CollectsRevenue = false,
                    Color = "#a855f7",
                    DisplayOrder = 3
                },
                new PaymentMethod
                {
                    Name = "At Gate ONLINE TRANSFER",
                    Code = "GATE_TRANSFER",
                    Description = "Bank / wallet transfer completed at the gate.",
                    CollectsRevenue = true,
                    Color = "#f59e0b",
                    DisplayOrder = 4
                },
                new PaymentMethod
                {
                    Name = "CARD MACHINE Payment",
                    Code = "CARD_MACHINE",
                    Description = "Card terminal payment at the venue.",
                    CollectsRevenue = true,
                    Color = "#f43f5e",
                    DisplayOrder = 5
                }
            );
        }

        if (!await db.CustomFields.AnyAsync())
        {
            db.CustomFields.AddRange(
                new CustomField { Label = "Customer Name", FieldKey = "customer_name", FieldType = "text", IsRequired = true, DisplayOrder = 1 },
                new CustomField { Label = "Customer Phone Number", FieldKey = "customer_phone", FieldType = "phone", IsRequired = true, DisplayOrder = 2 },
                new CustomField { Label = "Email", FieldKey = "email", FieldType = "email", IsRequired = false, DisplayOrder = 3 },
                new CustomField { Label = "NIC / ID Number", FieldKey = "nic", FieldType = "text", IsRequired = false, DisplayOrder = 4 }
            );
        }

        if (!await db.SystemSettings.AnyAsync())
        {
            db.SystemSettings.AddRange(
                new SystemSetting { Key = "EventName", Value = "EventsLab Live 2026" },
                new SystemSetting { Key = "Currency", Value = "LKR" },
                new SystemSetting { Key = "FreeEntryEnabled", Value = "true" }
            );
        }

        await db.SaveChangesAsync();

        if (!await db.TicketCategories.AnyAsync())
        {
            var general = new TicketCategory
            {
                Name = "General Admission",
                Description = "Standard entry for the main floor.",
                Price = 2500,
                NumberPrefix = "GEN-",
                StartNumber = 1001,
                EndNumber = 1100,
                Color = "#22d3ee",
                IsActive = true
            };
            var balcony = new TicketCategory
            {
                Name = "Balcony",
                Description = "Elevated seating section.",
                Price = 1500,
                NumberPrefix = "BAL-",
                StartNumber = 2001,
                EndNumber = 2080,
                Color = "#f59e0b",
                IsActive = true
            };
            var vip = new TicketCategory
            {
                Name = "VIP Lounge",
                Description = "Complimentary VIP access. No revenue.",
                Price = 0,
                IsVip = true,
                IsFreeEntry = true,
                NumberPrefix = "VIP-",
                StartNumber = 1,
                EndNumber = 40,
                Color = "#a855f7",
                IsActive = true
            };

            db.TicketCategories.AddRange(general, balcony, vip);
            await db.SaveChangesAsync();

            GenerateTickets(db, general);
            GenerateTickets(db, balcony);
            GenerateTickets(db, vip);
            await db.SaveChangesAsync();
        }
    }

    private static void GenerateTickets(AppDbContext db, TicketCategory category)
    {
        var tickets = new List<Ticket>();
        var pad = Math.Max(4, category.EndNumber.ToString().Length);
        for (var n = category.StartNumber; n <= category.EndNumber; n++)
        {
            tickets.Add(new Ticket
            {
                TicketCategoryId = category.Id,
                SequenceNumber = n,
                TicketNumber = TicketNumberFormatter.Format(category.NumberPrefix, n, pad),
                Status = TicketStatus.Available,
                CreatedAt = DateTime.UtcNow
            });
        }

        db.Tickets.AddRange(tickets);
    }
}
