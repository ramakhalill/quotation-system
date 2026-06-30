namespace AuroraManagementAPI.Models
{
    public class SystemType
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        // Optional: navigation to projects
        public ICollection<ProjectSystemType>? ProjectSystemTypes { get; set; }
    }
}
