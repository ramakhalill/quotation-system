using System;
using System.Collections.Generic;

namespace AuroraManagementAPI.Models
{

    public enum QuoteStatus
    {
        Pending,
        Approved,
        Accepted,
        Declined,
        Revision   // 
    }
    public class Quote
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int ProjectId { get; set; }
        public int? ParentQuoteId { get; set; }
        public Quote ParentQuote { get; set; }       // Navigation property

        public int RevisionNumber { get; set; } = 0;

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public string Notes { get; set; }
        public decimal InstallationFee { get; set; }   // NEW
        public decimal Discount { get; set; }          // NEW
        public QuoteStatus Status { get; set; } = QuoteStatus.Pending; // enum: Pending, Approved, Declined
        public string? CreatedByUserId { get; set; }  // Salesperson
        public string? CreatedByUserName { get; set; }   // <--- REQUIRED

        public string? ApprovedByUserId { get; set; } // Manager

        public decimal FinalTotal { get; set; }   // ← ADD THIS
        public decimal NetProfit { get; set; }    // ← ADD THIS
        public int TotalPowerConsumption { get; set; }   // NEW

        public Client Client { get; set; }
        public Project Project { get; set; }

        public List<QuoteItem> Items { get; set; } = new List<QuoteItem>();
        public decimal TotalRevenue { get; internal set; }
    }

}
