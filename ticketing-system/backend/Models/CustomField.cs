namespace EventsLab.Api.Models;

public class CustomField
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string FieldKey { get; set; } = string.Empty;
    public string FieldType { get; set; } = "text";
    public bool IsRequired { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TransactionFieldValue> FieldValues { get; set; } = new List<TransactionFieldValue>();
}
