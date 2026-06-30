using System;
using System.Collections.Generic;

namespace AuroraManagementAPI.Models
{
    public class CreateQuoteDto
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public string ClientMobile { get; set; }
        public string  ClientAddress { get; set; }

        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Notes { get; set; }
        public decimal InstallationFee { get; set; }   // NEW
        public decimal Discount { get; set; }          // NEW (applies only to devices, not installation)

        public List<CreateQuoteItemDto> Items { get; set; }
    }

    public class CreateQuoteItemDto
    {
        public int DeviceId { get; set; }
        public int Quantity { get; set; }
        public decimal ProfitRatio { get; set; } // <-- Added
        public int? PowerConsumption { get; set; }
        public string? Color { get; set; }

    }

    public class QuoteDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Notes { get; set; }
        public decimal TotalQuotePrice { get; set; }
        public decimal TotalPriceWithProfit { get; set; } // sum after profit
        public decimal TotalRevenue { get; set; }       // TotalProfit = TotalPriceWithProfit - TotalQuotePrice
        public decimal InstallationFee { get; set; }   // NEW
        public decimal Discount { get; set; }          // NEW
        public decimal FinalTotal { get; set; }        // NEW = TotalPriceWithProfit - Discount + InstallationFee
        public decimal NetProfit { get; set; }   // only meaningful for manager
        public int TotalPowerConsumption { get; set; }   // NEW

        public QuoteStatus Status { get; set; }

        public List<QuoteItemDto> Items { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public string? ApprovedBy { get; internal set; }
        public int RevisionNumber { get; set; } = 0;
        public int? ParentQuoteId { get; internal set; }
        public string? CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }


    }

    public class QuoteItemDto
    {
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }          // original unit price
        public decimal TotalPrice { get; set; }         // original total
        public decimal ProfitRatio { get; set; }        // percentage, e.g., 10 for 10%
        public decimal PriceAfterProfit { get; set; }   // total price after profit

    }
    public class QuoteItemInputDto
    {
        public int DeviceId { get; set; }
        public int Quantity { get; set; }
        public decimal ProfitRatio { get; set; }
    }
   


}
