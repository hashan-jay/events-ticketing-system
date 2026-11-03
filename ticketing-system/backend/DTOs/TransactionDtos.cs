using System.ComponentModel.DataAnnotations;

namespace EventsLab.Api.DTOs;

public class IssueTicketRequest
{
    public int? TicketId { get; set; }

    [Required]
    public int TicketCategoryId { get; set; }

    [Required]
    public int PaymentMethodId { get; set; }

    [MaxLength(160)]
    public string CustomerName { get; set; } = string.Empty;

    [MaxLength(40)]
    public string CustomerPhone { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public List<FieldValueInput> FieldValues { get; set; } = new();
}

public class FieldValueInput
{
    public int CustomFieldId { get; set; }
    public string Value { get; set; } = string.Empty;
}

public class TransactionDetailDto
{
    public int Id { get; set; }
    public DateTime IssuedAt { get; set; }
    public decimal AmountCollected { get; set; }
    public string? Notes { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string TicketNumber { get; set; } = string.Empty;
    public int TicketId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = string.Empty;
    public bool IsVip { get; set; }
    public int PaymentMethodId { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public string PaymentMethodColor { get; set; } = string.Empty;
    public bool CollectsRevenue { get; set; }
    public string IssuedBy { get; set; } = string.Empty;
    public List<FieldValueDto> FieldValues { get; set; } = new();
}

public class FieldValueDto
{
    public int CustomFieldId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string FieldKey { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
