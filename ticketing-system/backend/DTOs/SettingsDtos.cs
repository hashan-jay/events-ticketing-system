using System.ComponentModel.DataAnnotations;

namespace EventsLab.Api.DTOs;

public class SettingsDto
{
    public string EventName { get; set; } = string.Empty;
    public string Currency { get; set; } = "LKR";
    public bool FreeEntryEnabled { get; set; }
}

public class UpdateSettingsRequest
{
    [Required, MaxLength(160)]
    public string EventName { get; set; } = string.Empty;

    [Required, MaxLength(10)]
    public string Currency { get; set; } = "LKR";

    public bool FreeEntryEnabled { get; set; }
}
