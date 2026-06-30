using AuroraManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AuroraManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly AuroraDbContext _context;

        public ProjectsController(AuroraDbContext context)
        {
            _context = context;
        }

        // GET: api/Projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects()
        {
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager");
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var projects = await _context.Projects
                .Include(p => p.Client)
                .Where(p => isAdmin || p.Client.CreatedByUserId == userId)
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    SystemTypes = p.SystemTypesList, // use the List<string> property
                    ClientName = p.Client != null ? p.Client.Name : "N/A",
                    ClientEmail = p.Client != null ? p.Client.Email : "N/A",
                    ClientMobile = p.Client != null ? p.Client.Mobile : "N/A",
                    ClientAddress = p.Client != null ? p.Client.Address : "N/A",
                    Status = p.Status
                })
                .ToListAsync();

            return Ok(projects);
        }

        // GET: api/Projects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDto>> GetProject(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Client)
                .Where(p => p.Id == id)
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    SystemTypes = p.SystemTypesList, // use the List<string> property
                    ClientEmail = p.Client != null ? p.Client.Email : "N/A",
                    ClientMobile = p.Client != null ? p.Client.Mobile : "N/A",
                    ClientAddress = p.Client != null ? p.Client.Address : "N/A",
                    ClientId = p.ClientId,
                    Status = p.Status
                })
                .FirstOrDefaultAsync();

            if (project == null) return NotFound();

            var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager");
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (!isAdmin && project.ClientId != 0)
            {
                var client = await _context.Clients.FindAsync(project.ClientId);
                if (client?.CreatedByUserId != userId)
                    return Forbid();
            }

            return Ok(project);
        }

        // PUT: api/Projects/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProjectStatus(int id, ProjectStatus newStatus)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            var isAdmin = User.IsInRole("Admin");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!isAdmin && project.CreatedByUserId != userId)
                return Forbid();

            project.Status = newStatus;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Projects
        [HttpPost]
        public async Task<ActionResult<ProjectDto>> CreateProject(CreateProjectDto dto, ProjectStatus status = ProjectStatus.Pending)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Client client;

            // Validate system types from the database
            var allowedSystemTypes = await _context.SystemTypes
                .Select(st => st.Name)
                .ToListAsync();

            if (dto.SystemTypes == null || dto.SystemTypes.Count == 0)
                return BadRequest("At least one system type must be selected.");

            // Normalize and validate system types
            for (int i = 0; i < dto.SystemTypes.Count; i++)
            {
                if (dto.SystemTypes[i] == "Smart Wi-Fi")
                    dto.SystemTypes[i] = "Low Current";

                if (!allowedSystemTypes.Contains(dto.SystemTypes[i]))
                    return BadRequest($"Invalid system type: {dto.SystemTypes[i]}");
            }


            if (dto.ClientId > 0)
            {
                client = await _context.Clients.FindAsync(dto.ClientId);
                if (client == null)
                    return BadRequest("Invalid client ID");
            }
            else
            {
                if (string.IsNullOrEmpty(dto.ClientName) ||
                    string.IsNullOrEmpty(dto.ClientEmail) ||
                    string.IsNullOrEmpty(dto.ClientMobile))
                    return BadRequest("New client info required");

                client = new Client
                {
                    Name = dto.ClientName,
                    Email = dto.ClientEmail,
                    Mobile = dto.ClientMobile,
                    Address = dto.ClientAddress,
                    CreatedByUserId = userId
                };

                _context.Clients.Add(client);
                await _context.SaveChangesAsync();
            }

            var project = new Project
            {
                Name = dto.Name,
                SystemTypesList = dto.SystemTypes,
                ClientId = client.Id,
                Status = status,
                CreatedByUserId = userId
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var projectDto = new ProjectDto
            {
                Id = project.Id,
                ClientId = project.ClientId,
                Name = project.Name,
                SystemTypes = project.SystemTypesList,
                ClientName = client.Name,
                ClientEmail = client.Email,
                ClientMobile = client.Mobile,
                ClientAddress = client.Address,
                Status = project.Status
            };

            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, projectDto);
        }
    }
}
