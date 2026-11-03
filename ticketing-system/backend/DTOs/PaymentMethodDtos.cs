using System.ComponentModel.DataAnnotations;

namespace EventsLab.Api.DTOs;

public class PaymentMethodDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool CollectsRevenue { get; set; }
    public bool IsActive { get; set; }
    public string Color { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class UpsertPaymentMethodRequest
{
    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(40)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(400)]
    public string Description { get; set; } = string.Empty;

    public bool CollectsRevenue { get; set; } = true;
    public bool IsActive { get; set; } = true;

    [MaxLength(20)]
    public string Color { get; set; } = "#22d3ee";

    public int DisplayOrder { get; set; }
}
