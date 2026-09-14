namespace Dmb.Lms.Model.Dtos;

public class LocationStatsDto
{
    public int ParentCount { get; set; }
    public int TutorCount { get; set; }
    public int StudentCount { get; set; }
    public int UpcomingLessons { get; set; }
    public int OpenRequests { get; set; }
}

public class StudentDto
{
    public Guid Id { get; set; }
    public Guid ParentUserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string GradeLevel { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class UpsertStudentDto
{
    public required string FirstName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string GradeLevel { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class SubjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class TutorCardDto
{
    public Guid TutorProfileId { get; set; }
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public string Currency { get; set; } = "PHP";
    public int ExperienceYears { get; set; }
    public bool OnlineOnly { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public string? AvatarPath { get; set; }
    public string Subjects { get; set; } = string.Empty;
    public decimal Score { get; set; }
}

public class TutorProfileDto : TutorCardDto
{
    public IReadOnlyList<AvailabilityDto> Availability { get; set; } = [];
}

public class UpsertTutorProfileDto
{
    public string Headline { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public int ExperienceYears { get; set; }
    public bool OnlineOnly { get; set; } = true;
    public IReadOnlyList<Guid> SubjectIds { get; set; } = [];
}

public class AvailabilityDto
{
    public Guid Id { get; set; }
    public int Weekday { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
}

public class UpsertAvailabilityDto
{
    public int Weekday { get; set; }
    public required string StartTime { get; set; }
    public required string EndTime { get; set; }
}

public class BookingDto
{
    public Guid Id { get; set; }
    public Guid ParentUserId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public Guid TutorProfileId { get; set; }
    public string TutorName { get; set; } = string.Empty;
    public Guid? SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? MeetingUrl { get; set; }
    public string? Note { get; set; }
    public bool? Present { get; set; }
}

public class CreateBookingDto
{
    public required Guid StudentId { get; set; }
    public required Guid TutorProfileId { get; set; }
    public Guid? SubjectId { get; set; }
    public required DateTimeOffset StartsAt { get; set; }
    public required DateTimeOffset EndsAt { get; set; }
    public string? MeetingUrl { get; set; }
}

public class CompleteLessonDto
{
    public bool Present { get; set; } = true;
    public string? Note { get; set; }
    public string? MeetingUrl { get; set; }
}

public class CourseDto
{
    public Guid Id { get; set; }
    public Guid TutorProfileId { get; set; }
    public string TutorName { get; set; } = string.Empty;
    public Guid? SubjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IReadOnlyList<string> EnrolledStudents { get; set; } = [];
}

public class UpsertCourseDto
{
    public required string Title { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? SubjectId { get; set; }
    public Guid? TutorProfileId { get; set; }
}

public class MaterialDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ExternalUrl { get; set; }
    public string? StoragePath { get; set; }
    public string? SignedUrl { get; set; }
}

public class AssignmentDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public DateTimeOffset? DueAt { get; set; }
    public decimal MaxScore { get; set; }
    public Guid? SubmissionId { get; set; }
    public string? SubmissionText { get; set; }
    public decimal? Score { get; set; }
    public string? Feedback { get; set; }
    public Guid? StudentId { get; set; }
    public string? StudentName { get; set; }
}

public class UpsertAssignmentDto
{
    public required Guid CourseId { get; set; }
    public required string Title { get; set; }
    public string Instructions { get; set; } = string.Empty;
    public DateTimeOffset? DueAt { get; set; }
    public decimal MaxScore { get; set; } = 20;
}

public class SubmitAssignmentDto
{
    public required Guid StudentId { get; set; }
    public string? BodyText { get; set; }
    public string? StoragePath { get; set; }
}

public class GradeDto
{
    public required decimal Score { get; set; }
    public string? Feedback { get; set; }
}

public class ProgressDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public int MaterialsDone { get; set; }
    public int AssignmentsGraded { get; set; }
    public int LessonsAttended { get; set; }
}

public class FileUploadResult
{
    public string StoragePath { get; set; } = string.Empty;
    public string? SignedUrl { get; set; }
}

public class AdminUserDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
