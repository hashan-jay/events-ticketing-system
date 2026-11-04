namespace EventsLab.Api.Services;

public static class AuditActions
{
    public const string LoginSucceeded = "login.succeeded";
    public const string LoginFailed = "login.failed";
    public const string Logout = "logout";
    public const string TicketIssued = "ticket.issued";
    public const string TicketLookedUp = "ticket.looked_up";
    public const string CategoryCreated = "category.created";
    public const string CategoryUpdated = "category.updated";
    public const string TicketsGenerated = "tickets.generated";
    public const string PaymentMethodCreated = "payment_method.created";
    public const string PaymentMethodUpdated = "payment_method.updated";
    public const string CustomFieldCreated = "custom_field.created";
    public const string CustomFieldUpdated = "custom_field.updated";
    public const string SettingsUpdated = "settings.updated";
}
