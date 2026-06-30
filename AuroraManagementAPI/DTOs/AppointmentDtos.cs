// Models/Dtos/AppointmentDtos.cs
namespace AuroraManagementAPI.Models
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }

        public string ProjectName { get; set; } = "";
        public string ClientName { get; set; } = "";

        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public string? Location { get; set; }
        public string? AssignedTo { get; set; }
        public AppointmentStatus Status { get; set; }
        public bool AllDay { get; set; }
    }

    public class CreateAppointmentDto
    {
        public int ProjectId { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public string? Location { get; set; }
        public string? AssignedTo { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
        public bool AllDay { get; set; } = false;
    }

    public class UpdateAppointmentDto : CreateAppointmentDto
    {
        public int Id { get; set; }
    }
}
