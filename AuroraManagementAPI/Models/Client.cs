using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuroraManagementAPI.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(20), Phone]
        public string Mobile { get; set; }

        [Required, MaxLength(100), EmailAddress]
        public string Email { get; set; }

        [MaxLength(200)]
        public string Address { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public ICollection<Project>? Projects { get; set; } = new List<Project>();
        public ICollection<Quote>? Quotes { get; set; } = new List<Quote>();

        public string? CreatedByUserId { get; set; }
        public IdentityUser? CreatedByUser { get; set; }
    }
}
