namespace EventsLab.Api.Models;

public class AuditLog
{
    public long Id { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public int? UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string? Details { get; set; }
}
