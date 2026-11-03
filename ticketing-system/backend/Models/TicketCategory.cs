namespace EventsLab.Api.Models;

public class TicketCategory
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
    public string Color { get; set; } = "#6366f1";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
