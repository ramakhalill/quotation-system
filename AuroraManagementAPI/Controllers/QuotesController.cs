using AuroraManagementAPI.Models;
using AuroraManagementAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace AuroraManagementAPI.Controllers
{
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class QuotesController : ControllerBase
{
    private readonly AuroraDbContext _context;

  
        private readonly UserManager<IdentityUser> _userManager;

        public QuotesController(AuroraDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // ------------------------- Create Quote -------------------------
        [HttpPost]
        public async Task<ActionResult<QuoteDto>> CreateQuote([FromBody] CreateQuoteDto dto)
        {
            if (dto.Items == null || !dto.Items.Any())
                return BadRequest("Quote must contain items.");

            // Ensure project exists
            var project = await _context.Projects
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId);

            if (project == null)
                return BadRequest("Project does not exist.");

            // Use the client linked to the project
            Client client = project.Client;

            if (client == null)
            {
                // Only create a new client if DTO provides client info
                if (!string.IsNullOrEmpty(dto.ClientName))
                {
                    client = new Client
                    {
                        Name = dto.ClientName,
                        Email = dto.ClientEmail,
                        Mobile = dto.ClientMobile,
                        Address = dto.ClientAddress
                    };
                    _context.Clients.Add(client);
                    await _context.SaveChangesAsync();

                    // Link client to project
                    project.Client = client;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    return BadRequest("Selected project has no client linked.");
                }
            }

            //Client? client = project.Client; // may be null

            // Create quote
            var quote = new Quote
            {
                ClientId = client.Id,       // <--- use client.Id
                Client = client,
                ProjectId = project.Id,     // <--- use project.Id
                Project = project,
                Notes = dto.Notes,
                DateCreated = DateTime.UtcNow,
                InstallationFee = dto.InstallationFee,
                Discount = dto.Discount,
                CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                CreatedByUserName = User.Identity.Name,

                ParentQuoteId = null,      // 🔥 أهم خط
                RevisionNumber = 0         // 🔥 كل quote رئيسي يبدأ من 0

            };

            var itemsDto = new List<QuoteItemDto>();

            // Totals
            decimal totalBase = 0m;
            decimal totalWithProfit = 0m;
            decimal totalAfterDiscount = 0m;
            decimal totalRevenue = 0m;
            decimal totalNetProfit = 0m;
            decimal totalActualCost = 0m;
            int totalPowerConsumption = 0;

            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            bool isAdmin = userRole?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;

            foreach (var item in dto.Items)
            {
                var projectSystemTypeNames = project.SystemTypesList ?? new List<string>();


                var selectedDevice = await _context.Devices
                    .Where(d => d.Id == item.DeviceId &&
                                (d.SystemType == null || projectSystemTypeNames.Contains(d.SystemType)) &&
                                (d.PowerConsumption == 0 || d.PowerConsumption == item.PowerConsumption) &&
                                (d.Color == null || d.Color == "" || d.Color == item.Color || item.Color == "Default"))
                    .FirstOrDefaultAsync();

                if (selectedDevice == null)
                {
                    string systemTypesText = projectSystemTypeNames.Any()
                        ? string.Join(", ", projectSystemTypeNames)
                        : "N/A";

                    return BadRequest($"Device {item.DeviceId} is not valid for project system types '{string.Join(", ", systemTypesText)}'.");
                }

                // ---------------- POWER CONSUMPTION SUM ----------------
                if (selectedDevice.PowerConsumption.HasValue && selectedDevice.PowerConsumption.Value > 0)
                {
                    totalPowerConsumption += selectedDevice.PowerConsumption.Value * item.Quantity;
                }



                // Guard: profit ratio 100% would cause division by zero in manager-style calc
                if (item.ProfitRatio >= 100m)
                    return BadRequest("ProfitRatio must be less than 100.");



                // Calculations
                //decimal unitSalesPrice = selectedDevice.SalesPrice;
                //decimal unitWithProfit = unitSalesPrice * (1 + item.ProfitRatio / 100m);
                //decimal unitAfterDiscount = unitWithProfit * (1 - dto.Discount / 100m);

                //decimal baseTotal = unitSalesPrice * item.Quantity;
                //decimal profitTotal = unitWithProfit * item.Quantity;
                //decimal afterDiscountTotal = unitAfterDiscount * item.Quantity;

                //totalBase += baseTotal;
                //totalWithProfit += profitTotal;
                //totalAfterDiscount += afterDiscountTotal;

                //totalRevenue += afterDiscountTotal - baseTotal;

                //decimal netProfitForItem = (unitAfterDiscount - selectedDevice.ActualPrice) * item.Quantity;
                //totalNetProfit += netProfitForItem;

                // ---------------- NEW CALCULATION LOGIC ----------------

                decimal unitSalesPrice = selectedDevice.SalesPrice;

                // Manager logic: profit ratio defines the *actual percentage of profit after total*
                decimal unitWithProfit = unitSalesPrice / (1 - (item.ProfitRatio / 100m));

                // total for this device
                decimal baseTotal = unitSalesPrice * item.Quantity;
                decimal profitTotal = unitWithProfit * item.Quantity;

                // accumulate totals before discount
                totalBase += baseTotal;
                totalWithProfit += profitTotal;

                // revenue = sales-based profit before discount
                //totalRevenue += profitTotal - baseTotal;

                totalActualCost += selectedDevice.ActualPrice * item.Quantity;





                // net profit = after subtracting actual cost
                // 1) base profit before discount
                //decimal netProfitBeforeDiscount =
                //    (unitWithProfit - selectedDevice.ActualPrice) * item.Quantity;

                //// 2) apply the same discount as totalWithProfit
                //decimal discountFactor = 1 - (dto.Discount / 100m);

                //// 3) final net profit after discount
                //decimal netProfitAfterDiscount = netProfitBeforeDiscount * discountFactor;

                //totalNetProfit += netProfitAfterDiscount;

                // Create quote item
                var quoteItem = new QuoteItem
                {
                    Device = selectedDevice,
                    Quantity = item.Quantity,
                    UnitPrice = unitSalesPrice,
                    TotalPrice = baseTotal,
                    ProfitRatio = item.ProfitRatio,
                    PriceAfterProfit = profitTotal
                };
                quote.Items.Add(quoteItem);

                itemsDto.Add(new QuoteItemDto
                {
                    DeviceId = selectedDevice.Id,
                    DeviceName = selectedDevice.Name,
                    Quantity = item.Quantity,
                    UnitPrice = Math.Round(unitSalesPrice, 2),
                    TotalPrice = Math.Round(baseTotal, 2),
                    ProfitRatio = item.ProfitRatio,
                    PriceAfterProfit =  Math.Round(profitTotal, 2)
                });
            }

            decimal discountAmount = totalWithProfit * (dto.Discount / 100m);
             totalAfterDiscount = totalWithProfit - discountAmount;

            // totalRevenue should reflect discount
            totalRevenue = totalAfterDiscount - totalBase;


            // sumDiff = totalBase - totalActualCost = Σ(qty * (salesPrice - actualPrice))
            decimal sumDiff = totalBase - totalActualCost;

            // totalNetProfit = totalRevenue + sumDiff  (your requested formula)
             totalNetProfit = totalRevenue + sumDiff;



            // Final amounts
            decimal finalTotal = totalAfterDiscount + dto.InstallationFee;

            quote.FinalTotal = Math.Round(finalTotal, 2);
            quote.NetProfit = Math.Round(totalNetProfit, 2);
            quote.TotalRevenue = Math.Round(totalRevenue, 2);
            quote.TotalPowerConsumption = totalPowerConsumption;


            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();

            var quoteDto = new QuoteDto
            {
                Id = quote.Id,
                ClientId = client.Id,
                ClientName = client.Name,
                ProjectId = project.Id,
                ProjectName = project.Name,
                Notes = quote.Notes,
                TotalQuotePrice = Math.Round(totalBase, 2),
                TotalPriceWithProfit = Math.Round(totalWithProfit, 2),
                TotalRevenue =  Math.Round(totalRevenue, 2),
                InstallationFee = dto.InstallationFee,
                Discount = dto.Discount,
                FinalTotal = Math.Round(finalTotal, 2),
                NetProfit = isAdmin ? 0 : Math.Round(totalNetProfit, 2),
                Status = quote.Status,
                ApprovedBy = quote.ApprovedByUserId,
                DateCreated = quote.DateCreated,
                Items = itemsDto,
                TotalPowerConsumption = totalPowerConsumption  // ADD THIS

            };

            return CreatedAtAction(nameof(GetQuote), new { id = quote.Id }, quoteDto);
        }







        // ------------------------- Get Quote -------------------------
        //    [HttpGet("{id}")]
        //public async Task<ActionResult<QuoteDto>> GetQuote(int id)
        //{
        //    var quote = await _context.Quotes
        //        .Include(q => q.Client)
        //        .Include(q => q.Project)
        //        .Include(q => q.Items)
        //        .ThenInclude(i => i.Device)
        //        .FirstOrDefaultAsync(q => q.Id == id);

        //    if (quote == null) return NotFound();

        //    var itemsDto = quote.Items.Select(i => new QuoteItemDto
        //    {
        //        DeviceId = i.DeviceId,
        //        DeviceName = i.Device?.Name ?? "Unknown",
        //        Quantity = i.Quantity,
        //        UnitPrice = i.UnitPrice,
        //        ProfitRatio = i.ProfitRatio,
        //        TotalPrice = i.TotalPrice,
        //        PriceAfterProfit = i.PriceAfterProfit
        //    }).ToList();

        //    decimal totalQuotePrice = quote.Items.Sum(i => i.TotalPrice);
        //    decimal totalPriceWithProfit = quote.Items.Sum(i => i.PriceAfterProfit);
        //    decimal totalRevenue = totalPriceWithProfit - totalQuotePrice;

        //    var quoteDto = new QuoteDto
        //    {
        //        Id = quote.Id,
        //        ClientId = quote.ClientId,
        //        ClientName = quote.Client?.Name ?? "Unknown",
        //        ProjectId = quote.ProjectId,
        //        ProjectName = quote.Project?.Name ?? "Unknown",
        //        Notes = quote.Notes,
        //        TotalQuotePrice = totalQuotePrice,
        //        TotalPriceWithProfit = totalPriceWithProfit,
        //        TotalRevenue = totalRevenue,
        //        Items = itemsDto
        //    };

        //    return quoteDto;
        //}
        [HttpGet("{id}")]
        public async Task<ActionResult<QuoteDto>> GetQuote(int id)
        {
            var quote = await _context.Quotes
                .Include(q => q.Client)
                .Include(q => q.Project)
                .Include(q => q.Items)
                .ThenInclude(i => i.Device)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quote == null)
                return NotFound();

            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            bool isManager = userRole?.Equals("Manager", StringComparison.OrdinalIgnoreCase) == true;


            var itemsDto = quote.Items.Select(i => new QuoteItemDto
            {
                DeviceId = i.DeviceId,
                DeviceName = i.Device?.Name ?? "Unknown",
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                ProfitRatio = i.ProfitRatio,
                TotalPrice = i.TotalPrice,
                PriceAfterProfit = i.PriceAfterProfit
            }).ToList();

            var quoteDto = new QuoteDto
            {
                Id = quote.Id,
                ClientId = quote.ClientId,
                ClientName = quote.Client?.Name ?? "Unknown",
                ProjectId = quote.ProjectId,
                ProjectName = quote.Project?.Name ?? "Unknown",
                Notes = quote.Notes,
                DateCreated = quote.DateCreated,
                Status = quote.Status,
                CreatedByUserId = quote.CreatedByUserId,
                CreatedByUserName = quote.CreatedByUserId != null
    ? _userManager.Users.FirstOrDefault(u => u.Id == quote.CreatedByUserId)?.UserName
    : null,
                ApprovedBy = quote.ApprovedByUserId,
                RevisionNumber = quote.RevisionNumber, // ✅ Add this
                ParentQuoteId = quote.ParentQuoteId,   // ✅ Add this
                TotalQuotePrice = quote.Items.Sum(i => i.TotalPrice),
                TotalPriceWithProfit = quote.Items.Sum(i => i.PriceAfterProfit),
                Discount = quote.Discount,
                InstallationFee = quote.InstallationFee,
                FinalTotal = quote.FinalTotal,
                TotalRevenue = quote.TotalRevenue,
                NetProfit = isManager ? quote.NetProfit : 0, // only for manager
                Items = itemsDto
            };

            return Ok(quoteDto);
        }

        // ------------------------------------------------------
        // 👁 GET QUOTES FOR ADMIN (No profit info)
        // ------------------------------------------------------
        [HttpGet("admin")]

        
        public async Task<ActionResult<IEnumerable<QuoteDto>>> GetQuotesForAdmin()
        {
            var quotes = await _context.Quotes
                .Include(q => q.Client)
                .Include(q => q.Project)
                .Include(q => q.Items)
                .ThenInclude(i => i.Device)
                .OrderBy(q => q.ParentQuoteId ?? q.Id)       // Group revisions together
                .ThenByDescending(q => q.RevisionNumber)     // Show latest revision first
                .ToListAsync();


            var result = quotes.Select(q => new QuoteDto
            {
                Id = q.Id,
                ClientName = q.Client?.Name??"",
                ProjectName = q.Project.Name,
                Notes = q.Notes,
                DateCreated = q.DateCreated,
                Status = q.Status,
                InstallationFee = q.InstallationFee,
                Discount = q.Discount,
                RevisionNumber = q.RevisionNumber, // ✅ show revision number
                ParentQuoteId = q.ParentQuoteId,
                TotalQuotePrice = Math.Round(q.Items.Sum(i => i.TotalPrice), 2),
                // Hide financials for Admin
                TotalPriceWithProfit = q.Items.Sum(i => i.PriceAfterProfit),
                TotalRevenue = q.TotalRevenue,   // استخدم القيمة المخزنة عند الإنشاء
                NetProfit = 0,         // استخدم القيمة المخزنة عند الإنشاء
                FinalTotal = q.FinalTotal,       // استخدم القيمة المخزنة عند الإنشاء
                ApprovedBy = q.ApprovedByUserId,
                CreatedByUserId = q.CreatedByUserId,
                CreatedByUserName = q.CreatedByUserId != null
    ? _userManager.Users.FirstOrDefault(u => u.Id == q.CreatedByUserId)?.UserName
    : null,
                Items = q.Items.Select(i => new QuoteItemDto
                {
                    DeviceName = i.Device.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice,
                    ProfitRatio = i.ProfitRatio,
                    PriceAfterProfit = i.PriceAfterProfit
                }).ToList()
            });

            return Ok(result);
        }

        // ------------------------------------------------------
        // 👨‍💼 GET QUOTES FOR MANAGER (Full details)
        // ------------------------------------------------------
        [HttpGet("Manager")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<IEnumerable<QuoteDto>>> GetQuotesForManager()
        {
            var quotes = await _context.Quotes
                .Include(q => q.Client)
                .Include(q => q.Project)
                .Include(q => q.Items)
                .ThenInclude(i => i.Device)
                .OrderBy(q => q.ParentQuoteId ?? q.Id)       // Group revisions with parent
                .ThenByDescending(q => q.RevisionNumber)     // Latest revision first
                .ToListAsync();

            var result = quotes.Select(q => new QuoteDto
            {
                Id = q.Id,
                ClientName = q.Client.Name,
                ProjectName = q.Project.Name,
                Notes = q.Notes,
                DateCreated = q.DateCreated,
                Status = q.Status,
                InstallationFee = q.InstallationFee,
                Discount = q.Discount,
                RevisionNumber = q.RevisionNumber, // ✅ Added
                ParentQuoteId = q.ParentQuoteId,
                TotalQuotePrice = Math.Round(q.Items.Sum(i => i.TotalPrice), 2),
                TotalPriceWithProfit = q.Items.Sum(i => i.PriceAfterProfit),
                TotalRevenue = q.TotalRevenue,   // استخدم القيمة المخزنة عند الإنشاء
                NetProfit = q.NetProfit,         // استخدم القيمة المخزنة عند الإنشاء
                FinalTotal = q.FinalTotal,       // استخدم القيمة المخزنة عند الإنشاء
                ApprovedBy = q.ApprovedByUserId,
                CreatedByUserId = q.CreatedByUserId,
                CreatedByUserName = q.CreatedByUserId != null
    ? _userManager.Users.FirstOrDefault(u => u.Id == q.CreatedByUserId)?.UserName
    : null,
                Items = q.Items.Select(i => new QuoteItemDto
                {
                    DeviceName = i.Device.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice,
                    ProfitRatio = i.ProfitRatio,
                    PriceAfterProfit = i.PriceAfterProfit
                }).ToList()
            });

            return Ok(result);
        }



        // ------------------------- Missing Devices PDF -------------------------
        [HttpGet("{quoteId}/MissingDevicesPdf")]
    public async Task<IActionResult> DownloadMissingDevicesPdf(int quoteId)
    {
        var missing = await _context.MissingDevices
            .Include(m => m.Device)
            .Include(m => m.Quote)
            .ThenInclude(q => q.Project)
            .Include(m => m.Quote)
            .ThenInclude(q => q.Client)
            .Where(m => m.QuoteId == quoteId)
            .ToListAsync();

        if (!missing.Any())
            return NotFound("No missing devices for this quote.");

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.Header().Text($"Missing Devices Report - Quote #{quoteId}").FontSize(16).SemiBold();

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.ConstantColumn(100);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Device").Bold();
                        header.Cell().Text("Missing Qty").Bold();
                        header.Cell().Text("Project").Bold();
                        header.Cell().Text("Client").Bold();
                    });

                    foreach (var m in missing)
                    {
                        table.Cell().Text(m.Device?.Name ?? "Unknown");
                        table.Cell().Text(m.MissingQuantity.ToString());
                        table.Cell().Text(m.Quote?.Project?.Name ?? "Unknown Project");
                        table.Cell().Text(m.Quote?.Client?.Name ?? "Unknown Client");
                    }
                });
            });
        }).GeneratePdf();

        return File(pdfBytes, "application/pdf", $"MissingDevices_Quote_{quoteId}.pdf");
    }

    // ------------------------- Approve Quote -------------------------
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> AcceptQuote(int id)
    {
        var quote = await _context.Quotes
            .Include(q => q.Items)
            .ThenInclude(i => i.Device)
            .FirstOrDefaultAsync(q => q.Id == id);

            if (quote == null)
                return NotFound("Quote not found.");

            if (quote.Status == QuoteStatus.Approved)
                return BadRequest("Quote already fully approved by client.");

            if (quote.Status == QuoteStatus.Accepted)
                return BadRequest("Quote already accepted by manager.");

            // ✅ فقط تغيير الحالة بدون التأثير على الستوك
            quote.Status = QuoteStatus.Accepted;
            quote.ApprovedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _context.SaveChangesAsync();

            return Ok("Quote accepted successfully (waiting for client approval).");
        }

        // ------------------------- Decline Quote -------------------------
        [HttpPost("{id}/decline")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeclineQuote(int id)
        {
            var quote = await _context.Quotes.FindAsync(id);
            if (quote == null) return NotFound();

            quote.Status = QuoteStatus.Declined;
            await _context.SaveChangesAsync();
            return Ok("Quote declined.");
        }

        [HttpPost("{id}/approve-client")]
        [Authorize(Roles = "Manager,admin")]
        public async Task<IActionResult> ApproveQuoteByClient(int id)
        {
            var quote = await _context.Quotes
                .Include(q => q.Items)
                .ThenInclude(i => i.Device)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quote == null)
                return NotFound("Quote not found.");

            // ✅ لازم تكون Accepted قبل الموافقة النهائية
            if (quote.Status != QuoteStatus.Accepted)
                return BadRequest("Quote must be accepted by the manager first.");

            // ✅ سحب الأجهزة وتسجيل المفقود
            foreach (var item in quote.Items)
            {
                var device = item.Device;
                if (device.StockQuantity >= item.Quantity)
                {
                    device.StockQuantity -= item.Quantity;
                }
                else
                {
                    var missingQty = item.Quantity - device.StockQuantity;
                    device.StockQuantity = 0;

                    _context.MissingDevices.Add(new MissingDevice
                    {
                        Quote = quote,
                        Device = device,
                        MissingQuantity = missingQty,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            quote.Status = QuoteStatus.Approved;
            quote.ApprovedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _context.SaveChangesAsync();

            return Ok("Quote approved successfully (client confirmed and stock updated).");
        }




















        // ------------------------- Update Quote -------------------------
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateQuote(int id, [FromBody] UpdateQuoteDto dto)
        {
            var existingQuote = await _context.Quotes
                .Include(q => q.Items)
                    .ThenInclude(i => i.Device)
                .Include(q => q.Project)
                    .ThenInclude(p => p.Client)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (existingQuote == null)
                return NotFound("Quote not found.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            bool isAdmin = userRole?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;
            bool isManager = userRole?.Equals("Manager", StringComparison.OrdinalIgnoreCase) == true;
            bool isSales = userRole?.Equals("Sales", StringComparison.OrdinalIgnoreCase) == true;
            bool isAdminOrSales = isAdmin || isManager || isSales;

            // ------------------- ACCEPTED QUOTE → CREATE REVISION -------------------
            if (existingQuote.Status == QuoteStatus.Accepted && isAdminOrSales)
            {
                int maxRevision = await _context.Quotes
                    .Where(q => q.ParentQuoteId == existingQuote.Id || q.Id == existingQuote.Id)
                    .MaxAsync(q => (int?)q.RevisionNumber) ?? 0;

                var newQuote = new Quote
                {
                    ParentQuoteId = existingQuote.Id,
                    RevisionNumber = maxRevision + 1,
                    ClientId = existingQuote.ClientId,
                    ProjectId = existingQuote.ProjectId,
                    Notes = dto.Notes ?? existingQuote.Notes,
                    InstallationFee = dto.InstallationFee ?? existingQuote.InstallationFee,
                    Discount = dto.Discount ?? existingQuote.Discount,
                    CreatedByUserId = userId,
                    Status = QuoteStatus.Revision,
                    DateCreated = DateTime.UtcNow,
                    Items = new List<QuoteItem>()
                };

                // Totals (same approach as CreateQuote)
                decimal totalBase = 0m;
                decimal totalWithProfit = 0m;
                decimal totalActualCost = 0m;

                var revisionItems = (dto.Items != null && dto.Items.Any())
                    ? dto.Items
                    : existingQuote.Items.Select(i => new UpdateQuoteItemDto
                    {
                        DeviceId = i.DeviceId,
                        Quantity = i.Quantity,
                        ProfitRatio = i.ProfitRatio
                    }).ToList();

                foreach (var item in revisionItems)
                {
                    var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == item.DeviceId);
                    if (device == null)
                        return BadRequest($"Device {item.DeviceId} not found.");

                    var existingItem = existingQuote.Items.FirstOrDefault(i => i.DeviceId == item.DeviceId);
                    int quantity = item.Quantity != 0 ? item.Quantity : existingItem?.Quantity ?? 1;
                    decimal profitRatio = item.ProfitRatio != 0 ? item.ProfitRatio : existingItem?.ProfitRatio ?? 0m;

                    decimal unitSalesPrice = device.SalesPrice;

                    // MATCHES CREATE QUOTE: manager-style unitWithProfit formula
                    decimal unitWithProfit = unitSalesPrice / (1 - (profitRatio / 100m));

                    decimal baseTotal = unitSalesPrice * quantity;
                    decimal priceWithProfitTotal = unitWithProfit * quantity;

                    totalBase += baseTotal;
                    totalWithProfit += priceWithProfitTotal;
                    totalActualCost += device.ActualPrice * quantity;

                    newQuote.Items.Add(new QuoteItem
                    {
                        DeviceId = device.Id,
                        Quantity = quantity,
                        UnitPrice = unitSalesPrice,
                        TotalPrice = baseTotal,
                        ProfitRatio = profitRatio,
                        PriceAfterProfit = priceWithProfitTotal // store total-with-profit OR per-unit? CreateQuote stores total per quantity — choose consistent shape
                    });
                }

                // Apply discount (percentage on totalWithProfit) — same as CreateQuote
                decimal discountAmount = totalWithProfit * (newQuote.Discount / 100m);
                decimal totalAfterDiscount = totalWithProfit - discountAmount;

                // totalRevenue = totalAfterDiscount - totalBase
                decimal totalRevenue = totalAfterDiscount - totalBase;

                // sumDiff = totalBase - totalActualCost
                decimal sumDiff = totalBase - totalActualCost;

                // totalNetProfit = totalRevenue + sumDiff  (same formula as CreateQuote)
                decimal totalNetProfit = totalRevenue + sumDiff;

                // Final amounts
                newQuote.FinalTotal = Math.Round(totalAfterDiscount + newQuote.InstallationFee, 2);
                newQuote.TotalRevenue = Math.Round(totalRevenue, 2);
                newQuote.NetProfit = Math.Round(totalNetProfit, 2);

                _context.Quotes.Add(newQuote);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "New revision created successfully.",
                    newQuoteId = newQuote.Id,
                    revisionNumber = newQuote.RevisionNumber,
                    status = newQuote.Status.ToString()
                });
            }

            // ------------------- NOT ACCEPTED → UPDATE ORIGINAL -------------------
            existingQuote.ClientId = existingQuote.ClientId;
            existingQuote.ProjectId = existingQuote.ProjectId;
            existingQuote.Notes = dto.Notes ?? existingQuote.Notes;
            existingQuote.Discount = dto.Discount ?? existingQuote.Discount;
            existingQuote.InstallationFee = dto.InstallationFee ?? existingQuote.InstallationFee;

            decimal totalBaseFinal = 0m;
            decimal totalWithProfitFinal = 0m;
            decimal totalActualCostFinal = 0m;

            var updatedItems = (dto.Items != null && dto.Items.Any())
                ? dto.Items
                : existingQuote.Items.Select(i => new UpdateQuoteItemDto
                {
                    DeviceId = i.DeviceId,
                    Quantity = i.Quantity,
                    ProfitRatio = i.ProfitRatio
                }).ToList();

            existingQuote.Items.Clear();
            existingQuote.ParentQuoteId = existingQuote.ParentQuoteId;

            foreach (var item in updatedItems)
            {
                var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == item.DeviceId);
                if (device == null)
                    return BadRequest($"Device {item.DeviceId} not found.");

                var existingItem = existingQuote.Items.FirstOrDefault(i => i.DeviceId == item.DeviceId);
                int quantity = item.Quantity != 0 ? item.Quantity : existingItem?.Quantity ?? 1;
                decimal profitRatio = item.ProfitRatio != 0 ? item.ProfitRatio : existingItem?.ProfitRatio ?? 0m;

                decimal unitSalesPrice = device.SalesPrice;

                // SAME formula as CreateQuote
                decimal unitWithProfit = unitSalesPrice / (1 - (profitRatio / 100m));

                decimal baseTotal = unitSalesPrice * quantity;
                decimal priceWithProfitTotal = unitWithProfit * quantity;

                totalBaseFinal += baseTotal;
                totalWithProfitFinal += priceWithProfitTotal;
                totalActualCostFinal += device.ActualPrice * quantity;

                existingQuote.Items.Add(new QuoteItem
                {
                    DeviceId = device.Id,
                    Quantity = quantity,
                    UnitPrice = unitSalesPrice,
                    TotalPrice = baseTotal,
                    ProfitRatio = profitRatio,
                    PriceAfterProfit = priceWithProfitTotal
                });
            }

            // Apply discount (percentage on totalWithProfitFinal)
            decimal discountAmountFinal = totalWithProfitFinal * (existingQuote.Discount / 100m);
            decimal totalAfterDiscountFinal = totalWithProfitFinal - discountAmountFinal;

            // totalRevenue = totalAfterDiscountFinal - totalBaseFinal
            decimal totalRevenueFinal = totalAfterDiscountFinal - totalBaseFinal;

            // sumDiff = totalBaseFinal - totalActualCostFinal
            decimal sumDiffFinal = totalBaseFinal - totalActualCostFinal;

            // totalNetProfit = totalRevenueFinal + sumDiffFinal
            decimal totalNetProfitCalc = totalRevenueFinal + sumDiffFinal;

            existingQuote.FinalTotal = Math.Round(totalAfterDiscountFinal + existingQuote.InstallationFee, 2);
            existingQuote.TotalRevenue = Math.Round(totalRevenueFinal, 2);
            existingQuote.NetProfit = Math.Round(totalNetProfitCalc, 2);
            existingQuote.DateCreated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Quote updated successfully.",
                quoteId = existingQuote.Id,
                revisionNumber = existingQuote.RevisionNumber,
                status = existingQuote.Status.ToString()
            });
        }












        [HttpGet("GetRevisions/{parentQuoteId}")]
        public async Task<IActionResult> GetRevisionsForQuote(int parentQuoteId)
        {
            var revisions = await _context.Quotes
                .Where(q => q.ParentQuoteId == parentQuoteId || q.Id == parentQuoteId)
                .OrderBy(q => q.RevisionNumber)
                .ToListAsync();

            return Ok(revisions);
        }


















        // DELETE: api/Quotes/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteQuote(int id)
        {
            var quote = await _context.Quotes.FindAsync(id);
            if (quote == null) return NotFound();

            _context.Quotes.Remove(quote);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "Admin,manager")] // Only authorized roles
        public async Task<ActionResult> ApproveQuote(int id)
        {
            var quote = await _context.Quotes.FindAsync(id);
            if (quote == null)
                return NotFound("Quote not found.");

            if (quote.Status == QuoteStatus.Approved)
                return BadRequest("Quote is already approved.");

            if (quote.Status != QuoteStatus.Accepted)
                return BadRequest("Only accepted quotes can be approved.");

            quote.Status = QuoteStatus.Approved;
            quote.ApprovedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //quote.DateApproved = DateTime.UtcNow; // optional: track when approved

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Quote approved successfully.",
                quoteId = quote.Id,
                status = quote.Status.ToString()
            });
        }


    }
}

