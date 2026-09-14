namespace Dmb.Lms.Data.Entities;

public class Student
{
    public Guid Id { get; set; }
    public Guid LocationId { get; set; }
    public Guid ParentUserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string GradeLevel { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public LmsUser Parent { get; set; } = null!;
}

public class Subject
{
    public Guid Id { get; set; }
    public Guid LocationId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class TutorProfile
{
    public Guid Id { get; set; }
    public Guid LocationId { get; set; }
    public Guid UserId { get; set; }
    public string Headline { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public string Currency { get; set; } = "PHP";
    public int ExperienceYears { get; set; }
    public bool OnlineOnly { get; set; } = true;
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public string? AvatarPath { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public LmsUser User { get; set; } = null!;
    public ICollection<TutorSubject> TutorSubjects { get; set; } = [];
    public ICollection<TutorAvailability> Availability { get; set; } = [];
}

public class TutorSubject
{
    public Guid TutorProfileId { get; set; }
    public Guid SubjectId { get; set; }
    public string LevelMin { get; set; } = string.Empty;
    public string LevelMax { get; set; } = string.Empty;
    public TutorProfile TutorProfile { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
}

public class TutorAvailability
{
    public Guid Id { get; set; }
    public Guid TutorProfileId { get; set; }
    public int Weekday { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public TutorProfile TutorProfile { get; set; } = null!;
}

public class Booking
{
    public Guid Id { get; set; }
    public Guid LocationId { get; set; }
    public Guid ParentUserId { get; set; }
    public Guid StudentId { get; set; }
    public Guid TutorProfileId { get; set; }
    public Guid? SubjectId { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public string Status { get; set; } = "requested";
    public decimal Price { get; set; }
    public string? MeetingUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Student Student { get; set; } = null!;
    public TutorProfile TutorProfile { get; set; } = null!;
    public Subject? Subject { get; set; }
    public ICollection<LessonNote> Notes { get; set; } = [];
    public Attendance? Attendance { get; set; }
}

public class LessonNote
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid AuthorUserId { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public class Attendance
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid StudentId { get; set; }
    public bool Present { get; set; } = true;
    public DateTimeOffset MarkedAt { get; set; }
}

public class Course
{
    public Guid Id { get; set; }
    public Guid LocationId { get; set; }
    public Guid TutorProfileId { get; set; }
    public Guid? SubjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public TutorProfile TutorProfile { get; set; } = null!;
    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public ICollection<Material> Materials { get; set; } = [];
    public ICollection<Assignment> Assignments { get; set; } = [];
}

public class Enrollment
{
    public Guid CourseId { get; set; }
    public Guid StudentId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Course Course { get; set; } = null!;
    public Student Student { get; set; } = null!;
}

public class Material
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ExternalUrl { get; set; }
    public string? StoragePath { get; set; }
    public string? Mime { get; set; }
    public long? SizeBytes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class Assignment
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public DateTimeOffset? DueAt { get; set; }
    public decimal MaxScore { get; set; } = 20;
    public DateTimeOffset CreatedAt { get; set; }
    public Course Course { get; set; } = null!;
    public ICollection<Submission> Submissions { get; set; } = [];
}

public class Submission
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }
    public string? BodyText { get; set; }
    public string? StoragePath { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public Assignment Assignment { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public Grade? Grade { get; set; }
}

public class Grade
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public decimal Score { get; set; }
    public string? Feedback { get; set; }
    public DateTimeOffset GradedAt { get; set; }
}

public class Progress
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public int MaterialsDone { get; set; }
    public int AssignmentsGraded { get; set; }
    public int LessonsAttended { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
