namespace AuroraManagementAPI.Models
{
    public class CreateProjectDto
    {
        public string Name { get; set; }
        public List<string> SystemTypes { get; set; } = new List<string>(); // ✅ now list

        public int ClientId { get; set; }  // Leave 0 to create new client
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public string ClientMobile { get; set; }
        public string ClientAddress { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.Pending; // 👈 default to Pending


    }

    public class ProjectDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string Name { get; set; }
        public List<string> SystemTypes { get; set; } = new List<string>(); // ✅ now list
        public string ClientName { get; set; } // <-- new
        public string ClientEmail { get; set; }
        public string ClientMobile { get; set; }
        public string ClientAddress { get; set; }
        public ProjectStatus Status { get; set; } // ✅ enum type


    }
}
