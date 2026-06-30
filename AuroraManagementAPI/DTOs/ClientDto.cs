namespace AuroraManagementAPI.DTOs
{
    public class ClientDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Mobile { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Address { get; set; } = null!;

        // New field
        public string CreatedByUserId { get; set; }   // 👈 add this

        public string CreatedByUsername { get; set; } = "N/A";
        public DateTime CreatedAt { get; set; }


    }
}
