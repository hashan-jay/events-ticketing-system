namespace EventsLab.Api.Models;

public class Transaction
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int PaymentMethodId { get; set; }
    public int IssuedByUserId { get; set; }
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public decimal AmountCollected { get; set; }
    public string? Notes { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;

    public Ticket Ticket { get; set; } = null!;
    public PaymentMethod PaymentMethod { get; set; } = null!;
    public User IssuedByUser { get; set; } = null!;
    public ICollection<TransactionFieldValue> FieldValues { get; set; } = new List<TransactionFieldValue>();
}
