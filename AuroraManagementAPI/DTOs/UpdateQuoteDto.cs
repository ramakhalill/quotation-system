namespace AuroraManagementAPI.DTOs
{
    public class UpdateQuoteDto
    {
        public int Id { get; set; }
        public decimal? Discount { get; set; }
        public decimal? InstallationFee { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; } // optional ("Pending", "Approved", "Declined", etc.)
        public List<UpdateQuoteItemDto> ?Items { get; set; }
    }

    public class UpdateQuoteItemDto
    {
        public int DeviceId { get; set; }
        public int Quantity { get; set; }
        public decimal ProfitRatio { get; set; }
        public string? Color { get; set; }
        public int? ButtonCount { get; set; }
    }
}
