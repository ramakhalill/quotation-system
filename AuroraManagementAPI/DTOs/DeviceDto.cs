namespace AuroraManagementAPI.Controllers
{
    public class DeviceDto
    {
        public int Id { get; set; }
        public string DeviceCode { get; set; } = string.Empty;

        public string Name { get; set; }
        public string Type { get; set; }
        public string SystemType { get; set; }
        public int StockQuantity { get; set; }
        //public decimal Price { get; set; }
        public int? PowerConsumption { get; set; }
        public string Color { get; set; }
        public decimal? ActualPrice { get; set; }   // only for manager
        public decimal SalesPrice { get; set; }     // for everyone
        public decimal? ManagerPercent { get; set; } // the percent manager enters
        public string? SupplierName { get; set; }  // nullable

    }


}