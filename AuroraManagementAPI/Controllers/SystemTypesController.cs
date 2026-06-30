using AuroraManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuroraManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SystemTypesController : ControllerBase
    {
        private readonly AuroraDbContext _context;

        public SystemTypesController(AuroraDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SystemType>>> GetSystemTypes()
        {
            var types = await _context.SystemTypes.ToListAsync();
            return Ok(types);
        }
    }
}
