using EventsLab.Api.DTOs;
using EventsLab.Api.Models;

namespace EventsLab.Api.Services;

public static class DtoMapper
{
    public static UserDto ToUser(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        FullName = user.FullName,
        Role = user.Role
    };

    public static PaymentMethodDto ToPaymentMethod(PaymentMethod method) => new()
    {
        Id = method.Id,
        Name = method.Name,
        Code = method.Code,
        Description = method.Description,
        CollectsRevenue = method.CollectsRevenue,
        IsActive = method.IsActive,
        Color = method.Color,
        DisplayOrder = method.DisplayOrder
    };

    public static CustomFieldDto ToCustomField(CustomField field) => new()
    {
        Id = field.Id,
        Label = field.Label,
        FieldKey = field.FieldKey,
        FieldType = field.FieldType,
        IsRequired = field.IsRequired,
        IsActive = field.IsActive,
        DisplayOrder = field.DisplayOrder
    };

    public static TicketCategoryDto ToCategory(TicketCategory category, IEnumerable<Ticket>? tickets = null)
    {
        var list = tickets?.ToList() ?? category.Tickets?.ToList() ?? new List<Ticket>();
        return new TicketCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Price = category.Price,
            IsVip = category.IsVip,
            IsFreeEntry = category.IsFreeEntry,
            NumberPrefix = category.NumberPrefix,
            StartNumber = category.StartNumber,
            EndNumber = category.EndNumber,
            Color = category.Color,
            IsActive = category.IsActive,
            TotalTickets = list.Count,
            SoldTickets = list.Count(t => t.Status == TicketStatus.Issued),
            RemainingTickets = list.Count(t => t.Status == TicketStatus.Available)
        };
    }

    public static TransactionDetailDto ToTransaction(Transaction tx) => new()
    {
        Id = tx.Id,
        IssuedAt = tx.IssuedAt,
        AmountCollected = tx.AmountCollected,
        Notes = tx.Notes,
        CustomerName = tx.CustomerName,
        CustomerPhone = tx.CustomerPhone,
        TicketNumber = tx.Ticket?.TicketNumber ?? string.Empty,
        TicketId = tx.TicketId,
        CategoryName = tx.Ticket?.TicketCategory?.Name ?? string.Empty,
        CategoryColor = tx.Ticket?.TicketCategory?.Color ?? "#6366f1",
        IsVip = tx.Ticket?.TicketCategory?.IsVip ?? false,
        PaymentMethodId = tx.PaymentMethodId,
        PaymentMethodName = tx.PaymentMethod?.Name ?? string.Empty,
        PaymentMethodColor = tx.PaymentMethod?.Color ?? "#22d3ee",
        CollectsRevenue = tx.PaymentMethod?.CollectsRevenue ?? false,
        IssuedBy = tx.IssuedByUser?.FullName ?? string.Empty,
        FieldValues = tx.FieldValues.Select(v => new FieldValueDto
        {
            CustomFieldId = v.CustomFieldId,
            Label = v.CustomField?.Label ?? string.Empty,
            FieldKey = v.CustomField?.FieldKey ?? string.Empty,
            Value = v.Value
        }).ToList()
    };
}
