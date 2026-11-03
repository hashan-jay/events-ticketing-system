using System.ComponentModel.DataAnnotations;
using EventsLab.Api.Models;

namespace EventsLab.Api.DTOs;

public class TicketCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsVip { get; set; }
    public bool IsFreeEntry { get; set; }
    public string NumberPrefix { get; set; } = string.Empty;
    public long StartNumber { get; set; }
    public long EndNumber { get; set; }
    public string Color { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int TotalTickets { get; set; }
    public int SoldTickets { get; set; }
    public int RemainingTickets { get; set; }
}

public class UpsertTicketCategoryRequest
{
    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(400)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 10000000)]
    public decimal Price { get; set; }

    public bool IsVip { get; set; }
    public bool IsFreeEntry { get; set; }

    [MaxLength(20)]
    public string NumberPrefix { get; set; } = string.Empty;

    [Range(0, long.MaxValue)]
    public long StartNumber { get; set; }

    [Range(0, long.MaxValue)]
    public long EndNumber { get; set; }

    [MaxLength(20)]
    public string Color { get; set; } = "#6366f1";

    public bool IsActive { get; set; } = true;
}

public class GenerateTicketsResponse
{
    public int Created { get; set; }
    public int SkippedExisting { get; set; }
    public int TotalInRange { get; set; }
}

public class TicketListItemDto
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public long SequenceNumber { get; set; }
    public TicketStatus Status { get; set; }
    public int TicketCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsVip { get; set; }
}

public class TicketAuditDto
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public long SequenceNumber { get; set; }
    public TicketStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public TicketCategoryDto Category { get; set; } = null!;
    public TransactionDetailDto? Transaction { get; set; }
}
