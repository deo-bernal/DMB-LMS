using Dmb.Lms.Api.Filters;
using Dmb.Lms.Model.Dtos;
using Dmb.Lms.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dmb.Lms.Api.Controllers;

[ApiController]
[Authorize]
[ServiceFilter(typeof(LocationContextFilter))]
public class WorkspaceController : ControllerBase
{
    private readonly ILmsService _lms;
    private readonly IFileService _files;
    public WorkspaceController(ILmsService lms, IFileService files)
    {
        _lms = lms;
        _files = files;
    }

    [HttpGet("api/students")]
    public async Task<IActionResult> Students(CancellationToken ct)
    {
        var (userId, locationId, role) = HttpContext.RequireContext();
        return Ok(await _lms.ListStudentsAsync(locationId, userId, role, ct));
    }

    [HttpPost("api/students")]
    public async Task<IActionResult> CreateStudent([FromBody] UpsertStudentDto dto, CancellationToken ct)
    {
        var (userId, locationId, _) = HttpContext.RequireContext();
        return Ok(await _lms.CreateStudentAsync(locationId, userId, dto, ct));
    }

    [HttpGet("api/subjects")]
    public async Task<IActionResult> Subjects(CancellationToken ct)
    {
        var (_, locationId, _) = HttpContext.RequireContext();
        return Ok(await _lms.ListSubjectsAsync(locationId, ct));
    }

    [HttpGet("api/search/tutors")]
    public async Task<IActionResult> Search([FromQuery] string? subject, [FromQuery] string? grade, [FromQuery] decimal? maxRate, CancellationToken ct)
    {
        var (_, locationId, _) = HttpContext.RequireContext();
        return Ok(await _lms.SearchTutorsAsync(locationId, subject, grade, maxRate, ct));
    }

    [HttpGet("api/tutors/{id:guid}")]
    public async Task<IActionResult> Tutor(Guid id, CancellationToken ct)
    {
        var (_, locationId, _) = HttpContext.RequireContext();
        var row = await _lms.GetTutorAsync(locationId, id, ct);
        return row is null ? NotFound() : Ok(row);
    }

    [HttpGet("api/tutors/me")]
    public async Task<IActionResult> MyTutor(CancellationToken ct)
    {
        var (userId, locationId, _) = HttpContext.RequireContext();
        return Ok(await _lms.GetMyTutorProfileAsync(locationId, userId, ct));
    }

    [HttpPut("api/tutors/me")]
    public async Task<IActionResult> SaveTutor([FromBody] UpsertTutorProfileDto dto, CancellationToken ct)
    {
        var (userId, locationId, _) = HttpContext.RequireContext();
        return Ok(await _lms.UpsertMyTutorProfileAsync(locationId, userId, dto, ct));
    }

    [HttpPost("api/tutors/me/availability")]
    public async Task<IActionResult> AddAvail([FromBody] UpsertAvailabilityDto dto, CancellationToken ct)
    {
        var (userId, locationId, _) = HttpContext.RequireContext();
        return Ok(await _lms.AddAvailabilityAsync(locationId, userId, dto, ct));
    }

    [HttpDelete("api/tutors/me/availability/{id:guid}")]
    public async Task<IActionResult> DelAvail(Guid id, CancellationToken ct)
    {
        var (userId, locationId, _) = HttpContext.RequireContext();
        await _lms.DeleteAvailabilityAsync(locationId, userId, id, ct);
        return Ok();
    }

    [HttpGet("api/bookings")]
    public async Task<IActionResult> Bookings(CancellationToken ct)
    {
        var (userId, locationId, role) = HttpContext.RequireContext();
        return Ok(await _lms.ListBookingsAsync(locationId, userId, role, ct));
    }

    [HttpPost("api/bookings")]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto, CancellationToken ct)
    {
        try
        {
            var (userId, locationId, _) = HttpContext.RequireContext();
            return Ok(await _lms.CreateBookingAsync(locationId, userId, dto, ct));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("api/bookings/{id:guid}/{status}")]
    public async Task<IActionResult> SetStatus(Guid id, string status, CancellationToken ct)
    {
        var (userId, locationId, role) = HttpContext.RequireContext();
        return Ok(await _lms.SetBookingStatusAsync(locationId, userId, role, id, status, ct));
    }

    [HttpPost("api/bookings/{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteLessonDto dto, CancellationToken ct)
    {
        var (userId, locationId, _) = HttpContext.RequireContext();
        return Ok(await _lms.CompleteLessonAsync(locationId, userId, id, dto, ct));
    }

    [HttpGet("api/courses")]
    public async Task<IActionResult> Courses(CancellationToken ct)
    {
        var (userId, locationId, role) = HttpContext.RequireContext();
        return Ok(await _lms.ListCoursesAsync(locationId, userId, role, ct));
    }

    [HttpPost("api/courses")]
    public async Task<IActionResult> CreateCourse([FromBody] UpsertCourseDto dto, CancellationToken ct)
    {
        var (userId, locationId, role) = HttpContext.RequireContext();
        return Ok(await _lms.CreateCourseAsync(locationId, userId, role, dto, ct));
    }

    [HttpGet("api/materials")]
    public async Task<IActionResult> Materials([FromQuery] Guid? courseId, CancellationToken ct)
    {
        var (_, locationId, _) = HttpContext.RequireContext();
        var rows = await _lms.ListMaterialsAsync(locationId, courseId, ct);
        foreach (var row in rows)
        {
            if (!string.IsNullOrWhiteSpace(row.StoragePath))
            {
                try { row.SignedUrl = await _files.SignAsync(row.StoragePath, ct); } catch { /* optional */ }
            }
        }

        return Ok(rows);
    }

    [HttpPost("api/materials")]
    public async Task<IActionResult> AddMaterial([FromBody] MaterialDto dto, CancellationToken ct)
    {
        var (_, locationId, _) = HttpContext.RequireContext();
        return Ok(await _lms.AddMaterialAsync(locationId, dto.CourseId, dto.Title, dto.ExternalUrl, dto.StoragePath, ct));
    }

    [HttpGet("api/assignments")]
    public async Task<IActionResult> Assignments(CancellationToken ct)
    {
        var (userId, locationId, role) = HttpContext.RequireContext();
        return Ok(await _lms.ListAssignmentsAsync(locationId, userId, role, ct));
    }

    [HttpPost("api/assignments")]
    public async Task<IActionResult> CreateAssignment([FromBody] UpsertAssignmentDto dto, CancellationToken ct)
    {
        var (_, locationId, _) = HttpContext.RequireContext();
        return Ok(await _lms.CreateAssignmentAsync(locationId, dto, ct));
    }

    [HttpPost("api/assignments/{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitAssignmentDto dto, CancellationToken ct)
    {
        var (_, locationId, _) = HttpContext.RequireContext();
        return Ok(await _lms.SubmitAssignmentAsync(locationId, id, dto, ct));
    }

    [HttpPost("api/submissions/{id:guid}/grade")]
    public async Task<IActionResult> Grade(Guid id, [FromBody] GradeDto dto, CancellationToken ct)
    {
        var (_, locationId, _) = HttpContext.RequireContext();
        return Ok(await _lms.GradeSubmissionAsync(locationId, id, dto, ct));
    }

    [HttpGet("api/progress")]
    public async Task<IActionResult> Progress(CancellationToken ct)
    {
        var (userId, locationId, role) = HttpContext.RequireContext();
        return Ok(await _lms.ListProgressAsync(locationId, userId, role, ct));
    }

    [HttpGet("api/admin/users")]
    public async Task<IActionResult> Users(CancellationToken ct)
    {
        var (_, locationId, role) = HttpContext.RequireContext();
        if (role is not ("owner" or "admin")) return Forbid();
        return Ok(await _lms.ListUsersAsync(locationId, ct));
    }

    [HttpPost("api/files")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string prefix = "materials", CancellationToken ct = default)
    {
        if (file is null || file.Length == 0) return BadRequest(new { message = "File is required." });
        var allowed = new[] { "application/pdf", "image/jpeg", "image/png", "image/webp", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };
        if (!allowed.Contains(file.ContentType) && !file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Allowed: pdf, images, doc/docx." });
        }

        try
        {
            await using var stream = file.OpenReadStream();
            return Ok(await _files.UploadAsync(prefix, file.FileName, file.ContentType, stream, ct));
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
    }
}
