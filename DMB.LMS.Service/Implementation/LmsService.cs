using Dmb.Lms.Data.Repository.Interface;
using Dmb.Lms.Model.Dtos;
using Dmb.Lms.Model.Dtos.Auth;
using Dmb.Lms.Service.Interface;

namespace Dmb.Lms.Service.Implementation;

public class LmsService : ILmsService
{
    private readonly ILmsRepository _repo;
    public LmsService(ILmsRepository repo) => _repo = repo;

    public Task<LocationMembershipDto?> GetMembershipAsync(Guid userId, Guid locationId, CancellationToken cancellationToken = default)
        => _repo.GetMembershipAsync(userId, locationId, cancellationToken);
    public Task<LocationStatsDto> GetStatsAsync(Guid locationId, CancellationToken cancellationToken = default)
        => _repo.GetStatsAsync(locationId, cancellationToken);
    public Task<IReadOnlyList<StudentDto>> ListStudentsAsync(Guid locationId, Guid userId, string role, CancellationToken cancellationToken = default)
        => _repo.ListStudentsAsync(locationId, userId, role, cancellationToken);
    public Task<StudentDto> CreateStudentAsync(Guid locationId, Guid parentUserId, UpsertStudentDto dto, CancellationToken cancellationToken = default)
        => _repo.CreateStudentAsync(locationId, parentUserId, dto, cancellationToken);
    public Task<IReadOnlyList<SubjectDto>> ListSubjectsAsync(Guid locationId, CancellationToken cancellationToken = default)
        => _repo.ListSubjectsAsync(locationId, cancellationToken);
    public Task<IReadOnlyList<TutorCardDto>> SearchTutorsAsync(Guid locationId, string? subject, string? grade, decimal? maxRate, CancellationToken cancellationToken = default)
        => _repo.SearchTutorsAsync(locationId, subject, grade, maxRate, cancellationToken);
    public Task<TutorProfileDto?> GetTutorAsync(Guid locationId, Guid tutorProfileId, CancellationToken cancellationToken = default)
        => _repo.GetTutorAsync(locationId, tutorProfileId, cancellationToken);
    public Task<TutorProfileDto?> GetMyTutorProfileAsync(Guid locationId, Guid userId, CancellationToken cancellationToken = default)
        => _repo.GetMyTutorProfileAsync(locationId, userId, cancellationToken);
    public Task<TutorProfileDto> UpsertMyTutorProfileAsync(Guid locationId, Guid userId, UpsertTutorProfileDto dto, CancellationToken cancellationToken = default)
        => _repo.UpsertMyTutorProfileAsync(locationId, userId, dto, cancellationToken);
    public Task<AvailabilityDto> AddAvailabilityAsync(Guid locationId, Guid userId, UpsertAvailabilityDto dto, CancellationToken cancellationToken = default)
        => _repo.AddAvailabilityAsync(locationId, userId, dto, cancellationToken);
    public Task DeleteAvailabilityAsync(Guid locationId, Guid userId, Guid availabilityId, CancellationToken cancellationToken = default)
        => _repo.DeleteAvailabilityAsync(locationId, userId, availabilityId, cancellationToken);
    public Task<IReadOnlyList<BookingDto>> ListBookingsAsync(Guid locationId, Guid userId, string role, CancellationToken cancellationToken = default)
        => _repo.ListBookingsAsync(locationId, userId, role, cancellationToken);
    public Task<BookingDto> CreateBookingAsync(Guid locationId, Guid parentUserId, CreateBookingDto dto, CancellationToken cancellationToken = default)
        => _repo.CreateBookingAsync(locationId, parentUserId, dto, cancellationToken);
    public Task<BookingDto> SetBookingStatusAsync(Guid locationId, Guid userId, string role, Guid bookingId, string status, CancellationToken cancellationToken = default)
        => _repo.SetBookingStatusAsync(locationId, userId, role, bookingId, status, cancellationToken);
    public Task<BookingDto> CompleteLessonAsync(Guid locationId, Guid userId, Guid bookingId, CompleteLessonDto dto, CancellationToken cancellationToken = default)
        => _repo.CompleteLessonAsync(locationId, userId, bookingId, dto, cancellationToken);
    public Task<IReadOnlyList<CourseDto>> ListCoursesAsync(Guid locationId, Guid userId, string role, CancellationToken cancellationToken = default)
        => _repo.ListCoursesAsync(locationId, userId, role, cancellationToken);
    public Task<CourseDto> CreateCourseAsync(Guid locationId, Guid userId, string role, UpsertCourseDto dto, CancellationToken cancellationToken = default)
        => _repo.CreateCourseAsync(locationId, userId, role, dto, cancellationToken);
    public Task<IReadOnlyList<MaterialDto>> ListMaterialsAsync(Guid locationId, Guid? courseId, CancellationToken cancellationToken = default)
        => _repo.ListMaterialsAsync(locationId, courseId, cancellationToken);
    public Task<MaterialDto> AddMaterialAsync(Guid locationId, Guid courseId, string title, string? externalUrl, string? storagePath, CancellationToken cancellationToken = default)
        => _repo.AddMaterialAsync(locationId, courseId, title, externalUrl, storagePath, cancellationToken);
    public Task<IReadOnlyList<AssignmentDto>> ListAssignmentsAsync(Guid locationId, Guid userId, string role, CancellationToken cancellationToken = default)
        => _repo.ListAssignmentsAsync(locationId, userId, role, cancellationToken);
    public Task<AssignmentDto> CreateAssignmentAsync(Guid locationId, UpsertAssignmentDto dto, CancellationToken cancellationToken = default)
        => _repo.CreateAssignmentAsync(locationId, dto, cancellationToken);
    public Task<AssignmentDto> SubmitAssignmentAsync(Guid locationId, Guid assignmentId, SubmitAssignmentDto dto, CancellationToken cancellationToken = default)
        => _repo.SubmitAssignmentAsync(locationId, assignmentId, dto, cancellationToken);
    public Task<AssignmentDto> GradeSubmissionAsync(Guid locationId, Guid submissionId, GradeDto dto, CancellationToken cancellationToken = default)
        => _repo.GradeSubmissionAsync(locationId, submissionId, dto, cancellationToken);
    public Task<IReadOnlyList<ProgressDto>> ListProgressAsync(Guid locationId, Guid userId, string role, CancellationToken cancellationToken = default)
        => _repo.ListProgressAsync(locationId, userId, role, cancellationToken);
    public Task<IReadOnlyList<AdminUserDto>> ListUsersAsync(Guid locationId, CancellationToken cancellationToken = default)
        => _repo.ListUsersAsync(locationId, cancellationToken);
}
