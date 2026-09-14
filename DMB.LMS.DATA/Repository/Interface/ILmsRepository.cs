using Dmb.Lms.Model.Dtos;
using Dmb.Lms.Model.Dtos.Auth;

namespace Dmb.Lms.Data.Repository.Interface;

public interface ILmsRepository
{
    Task<LocationMembershipDto?> GetMembershipAsync(Guid userId, Guid locationId, CancellationToken cancellationToken = default);
    Task<LocationStatsDto> GetStatsAsync(Guid locationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentDto>> ListStudentsAsync(Guid locationId, Guid userId, string role, CancellationToken cancellationToken = default);
    Task<StudentDto> CreateStudentAsync(Guid locationId, Guid parentUserId, UpsertStudentDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubjectDto>> ListSubjectsAsync(Guid locationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TutorCardDto>> SearchTutorsAsync(Guid locationId, string? subject, string? grade, decimal? maxRate, CancellationToken cancellationToken = default);
    Task<TutorProfileDto?> GetTutorAsync(Guid locationId, Guid tutorProfileId, CancellationToken cancellationToken = default);
    Task<TutorProfileDto?> GetMyTutorProfileAsync(Guid locationId, Guid userId, CancellationToken cancellationToken = default);
    Task<TutorProfileDto> UpsertMyTutorProfileAsync(Guid locationId, Guid userId, UpsertTutorProfileDto dto, CancellationToken cancellationToken = default);
    Task<AvailabilityDto> AddAvailabilityAsync(Guid locationId, Guid userId, UpsertAvailabilityDto dto, CancellationToken cancellationToken = default);
    Task DeleteAvailabilityAsync(Guid locationId, Guid userId, Guid availabilityId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BookingDto>> ListBookingsAsync(Guid locationId, Guid userId, string role, CancellationToken cancellationToken = default);
    Task<BookingDto> CreateBookingAsync(Guid locationId, Guid parentUserId, CreateBookingDto dto, CancellationToken cancellationToken = default);
    Task<BookingDto> SetBookingStatusAsync(Guid locationId, Guid userId, string role, Guid bookingId, string status, CancellationToken cancellationToken = default);
    Task<BookingDto> CompleteLessonAsync(Guid locationId, Guid userId, Guid bookingId, CompleteLessonDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CourseDto>> ListCoursesAsync(Guid locationId, Guid userId, string role, CancellationToken cancellationToken = default);
    Task<CourseDto> CreateCourseAsync(Guid locationId, Guid userId, string role, UpsertCourseDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MaterialDto>> ListMaterialsAsync(Guid locationId, Guid? courseId, CancellationToken cancellationToken = default);
    Task<MaterialDto> AddMaterialAsync(Guid locationId, Guid courseId, string title, string? externalUrl, string? storagePath, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AssignmentDto>> ListAssignmentsAsync(Guid locationId, Guid userId, string role, CancellationToken cancellationToken = default);
    Task<AssignmentDto> CreateAssignmentAsync(Guid locationId, UpsertAssignmentDto dto, CancellationToken cancellationToken = default);
    Task<AssignmentDto> SubmitAssignmentAsync(Guid locationId, Guid assignmentId, SubmitAssignmentDto dto, CancellationToken cancellationToken = default);
    Task<AssignmentDto> GradeSubmissionAsync(Guid locationId, Guid submissionId, GradeDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProgressDto>> ListProgressAsync(Guid locationId, Guid userId, string role, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminUserDto>> ListUsersAsync(Guid locationId, CancellationToken cancellationToken = default);
}
