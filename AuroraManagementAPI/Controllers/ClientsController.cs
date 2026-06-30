using AuroraManagementAPI.DTOs;
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
    public class ClientsController : ControllerBase
    {
        private readonly AuroraDbContext _context;

        public ClientsController(AuroraDbContext context)
        {
            _context = context;
        }

        // GET: api/Clients
        // GET: api/Clients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientDto>>> GetClients()
        {
            var isManager = User.Claims
    .Any(c => c.Type == ClaimTypes.Role && c.Value.Equals("Manager", StringComparison.OrdinalIgnoreCase));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _context.Clients
                .Include(c => c.CreatedByUser)
                .Select(c => new ClientDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Mobile = c.Mobile,
                    Email = c.Email,
                    Address = c.Address,
                    CreatedByUserId = c.CreatedByUserId,
                    CreatedByUsername = c.CreatedByUser != null
                                        ? c.CreatedByUser.UserName
                                        : "N/A",
                    CreatedAt = c.CreatedAt  // 👈 include date

                });

            if (!isManager)
            {
                // 👤 Normal user (includes Admins) → only their own clients
                query = query.Where(c => c.CreatedByUserId == userId);
            }

            var clients = await query.ToListAsync();

            return Ok(clients);
        }

        [HttpGet("roles")]
        public IActionResult GetRoles()
        {
            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            return Ok(new { User = User.Identity?.Name, Roles = roles });
        }






        // GET: api/Clients/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClient(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Projects)
                .Include(c => c.CreatedByUser) // 👈 added this
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null) return NotFound();

            var isAdmin = User.IsInRole("Admin") || User.IsInRole("manager")|| User.IsInRole("Manager");
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!isAdmin && client.CreatedByUserId != userId)
                return Forbid();

            return Ok(client);
        }


        // GET: api/Clients/5/projects
        [HttpGet("{id}/projects")]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetClientProjects(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Projects)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null) return NotFound();

            var isAdmin = User.IsInRole("Admin") || User.IsInRole("manager") || User.IsInRole("Manager");
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!isAdmin && client.CreatedByUserId != userId)
                return Forbid();

            var projects = client.Projects.Select(p => new ProjectDto
            {
                Id = p.Id,
                ClientId = p.ClientId,
                Name = p.Name,
                SystemTypes = p.ProjectSystemTypes
            .Select(pst => pst.SystemType.Name)
            .ToList(),


                Status = p.Status,
                ClientName = client.Name,
                ClientEmail = client.Email,
                ClientMobile = client.Mobile,
                ClientAddress = client.Address
            }).ToList();

            return Ok(projects);
        }

        // POST: api/Clients
        [HttpPost]
        
        public async Task<ActionResult<Client>> CreateClient(Client client)
        {
            // get userId from JWT claims
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // assign the creator
            client.CreatedByUserId = userId;

            // nullify CreatedByUser (EF will handle it)
            client.CreatedByUser = null;
            if (client.Projects != null)
            {
                foreach (var p in client.Projects)
                {
                    p.Client = null;
                    if (p.Quotes != null)
                    {
                        foreach (var q in p.Quotes)
                        {
                            q.Client = null;
                            q.Project = null;
                        }
                    }
                }
            }

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClient), new { id = client.Id }, client);
        }


        // PUT: api/Clients/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(int id, Client updatedClient)
        {
            if (id != updatedClient.Id) return BadRequest("ID mismatch");

            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();

            var isAdmin = User.IsInRole("Admin") || User.IsInRole("manager") || User.IsInRole("Manager");
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!isAdmin && client.CreatedByUserId != userId)
                return Forbid();

            client.Name = updatedClient.Name;
            client.Email = updatedClient.Email;
            client.Mobile = updatedClient.Mobile;
            client.Address = updatedClient.Address;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Clients/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
