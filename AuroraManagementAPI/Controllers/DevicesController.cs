using AuroraManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuroraManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DevicesController : ControllerBase
    {
        private readonly AuroraDbContext _context;

        public DevicesController(AuroraDbContext context)
        {
            _context = context;
        }

        // GET: api/Devices
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeviceDto>>> GetDevices(
            [FromQuery] string? systemType,
            [FromQuery] string? section,
            [FromQuery] string? type)
        {
            var isManager = User.Claims
                .Where(c => c.Type == "role" || c.Type.EndsWith("role"))
                .Any(c => string.Equals(c.Value, "manager", StringComparison.OrdinalIgnoreCase));

            var query = _context.Devices.AsQueryable();

            if (!string.IsNullOrEmpty(systemType))
                query = query.Where(d => d.SystemType == systemType);
            
            if (!string.IsNullOrEmpty(type))
                query = query.Where(d => d.Type == type);

            var devices = await query.ToListAsync();

            var deviceDtos = devices.Select(d => new DeviceDto
            {
                Id = d.Id,
                DeviceCode = d.DeviceCode,   // ✅ ADD THIS
                Name = d.Name,
                Type = d.Type,
                SystemType = d.SystemType,
                StockQuantity = d.StockQuantity,
                PowerConsumption = d.PowerConsumption ,    
                Color = d.Color,
                ActualPrice = isManager ? d.ActualPrice : null,  // only manager sees
                SalesPrice = d.SalesPrice    ,  // always shown
                SupplierName = d.SystemType == "Low Current" || d.SystemType == "Smart Wi-Fi" ? d.SupplierName : null,
                ManagerPercent = d.ManagerPercent

            }).ToList();

            return Ok(deviceDtos);
        }

        // GET: api/Devices/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DeviceDto>> GetDevice(int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null) return NotFound();

            var isManager = User.Claims
                .Where(c => c.Type == "role" || c.Type.EndsWith("role"))
                .Any(c => string.Equals(c.Value, "manager", StringComparison.OrdinalIgnoreCase));

            var deviceDto = new DeviceDto
            {
                Id = device.Id,
                DeviceCode = device.DeviceCode,

                Name = device.Name,
                Type = device.Type,
                SystemType = device.SystemType,
                StockQuantity = device.StockQuantity,
                PowerConsumption = device.PowerConsumption,
                Color = device.Color,
                ActualPrice = isManager ? device.ActualPrice : null,
                SalesPrice = device.SalesPrice,
                SupplierName = device.SystemType == "Low Current" || device.SystemType == "Smart Wi-Fi" ? device.SupplierName : null,
                ManagerPercent = device.ManagerPercent

            };

            return Ok(deviceDto);
        }

        // POST: api/Devices
        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<Device>> CreateDevice(Device device)
        {
            // ✅ Validate system type from DB
            var allowedSystemTypes = await _context.SystemTypes
                .Select(st => st.Name)
                .ToListAsync();

            if (!allowedSystemTypes.Contains(device.SystemType))
                return BadRequest("Invalid system type. Must be one of: KNX, BusPro, Wireless, Low Current, Smart Wi-Fi.");

            // ✅ Auto-generate DeviceCode if not provided
            if (string.IsNullOrEmpty(device.DeviceCode))
                device.DeviceCode = $"DEV-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

            // ✅ Logic based on system type
            if (device.SystemType == "Low Current" || device.SystemType == "Smart Wi-Fi")
            {
                // These devices don’t calculate prices automatically
                _context.Devices.Add(device);
            }
            else
            {
                // Regular devices get 30% markup
                device.SalesPrice = device.ActualPrice * 1.3m;
                _context.Devices.Add(device);
            }
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetDevice), new { id = device.Id }, device);
        }

        // PUT: api/Devices/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdateDevice(int id, Device updatedDevice)
        {
            if (id != updatedDevice.Id) return BadRequest("ID mismatch");

            var device = await _context.Devices.FindAsync(id);
            if (device == null) return NotFound();
            device.DeviceCode = updatedDevice.DeviceCode;
            device.Name = updatedDevice.Name;
            device.Type = updatedDevice.Type;
            device.SystemType = updatedDevice.SystemType;
            device.StockQuantity = updatedDevice.StockQuantity;
            if (device.SystemType == "Low Current" || device.SystemType == "Smart Wi-Fi")
            {
                device.SalesPrice = updatedDevice.SalesPrice; // user-defined
                device.SupplierName = updatedDevice.SupplierName; // new!

            }
            else
            {
                device.ActualPrice = updatedDevice.ActualPrice;
                device.SalesPrice = updatedDevice.ActualPrice * 1.3m; // auto-calc
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Devices/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteDevice(int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null) return NotFound();

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();
            return NoContent();
        }





        //------------------Low Current----------------

        //[HttpPost("LocalMarketDevices")]
        //[Authorize] // users can create
        //public async Task<ActionResult<SupplierDevice>> CreateLowCurrentSupplierDevice(SupplierDevice model)
        //{
        //    var device = await _context.Devices.FindAsync(model.DeviceId);
        //    if (device == null ||
        //       (device.SystemType != "Low Current" && device.SystemType != "Smart Wi-Fi"))
        //        return BadRequest("Device must exist and be of Low Current or Smart Wi-Fi system type.");


        //    _context.SupplierDevices.Add(model);
        //    await _context.SaveChangesAsync();

        //    return Ok(model);
        //}



        [HttpPost("UserAddDevice")]
        [Authorize] // any logged-in user
        public async Task<ActionResult<Device>> UserAddDevice(Device device)
        {
            // Only allow Low Current / Smart Wi-Fi
            if (device.SystemType != "Low Current" && device.SystemType != "Smart Wi-Fi")
                return BadRequest("Users can only add Low Current or Smart Wi-Fi devices.");

            // Auto-generate code if not provided
            if (string.IsNullOrEmpty(device.DeviceCode))
                device.DeviceCode = $"DEV-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

            // SalesPrice must be provided by user
            if (device.SalesPrice <= 0)
                return BadRequest("SalesPrice must be provided by the user.");

            // ActualPrice will be calculated later by manager, so set 0 for now
            device.ActualPrice = 0;

            // Only accept supplier name for local market
            if (device.SystemType != "Low Current" && device.SystemType != "Smart Wi-Fi")
                device.SupplierName = null;

            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            return Ok(device);
        }




        public class ProfitUpdateDto
        {
            public decimal ProfitRatio { get; set; }
        }

        [HttpPut("update-actual-price/{id}")]
        [Authorize(Roles = "Manager")]

        public async Task<IActionResult> UpdateActualPrice(int id, [FromBody] decimal managerPercent)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
                return NotFound(new { message = "Device not found." });
            var existingSalesPrice = device.SalesPrice; // 🔒 store it safely

            device.ManagerPercent = managerPercent;
            device.ActualPrice = existingSalesPrice - (existingSalesPrice * (managerPercent / 100));

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Actual price updated successfully.",
                id = device.Id,
                salesPrice = existingSalesPrice,
                managerPercent = device.ManagerPercent,
                actualPrice = device.ActualPrice
            });
        }





        [HttpGet("by-project/{projectId}")]
        public async Task<ActionResult<IEnumerable<DeviceDto>>> GetDevicesForProject(int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return NotFound("Project not found");

            // Split comma-separated system types into a list
            var systemTypes = project.ProjectSystemTypes
    .Select(pst => pst.SystemType.Name)
    .ToList();


            // Fetch all devices that match any of the system types
            var devices = await _context.Devices
                .Where(d => systemTypes.Contains(d.SystemType))
                .ToListAsync();

            var isManager = User.Claims
                .Any(c => (c.Type == "role" || c.Type.EndsWith("role")) &&
                          c.Value.Equals("Manager", StringComparison.OrdinalIgnoreCase));

            var deviceDtos = devices.Select(d => new DeviceDto
            {
                Id = d.Id,
                DeviceCode = d.DeviceCode,

                Name = d.Name,
                Type = d.Type,
                SystemType = d.SystemType,
                StockQuantity = d.StockQuantity,
                PowerConsumption = d.PowerConsumption   ,
                Color = d.Color,
                ActualPrice = isManager ? d.ActualPrice : null,
                SalesPrice = d.SalesPrice,
                SupplierName = d.SupplierName,
                ManagerPercent = d.ManagerPercent
            }).ToList();

            return Ok(deviceDtos);
        }

    }
}


