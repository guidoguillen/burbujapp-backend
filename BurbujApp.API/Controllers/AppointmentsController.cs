using BurbujApp.Domain.Entities;
using BurbujApp.Persistence.Context;
using BurbujApp.Shared.DTOs.Appointments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BurbujApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly BurbujAppDbContext _context;

    public AppointmentsController(BurbujAppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointments()
    {
        var appointments = await _context.Appointments
            .Include(a => a.User)
            .Include(a => a.Service)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserName = $"{a.User.FirstName} {a.User.LastName}",
                ServiceId = a.ServiceId,
                ServiceName = a.Service.Name,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                Notes = a.Notes
            })
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .ToListAsync();

        return Ok(appointments);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentDto>> GetAppointment(int id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.User)
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
            return NotFound();

        var appointmentDto = new AppointmentDto
        {
            Id = appointment.Id,
            UserId = appointment.UserId,
            UserName = $"{appointment.User.FirstName} {appointment.User.LastName}",
            ServiceId = appointment.ServiceId,
            ServiceName = appointment.Service.Name,
            AppointmentDate = appointment.AppointmentDate,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status,
            Notes = appointment.Notes
        };

        return Ok(appointmentDto);
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> CreateAppointment(CreateAppointmentDto createAppointmentDto)
    {
        // Verificar que el servicio existe
        var service = await _context.Services.FindAsync(createAppointmentDto.ServiceId);
        if (service == null)
            return BadRequest("Servicio no encontrado");

        // Calcular la hora de fin basada en la duración del servicio
        var endTime = createAppointmentDto.StartTime.AddMinutes(service.DurationMinutes);

        var appointment = new Appointment
        {
            UserId = createAppointmentDto.UserId,
            ServiceId = createAppointmentDto.ServiceId,
            AppointmentDate = createAppointmentDto.AppointmentDate,
            StartTime = createAppointmentDto.StartTime,
            EndTime = endTime,
            Notes = createAppointmentDto.Notes,
            Status = AppointmentStatus.Scheduled
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        // Cargar datos relacionados para la respuesta
        appointment = await _context.Appointments
            .Include(a => a.User)
            .Include(a => a.Service)
            .FirstAsync(a => a.Id == appointment.Id);

        var appointmentDto = new AppointmentDto
        {
            Id = appointment.Id,
            UserId = appointment.UserId,
            UserName = $"{appointment.User.FirstName} {appointment.User.LastName}",
            ServiceId = appointment.ServiceId,
            ServiceName = appointment.Service.Name,
            AppointmentDate = appointment.AppointmentDate,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status,
            Notes = appointment.Notes
        };

        return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, appointmentDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAppointment(int id, UpdateAppointmentDto updateAppointmentDto)
    {
        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment == null)
            return NotFound();

        appointment.AppointmentDate = updateAppointmentDto.AppointmentDate;
        appointment.StartTime = updateAppointmentDto.StartTime;
        appointment.Status = updateAppointmentDto.Status;
        appointment.Notes = updateAppointmentDto.Notes;
        appointment.UpdatedAt = DateTime.UtcNow;

        // Recalcular hora de fin si es necesario
        var service = await _context.Services.FindAsync(appointment.ServiceId);
        if (service != null)
        {
            appointment.EndTime = appointment.StartTime.AddMinutes(service.DurationMinutes);
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment == null)
            return NotFound();

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("my-appointments")]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetMyAppointments()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var appointments = await _context.Appointments
            .Include(a => a.Service)
            .Where(a => a.UserId == userId)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                UserId = a.UserId,
                ServiceId = a.ServiceId,
                ServiceName = a.Service.Name,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                Notes = a.Notes
            })
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .ToListAsync();

        return Ok(appointments);
    }
}
