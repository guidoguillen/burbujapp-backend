using BurbujApp.Domain.Entities;
using BurbujApp.Persistence.Context;
using BurbujApp.Shared.DTOs.Invoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BurbujApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly BurbujAppDbContext _context;

    public InvoicesController(BurbujAppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetInvoices()
    {
        var invoices = await _context.Invoices
            .Include(i => i.User)
            .Include(i => i.Items)
                .ThenInclude(item => item.Service)
            .Select(i => new InvoiceDto
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                UserId = i.UserId,
                UserName = $"{i.User.FirstName} {i.User.LastName}",
                IssueDate = i.IssueDate,
                DueDate = i.DueDate,
                SubTotal = i.SubTotal,
                Tax = i.Tax,
                Total = i.Total,
                Status = i.Status,
                Notes = i.Notes,
                Items = i.Items.Select(item => new InvoiceItemDto
                {
                    Id = item.Id,
                    ServiceId = item.ServiceId,
                    ServiceName = item.Service.Name,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Total = item.Total
                }).ToList()
            })
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync();

        return Ok(invoices);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InvoiceDto>> GetInvoice(int id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.User)
            .Include(i => i.Items)
                .ThenInclude(item => item.Service)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null)
            return NotFound();

        var invoiceDto = new InvoiceDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            UserId = invoice.UserId,
            UserName = $"{invoice.User.FirstName} {invoice.User.LastName}",
            IssueDate = invoice.IssueDate,
            DueDate = invoice.DueDate,
            SubTotal = invoice.SubTotal,
            Tax = invoice.Tax,
            Total = invoice.Total,
            Status = invoice.Status,
            Notes = invoice.Notes,
            Items = invoice.Items.Select(item => new InvoiceItemDto
            {
                Id = item.Id,
                ServiceId = item.ServiceId,
                ServiceName = item.Service.Name,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Total = item.Total
            }).ToList()
        };

        return Ok(invoiceDto);
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> CreateInvoice(CreateInvoiceDto createInvoiceDto)
    {
        // Verificar que el usuario existe
        var user = await _context.Users.FindAsync(createInvoiceDto.UserId);
        if (user == null)
            return BadRequest("Usuario no encontrado");

        // Generar número de factura único
        var invoiceNumber = await GenerateInvoiceNumber();

        // Calcular totales
        decimal subTotal = 0;
        var invoiceItems = new List<InvoiceItem>();

        foreach (var itemDto in createInvoiceDto.Items)
        {
            var service = await _context.Services.FindAsync(itemDto.ServiceId);
            if (service == null)
                return BadRequest($"Servicio con ID {itemDto.ServiceId} no encontrado");

            var unitPrice = itemDto.UnitPrice > 0 ? itemDto.UnitPrice : service.Price;
            var itemTotal = unitPrice * itemDto.Quantity;
            subTotal += itemTotal;

            var invoiceItem = new InvoiceItem
            {
                ServiceId = itemDto.ServiceId,
                Description = string.IsNullOrEmpty(itemDto.Description) ? service.Name : itemDto.Description,
                Quantity = itemDto.Quantity,
                UnitPrice = unitPrice,
                Total = itemTotal
            };

            invoiceItems.Add(invoiceItem);
        }

        // Calcular impuestos (ejemplo: 19% IVA)
        var tax = subTotal * 0.19m;
        var total = subTotal + tax;

        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            UserId = createInvoiceDto.UserId,
            DueDate = createInvoiceDto.DueDate,
            SubTotal = subTotal,
            Tax = tax,
            Total = total,
            Notes = createInvoiceDto.Notes,
            Items = invoiceItems
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        // Cargar datos relacionados para la respuesta
        invoice = await _context.Invoices
            .Include(i => i.User)
            .Include(i => i.Items)
                .ThenInclude(item => item.Service)
            .FirstAsync(i => i.Id == invoice.Id);

        var invoiceDto = new InvoiceDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            UserId = invoice.UserId,
            UserName = $"{invoice.User.FirstName} {invoice.User.LastName}",
            IssueDate = invoice.IssueDate,
            DueDate = invoice.DueDate,
            SubTotal = invoice.SubTotal,
            Tax = invoice.Tax,
            Total = invoice.Total,
            Status = invoice.Status,
            Notes = invoice.Notes,
            Items = invoice.Items.Select(item => new InvoiceItemDto
            {
                Id = item.Id,
                ServiceId = item.ServiceId,
                ServiceName = item.Service.Name,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Total = item.Total
            }).ToList()
        };

        return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoiceDto);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateInvoiceStatus(int id, [FromBody] InvoiceStatus status)
    {
        var invoice = await _context.Invoices.FindAsync(id);

        if (invoice == null)
            return NotFound();

        invoice.Status = status;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Estado de la factura actualizado exitosamente" });
    }

    [HttpGet("by-user/{userId}")]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetInvoicesByUser(string userId)
    {
        var invoices = await _context.Invoices
            .Include(i => i.Items)
                .ThenInclude(item => item.Service)
            .Where(i => i.UserId == userId)
            .Select(i => new InvoiceDto
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                UserId = i.UserId,
                IssueDate = i.IssueDate,
                DueDate = i.DueDate,
                SubTotal = i.SubTotal,
                Tax = i.Tax,
                Total = i.Total,
                Status = i.Status,
                Notes = i.Notes,
                Items = i.Items.Select(item => new InvoiceItemDto
                {
                    Id = item.Id,
                    ServiceId = item.ServiceId,
                    ServiceName = item.Service.Name,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Total = item.Total
                }).ToList()
            })
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync();

        return Ok(invoices);
    }

    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetPendingInvoices()
    {
        var invoices = await _context.Invoices
            .Include(i => i.User)
            .Where(i => i.Status == InvoiceStatus.Sent && i.DueDate < DateTime.UtcNow)
            .Select(i => new InvoiceDto
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                UserId = i.UserId,
                UserName = $"{i.User.FirstName} {i.User.LastName}",
                IssueDate = i.IssueDate,
                DueDate = i.DueDate,
                SubTotal = i.SubTotal,
                Tax = i.Tax,
                Total = i.Total,
                Status = i.Status,
                Notes = i.Notes
            })
            .OrderBy(i => i.DueDate)
            .ToListAsync();

        return Ok(invoices);
    }

    private async Task<string> GenerateInvoiceNumber()
    {
        var year = DateTime.UtcNow.Year;
        var lastInvoice = await _context.Invoices
            .Where(i => i.InvoiceNumber.StartsWith($"FAC-{year}-"))
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastInvoice != null)
        {
            var lastNumberStr = lastInvoice.InvoiceNumber.Split('-')[2];
            if (int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"FAC-{year}-{nextNumber:D6}";
    }
}
