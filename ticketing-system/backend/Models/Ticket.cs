namespace EventsLab.Api.Models;

public class Ticket
{
    public int Id { get; set; }
    public int TicketCategoryId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public long SequenceNumber { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Available;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public TicketCategory TicketCategory { get; set; } = null!;
    public Transaction? Transaction { get; set; }
}
