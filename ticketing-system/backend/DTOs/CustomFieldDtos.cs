using System.ComponentModel.DataAnnotations;

namespace EventsLab.Api.DTOs;

public class CustomFieldDto
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string FieldKey { get; set; } = string.Empty;
    public string FieldType { get; set; } = "text";
    public bool IsRequired { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}

public class UpsertCustomFieldRequest
{
    [Required, MaxLength(120)]
    public string Label { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string FieldKey { get; set; } = string.Empty;

    [Required, MaxLength(40)]
    public string FieldType { get; set; } = "text";

    public bool IsRequired { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}
