using AuroraManagementAPI.Models;

public class QuoteItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    public decimal ProfitRatio { get; set; }       // NEW
    public decimal PriceAfterProfit { get; set; } // NEW

    public int DeviceId { get; set; }
    public Device Device { get; set; }

    public int QuoteId { get; set; }
    public Quote Quote { get; set; }
}

