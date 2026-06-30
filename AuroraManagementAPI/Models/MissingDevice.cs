using System;

namespace AuroraManagementAPI.Models
{
    public class MissingDevice
    {
        public int Id { get; set; }
        public int QuoteId { get; set; }
        public int DeviceId { get; set; }
        public int MissingQuantity { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Quote Quote { get; set; }
        public Device Device { get; set; }
    }
}
