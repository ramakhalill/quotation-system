// Models/Appointment.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuroraManagementAPI.Models
{
    public enum AppointmentStatus
    {
        Pending = 0,
        Confirmed = 1,
        Done = 2,
        Canceled = 3
    }

    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        [Required, MaxLength(150)]
        public string Title { get; set; } = "";

        [MaxLength(500)]
        public string? Description { get; set; }

        // Always store in UTC; convert on the client for local display
        [Required]
        public DateTime StartUtc { get; set; }

        [Required]
        public DateTime EndUtc { get; set; }

        [MaxLength(120)]
        public string? Location { get; set; }

        [MaxLength(120)]
        public string? AssignedTo { get; set; } // technician/engineer

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        public bool AllDay { get; set; } = false;
        public bool? ReminderMinutesBefore { get; set; } // optional reminder later
    }
}
