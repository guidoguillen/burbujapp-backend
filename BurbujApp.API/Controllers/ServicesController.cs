using BurbujApp.Domain.Entities;
using BurbujApp.Persistence.Context;
using BurbujApp.Shared.DTOs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BurbujApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly BurbujAppDbContext _context;

    public ServicesController(BurbujAppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceDto>>> GetServices()
    {
        var services = await _context.Services
            .Where(s => s.IsActive)
            .Select(s => new ServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Price = s.Price,
                DurationMinutes = s.DurationMinutes,
                IsActive = s.IsActive
            })
            .ToListAsync();

        return Ok(services);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceDto>> GetService(int id)
    {
        var service = await _context.Services.FindAsync(id);

        if (service == null)
            return NotFound();

        var serviceDto = new ServiceDto
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description,
            Price = service.Price,
            DurationMinutes = service.DurationMinutes,
            IsActive = service.IsActive
        };

        return Ok(serviceDto);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceDto>> CreateService(CreateServiceDto createServiceDto)
    {
        var service = new Service
        {
            Name = createServiceDto.Name,
            Description = createServiceDto.Description,
            Price = createServiceDto.Price,
            DurationMinutes = createServiceDto.DurationMinutes
        };

        _context.Services.Add(service);
        await _context.SaveChangesAsync();

        var serviceDto = new ServiceDto
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description,
            Price = service.Price,
            DurationMinutes = service.DurationMinutes,
            IsActive = service.IsActive
        };

        return CreatedAtAction(nameof(GetService), new { id = service.Id }, serviceDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateService(int id, UpdateServiceDto updateServiceDto)
    {
        var service = await _context.Services.FindAsync(id);

        if (service == null)
            return NotFound();

        service.Name = updateServiceDto.Name;
        service.Description = updateServiceDto.Description;
        service.Price = updateServiceDto.Price;
        service.DurationMinutes = updateServiceDto.DurationMinutes;
        service.IsActive = updateServiceDto.IsActive;
        service.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteService(int id)
    {
        var service = await _context.Services.FindAsync(id);

        if (service == null)
            return NotFound();

        service.IsActive = false;
        service.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
