using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AuroraManagementAPI.Models
{
    public class Project
    {
        public int Id { get; set; }
        public int ClientId { get; set; }

        public string Name { get; set; }
        public string SystemTypes { get; set; } = string.Empty;
        [NotMapped]
        public List<string> SystemTypesList
        {
            get => string.IsNullOrEmpty(SystemTypes)
                   ? new List<string>()
                   : SystemTypes.Split(',').Select(s => s.Trim()).ToList();
            set => SystemTypes = string.Join(',', value);
        }
        public ProjectStatus Status { get; set; } = ProjectStatus.Pending;
        public string? CreatedByUserId { get; set; } = null!;
        public User CreatedByUser { get; set; } = null!;

        [JsonIgnore]
        public Client Client { get; set; }
        public ICollection<ProjectSystemType> ProjectSystemTypes { get; set; } = new List<ProjectSystemType>();

        public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
    }
}
