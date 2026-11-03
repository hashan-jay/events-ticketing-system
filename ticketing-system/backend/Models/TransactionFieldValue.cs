namespace EventsLab.Api.Models;

public class TransactionFieldValue
{
    public int Id { get; set; }
    public int TransactionId { get; set; }
    public int CustomFieldId { get; set; }
    public string Value { get; set; } = string.Empty;

    public Transaction Transaction { get; set; } = null!;
    public CustomField CustomField { get; set; } = null!;
}
