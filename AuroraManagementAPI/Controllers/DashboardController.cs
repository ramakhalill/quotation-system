using AuroraManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuroraManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // ensures only logged-in users with valid JWT can access

    public class DashboardController : ControllerBase
    {
        private readonly AuroraDbContext _context;

        public DashboardController(AuroraDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetDashboard()
        {
            var clientsCount = await _context.Clients.CountAsync();
            var projects = await _context.Projects.ToListAsync();
            var projectsByStatus = new
            {
                Pending = projects.Count(p => p.Status == ProjectStatus.Pending),
                InProgress = projects.Count(p => p.Status == ProjectStatus.InProgress),
                Completed = projects.Count(p => p.Status == ProjectStatus.Completed)
            };

            return Ok(new
            {
                clientsCount,
                projectsCount = projects.Count,
                projectsByStatus
            });
        }
    }
}
