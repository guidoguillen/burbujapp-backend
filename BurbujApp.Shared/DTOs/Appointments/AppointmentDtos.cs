using BurbujApp.Domain.Entities;

namespace BurbujApp.Shared.DTOs.Appointments;

public class AppointmentDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Notes { get; set; }
}

public class CreateAppointmentDto
{
    public string UserId { get; set; } = string.Empty;
    public int ServiceId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public DateTime StartTime { get; set; }
    public string? Notes { get; set; }
}

public class UpdateAppointmentDto
{
    public DateTime AppointmentDate { get; set; }
    public DateTime StartTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Notes { get; set; }
}
