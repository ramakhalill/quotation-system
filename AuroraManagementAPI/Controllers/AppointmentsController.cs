// Controllers/AppointmentsController.cs
using AuroraManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuroraManagementAPI.Controllers
{
    [ApiController]
    [Authorize] // ensures only logged-in users with valid JWT can access

    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly AuroraDbContext _context;
        public AppointmentsController(AuroraDbContext context) => _context = context;

        // GET: api/Appointments?from=2025-08-01&to=2025-08-31&projectId=2
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointments(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int? projectId,
            [FromQuery] int? clientId)
        {
            var q = _context.Appointments
                .Include(a => a.Project)
                .ThenInclude(p => p.Client)
                .AsQueryable();

            if (from.HasValue) q = q.Where(a => a.EndUtc >= from.Value);
            if (to.HasValue) q = q.Where(a => a.StartUtc <= to.Value);
            if (projectId.HasValue) q = q.Where(a => a.ProjectId == projectId.Value);
            if (clientId.HasValue) q = q.Where(a => a.Project.ClientId == clientId.Value);

            var list = await q
                .OrderBy(a => a.StartUtc)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    ProjectId = a.ProjectId,
                    ProjectName = a.Project.Name,
                    ClientName = a.Project.Client.Name,
                    Title = a.Title,
                    Description = a.Description,
                    StartUtc = a.StartUtc,
                    EndUtc = a.EndUtc,
                    Location = a.Location,
                    AssignedTo = a.AssignedTo,
                    Status = a.Status,
                    AllDay = a.AllDay
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/Appointments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentDto>> GetAppointment(int id)
        {
            var a = await _context.Appointments
                .Include(x => x.Project).ThenInclude(p => p.Client)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (a == null) return NotFound();

            return new AppointmentDto
            {
                Id = a.Id,
                ProjectId = a.ProjectId,
                ProjectName = a.Project.Name,
                ClientName = a.Project.Client.Name,
                Title = a.Title,
                Description = a.Description,
                StartUtc = a.StartUtc,
                EndUtc = a.EndUtc,
                Location = a.Location,
                AssignedTo = a.AssignedTo,
                Status = a.Status,
                AllDay = a.AllDay
            };
        }

        // POST: api/Appointments
        [HttpPost]
        public async Task<ActionResult<AppointmentDto>> Create(CreateAppointmentDto dto)
        {
            var project = await _context.Projects
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId);
            if (project == null) return BadRequest("Invalid ProjectId");

            var a = new Appointment
            {
                ProjectId = dto.ProjectId,
                Title = dto.Title,
                Description = dto.Description,
                StartUtc = dto.StartUtc,
                EndUtc = dto.EndUtc,
                Location = dto.Location,
                AssignedTo = dto.AssignedTo,
                Status = dto.Status,
                AllDay = dto.AllDay
            };

            _context.Appointments.Add(a);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAppointment), new { id = a.Id }, new AppointmentDto
            {
                Id = a.Id,
                ProjectId = a.ProjectId,
                ProjectName = project.Name,
                ClientName = project.Client.Name,
                Title = a.Title,
                Description = a.Description,
                StartUtc = a.StartUtc,
                EndUtc = a.EndUtc,
                Location = a.Location,
                AssignedTo = a.AssignedTo,
                Status = a.Status,
                AllDay = a.AllDay
            });
        }

        // PUT: api/Appointments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateAppointmentDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");

            var a = await _context.Appointments.FindAsync(id);
            if (a == null) return NotFound();

            a.ProjectId = dto.ProjectId;
            a.Title = dto.Title;
            a.Description = dto.Description;
            a.StartUtc = dto.StartUtc;
            a.EndUtc = dto.EndUtc;
            a.Location = dto.Location;
            a.AssignedTo = dto.AssignedTo;
            a.Status = dto.Status;
            a.AllDay = dto.AllDay;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Appointments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var a = await _context.Appointments.FindAsync(id);
            if (a == null) return NotFound();

            _context.Appointments.Remove(a);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
