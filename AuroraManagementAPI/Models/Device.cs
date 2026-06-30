using System.ComponentModel.DataAnnotations;

namespace AuroraManagementAPI.Models
{
    public class Device
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public string Type { get; set; }

        
        [MaxLength(50)]
        public string SystemType { get; set; }

        public int StockQuantity { get; set; }
        public string DeviceCode { get; set; } = string.Empty; // 🔹 new field
        public string? SupplierName { get; set; }  // nullable


        [Range(0, double.MaxValue)]
        // Variants
        public int? PowerConsumption { get; set; }      // 4, 6, etc. Nullable
        public string? Color { get; set; }          // White, Black, etc.
        public decimal ActualPrice { get; set; }
        public decimal SalesPrice { get; set; }
        public decimal? ManagerPercent { get; set; } // the percent manager enters

    }
}
