namespace AuroraManagementAPI.Models
{
    public class ProjectSystemType
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public int SystemTypeId { get; set; }
        public SystemType SystemType { get; set; } = null!;
    }
}
