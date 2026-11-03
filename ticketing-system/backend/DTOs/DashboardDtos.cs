namespace EventsLab.Api.DTOs;

public class DashboardDto
{
    public string EventName { get; set; } = string.Empty;
    public string Currency { get; set; } = "LKR";
    public bool FreeEntryEnabled { get; set; }
    public int TotalTickets { get; set; }
    public int SoldTickets { get; set; }
    public int RemainingTickets { get; set; }
    public int VoidedTickets { get; set; }
    public int FreePassTickets { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public int TodaySold { get; set; }
    public List<PaymentMethodStatDto> ByPaymentMethod { get; set; } = new();
    public List<CategoryStatDto> ByCategory { get; set; } = new();
    public List<HourlyStatDto> HourlyToday { get; set; } = new();
    public List<TransactionDetailDto> RecentTransactions { get; set; } = new();
}

public class PaymentMethodStatDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public bool CollectsRevenue { get; set; }
    public int TicketCount { get; set; }
    public decimal Revenue { get; set; }
}

public class CategoryStatDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsVip { get; set; }
    public int Total { get; set; }
    public int Sold { get; set; }
    public int Remaining { get; set; }
    public decimal Revenue { get; set; }
}

public class HourlyStatDto
{
    public int Hour { get; set; }
    public int Tickets { get; set; }
    public decimal Revenue { get; set; }
}
