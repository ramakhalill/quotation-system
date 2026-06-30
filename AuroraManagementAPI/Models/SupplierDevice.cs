namespace AuroraManagementAPI.Models
{
    public class SupplierDevice
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal SalesPrice { get; set; }   // user adds
        public decimal? ProfitRatio { get; set; } // manager adds
        public decimal? ActualPrice { get; set; } // calculated automatically

        public Device Device { get; set; }
    }
}
