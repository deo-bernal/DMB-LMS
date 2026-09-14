using Dmb.Lms.Data.Context;
using Dmb.Lms.Data.Entities;
using Dmb.Lms.Data.Repository.Interface;
using Dmb.Lms.Model;
using Dmb.Lms.Model.Dtos;
using Dmb.Lms.Model.Dtos.Auth;
using Microsoft.EntityFrameworkCore;

namespace Dmb.Lms.Data.Repository.Implementation;

public class LmsRepository : ILmsRepository
{
    private readonly LmsContext _db;

    public LmsRepository(LmsContext db) => _db = db;

    public async Task<LocationMembershipDto?> GetMembershipAsync(Guid userId, Guid locationId, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        var membership = await _db.UserLocations.AsNoTracking()
            .Where(ul => ul.UserId == userId && ul.LocationId == locationId && ul.Location.IsActive)
            .Select(ul => new LocationMembershipDto { LocationId = ul.LocationId, Name = ul.Location.Name, Role = ul.Role })
            .FirstOrDefaultAsync(cancellationToken);

        if (membership is not null) return membership;
        if (user?.IsSuperAdmin == true)
        {
            var loc = await _db.Locations.AsNoTracking().FirstOrDefaultAsync(l => l.Id == locationId && l.AgencyId == user.AgencyId, cancellationToken);
            if (loc is not null)
            {
                return new LocationMembershipDto { LocationId = loc.Id, Name = loc.Name, Role = Roles.Owner };
            }
        }

        return null;
    }

    public async Task<LocationStatsDto> GetStatsAsync(Guid locationId, CancellationToken cancellationToken = default)
    {
        return new LocationStatsDto
        {
            ParentCount = await _db.UserLocations.CountAsync(ul => ul.LocationId == locationId && ul.Role == Roles.Parent, cancellationToken),
            TutorCount = await _db.TutorProfiles.CountAsync(t => t.LocationId == locationId, cancellationToken),
            StudentCount = await _db.Students.CountAsync(s => s.LocationId == locationId, cancellationToken),
            UpcomingLessons = await _db.Bookings.CountAsync(b => b.LocationId == locationId && b.Status == "accepted" && b.StartsAt > DateTimeOffset.UtcNow, cancellationToken),
            OpenRequests = await _db.Bookings.CountAsync(b => b.LocationId == locationId && b.Status == "requested", cancellationToken)
        };
    }

    public async Task<IReadOnlyList<StudentDto>> ListStudentsAsync(Guid locationId, Guid userId, string role, CancellationToken cancellationToken = default)
    {
        var q = _db.Students.AsNoTracking().Where(s => s.LocationId == locationId);
        if (role == Roles.Parent) q = q.Where(s => s.ParentUserId == userId);
        if (role == Roles.Tutor)
        {
            var tutorId = await TutorId(locationId, userId, cancellationToken);
            q = q.Where(s => _db.Bookings.Any(b => b.StudentId == s.Id && b.TutorProfileId == tutorId)
                || _db.Enrollments.Any(e => e.StudentId == s.Id && e.Course.TutorProfileId == tutorId));
        }

        return await q.OrderBy(s => s.FirstName).Select(s => MapStudent(s)).ToListAsync(cancellationToken);
    }

    public async Task<StudentDto> CreateStudentAsync(Guid locationId, Guid parentUserId, UpsertStudentDto dto, CancellationToken cancellationToken = default)
    {
        var row = new Student
        {
            Id = Guid.NewGuid(),
            LocationId = locationId,
            ParentUserId = parentUserId,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            GradeLevel = dto.GradeLevel.Trim(),
            Notes = dto.Notes,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _db.Students.Add(row);
        await _db.SaveChangesAsync(cancellationToken);
        return MapStudent(row);
    }

    public Task<IReadOnlyList<SubjectDto>> ListSubjectsAsync(Guid locationId, CancellationToken cancellationToken = default)
        => _db.Subjects.AsNoTracking().Where(s => s.LocationId == locationId).OrderBy(s => s.Name)
            .Select(s => new SubjectDto { Id = s.Id, Name = s.Name }).ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<SubjectDto>)t.Result, cancellationToken);

    public async Task<IReadOnlyList<TutorCardDto>> SearchTutorsAsync(Guid locationId, string? subject, string? grade, decimal? maxRate, CancellationToken cancellationToken = default)
    {
        var q = _db.TutorProfiles.AsNoTracking().Include(t => t.User).Include(t => t.TutorSubjects).ThenInclude(ts => ts.Subject)
            .Where(t => t.LocationId == locationId);
        if (maxRate is not null) q = q.Where(t => t.HourlyRate <= maxRate);
        var list = await q.ToListAsync(cancellationToken);
        return list.Select(t => ToCard(t, subject, grade)).Where(t => t.Score >= 0).OrderByDescending(t => t.Score).ToList();
    }

    public async Task<TutorProfileDto?> GetTutorAsync(Guid locationId, Guid tutorProfileId, CancellationToken cancellationToken = default)
    {
        var t = await _db.TutorProfiles.AsNoTracking().Include(x => x.User).Include(x => x.TutorSubjects).ThenInclude(ts => ts.Subject)
            .Include(x => x.Availability)
            .FirstOrDefaultAsync(x => x.LocationId == locationId && x.Id == tutorProfileId, cancellationToken);
        return t is null ? null : ToProfile(t);
    }

    public async Task<TutorProfileDto?> GetMyTutorProfileAsync(Guid locationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var t = await _db.TutorProfiles.AsNoTracking().Include(x => x.User).Include(x => x.TutorSubjects).ThenInclude(ts => ts.Subject)
            .Include(x => x.Availability)
            .FirstOrDefaultAsync(x => x.LocationId == locationId && x.UserId == userId, cancellationToken);
        return t is null ? null : ToProfile(t);
    }

    public async Task<TutorProfileDto> UpsertMyTutorProfileAsync(Guid locationId, Guid userId, UpsertTutorProfileDto dto, CancellationToken cancellationToken = default)
    {
        var t = await _db.TutorProfiles.Include(x => x.TutorSubjects).FirstOrDefaultAsync(x => x.LocationId == locationId && x.UserId == userId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        if (t is null)
        {
            t = new TutorProfile { Id = Guid.NewGuid(), LocationId = locationId, UserId = userId, CreatedAt = now };
            _db.TutorProfiles.Add(t);
        }

        t.Headline = dto.Headline;
        t.Bio = dto.Bio;
        t.HourlyRate = dto.HourlyRate;
        t.ExperienceYears = dto.ExperienceYears;
        t.OnlineOnly = dto.OnlineOnly;
        t.UpdatedAt = now;
        t.TutorSubjects.Clear();
        foreach (var sid in dto.SubjectIds.Distinct())
        {
            t.TutorSubjects.Add(new TutorSubject { TutorProfileId = t.Id, SubjectId = sid });
        }

        await _db.SaveChangesAsync(cancellationToken);
        return (await GetMyTutorProfileAsync(locationId, userId, cancellationToken))!;
    }

    public async Task<AvailabilityDto> AddAvailabilityAsync(Guid locationId, Guid userId, UpsertAvailabilityDto dto, CancellationToken cancellationToken = default)
    {
        var tutorId = await TutorId(locationId, userId, cancellationToken) ?? throw new InvalidOperationException("Tutor profile not found.");
        var row = new TutorAvailability
        {
            Id = Guid.NewGuid(),
            TutorProfileId = tutorId,
            Weekday = dto.Weekday,
            StartTime = TimeOnly.Parse(dto.StartTime),
            EndTime = TimeOnly.Parse(dto.EndTime)
        };
        _db.TutorAvailabilities.Add(row);
        await _db.SaveChangesAsync(cancellationToken);
        return new AvailabilityDto { Id = row.Id, Weekday = row.Weekday, StartTime = row.StartTime.ToString("HH:mm"), EndTime = row.EndTime.ToString("HH:mm") };
    }

    public async Task DeleteAvailabilityAsync(Guid locationId, Guid userId, Guid availabilityId, CancellationToken cancellationToken = default)
    {
        var tutorId = await TutorId(locationId, userId, cancellationToken);
        var row = await _db.TutorAvailabilities.FirstOrDefaultAsync(a => a.Id == availabilityId && a.TutorProfileId == tutorId, cancellationToken);
        if (row is null) return;
        _db.TutorAvailabilities.Remove(row);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BookingDto>> ListBookingsAsync(Guid locationId, Guid userId, string role, CancellationToken cancellationToken = default)
    {
        var q = _db.Bookings.AsNoTracking()
            .Include(b => b.Student).Include(b => b.TutorProfile).ThenInclude(t => t.User)
            .Include(b => b.Subject).Include(b => b.Notes).Include(b => b.Attendance)
            .Where(b => b.LocationId == locationId);
        if (role == Roles.Parent) q = q.Where(b => b.ParentUserId == userId);
        if (role == Roles.Tutor)
        {
            var tutorId = await TutorId(locationId, userId, cancellationToken);
            q = q.Where(b => b.TutorProfileId == tutorId);
        }

        var list = await q.OrderByDescending(b => b.StartsAt).ToListAsync(cancellationToken);
        return list.Select(MapBooking).ToList();
    }

    public async Task<BookingDto> CreateBookingAsync(Guid locationId, Guid parentUserId, CreateBookingDto dto, CancellationToken cancellationToken = default)
    {
        var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == dto.StudentId && s.LocationId == locationId && s.ParentUserId == parentUserId, cancellationToken)
            ?? throw new InvalidOperationException("Student not found for this parent.");
        var tutor = await _db.TutorProfiles.FirstOrDefaultAsync(t => t.Id == dto.TutorProfileId && t.LocationId == locationId, cancellationToken)
            ?? throw new InvalidOperationException("Tutor not found.");
        if (await _db.Bookings.AnyAsync(b => b.TutorProfileId == tutor.Id && b.StartsAt == dto.StartsAt && b.Status != "cancelled" && b.Status != "rejected", cancellationToken))
        {
            throw new InvalidOperationException("That slot is already booked.");
        }

        var row = new Booking
        {
            Id = Guid.NewGuid(),
            LocationId = locationId,
            ParentUserId = parentUserId,
            StudentId = student.Id,
            TutorProfileId = tutor.Id,
            SubjectId = dto.SubjectId,
            StartsAt = dto.StartsAt,
            EndsAt = dto.EndsAt,
            Status = "requested",
            Price = tutor.HourlyRate,
            MeetingUrl = dto.MeetingUrl,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _db.Bookings.Add(row);
        await _db.SaveChangesAsync(cancellationToken);
        return (await ListBookingsAsync(locationId, parentUserId, Roles.Parent, cancellationToken)).First(b => b.Id == row.Id);
    }

    public async Task<BookingDto> SetBookingStatusAsync(Guid locationId, Guid userId, string role, Guid bookingId, string status, CancellationToken cancellationToken = default)
    {
        var row = await _db.Bookings.Include(b => b.Student).Include(b => b.TutorProfile).ThenInclude(t => t.User)
            .Include(b => b.Subject).Include(b => b.Notes).Include(b => b.Attendance)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.LocationId == locationId, cancellationToken)
            ?? throw new InvalidOperationException("Booking not found.");

        if (role == Roles.Tutor && row.TutorProfile.UserId != userId) throw new InvalidOperationException("Not your booking.");
        if (role == Roles.Parent && row.ParentUserId != userId) throw new InvalidOperationException("Not your booking.");
        row.Status = status;
        row.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return MapBooking(row);
    }

    public async Task<BookingDto> CompleteLessonAsync(Guid locationId, Guid userId, Guid bookingId, CompleteLessonDto dto, CancellationToken cancellationToken = default)
    {
        var row = await _db.Bookings.Include(b => b.Student).Include(b => b.TutorProfile).ThenInclude(t => t.User)
            .Include(b => b.Subject).Include(b => b.Notes).Include(b => b.Attendance)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.LocationId == locationId, cancellationToken)
            ?? throw new InvalidOperationException("Booking not found.");

        row.Status = "completed";
        row.UpdatedAt = DateTimeOffset.UtcNow;
        if (!string.IsNullOrWhiteSpace(dto.MeetingUrl)) row.MeetingUrl = dto.MeetingUrl;
        if (!string.IsNullOrWhiteSpace(dto.Note))
        {
            _db.LessonNotes.Add(new LessonNote
            {
                Id = Guid.NewGuid(),
                BookingId = row.Id,
                AuthorUserId = userId,
                Body = dto.Note,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        if (row.Attendance is null)
        {
            _db.Attendances.Add(new Attendance
            {
                Id = Guid.NewGuid(),
                BookingId = row.Id,
                StudentId = row.StudentId,
                Present = dto.Present,
                MarkedAt = DateTimeOffset.UtcNow
            });
        }
        else
        {
            row.Attendance.Present = dto.Present;
            row.Attendance.MarkedAt = DateTimeOffset.UtcNow;
        }

        var enrollment = await _db.Enrollments.FirstOrDefaultAsync(e => e.StudentId == row.StudentId && e.Course.TutorProfileId == row.TutorProfileId, cancellationToken);
        if (enrollment is not null)
        {
            var progress = await _db.ProgressRows.FirstOrDefaultAsync(p => p.StudentId == row.StudentId && p.CourseId == enrollment.CourseId, cancellationToken);
            if (progress is null)
            {
                _db.ProgressRows.Add(new Progress
                {
                    Id = Guid.NewGuid(),
                    StudentId = row.StudentId,
                    CourseId = enrollment.CourseId,
                    LessonsAttended = dto.Present ? 1 : 0,
                    UpdatedAt = DateTimeOffset.UtcNow
                });
            }
            else if (dto.Present)
            {
                progress.LessonsAttended += 1;
                progress.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return (await ListBookingsAsync(locationId, userId, Roles.Tutor, cancellationToken)).First(b => b.Id == row.Id);
    }

    public async Task<IReadOnlyList<CourseDto>> ListCoursesAsync(Guid locationId, Guid userId, string role, CancellationToken cancellationToken = default)
    {
        var q = _db.Courses.AsNoTracking().Include(c => c.TutorProfile).ThenInclude(t => t.User).Include(c => c.Enrollments).ThenInclude(e => e.Student)
            .Where(c => c.LocationId == locationId);
        if (role == Roles.Tutor)
        {
            var tutorId = await TutorId(locationId, userId, cancellationToken);
            q = q.Where(c => c.TutorProfileId == tutorId);
        }
        if (role == Roles.Parent)
        {
            q = q.Where(c => c.Enrollments.Any(e => e.Student.ParentUserId == userId));
        }

        var list = await q.OrderBy(c => c.Title).ToListAsync(cancellationToken);
        return list.Select(c => new CourseDto
        {
            Id = c.Id,
            TutorProfileId = c.TutorProfileId,
            TutorName = $"{c.TutorProfile.User.FirstName} {c.TutorProfile.User.LastName}".Trim(),
            SubjectId = c.SubjectId,
            Title = c.Title,
            Description = c.Description,
            EnrolledStudents = c.Enrollments.Select(e => $"{e.Student.FirstName} {e.Student.LastName}".Trim()).ToList()
        }).ToList();
    }

    public async Task<CourseDto> CreateCourseAsync(Guid locationId, Guid userId, string role, UpsertCourseDto dto, CancellationToken cancellationToken = default)
    {
        var tutorId = dto.TutorProfileId ?? await TutorId(locationId, userId, cancellationToken)
            ?? throw new InvalidOperationException("Tutor profile required.");
        var row = new Course
        {
            Id = Guid.NewGuid(),
            LocationId = locationId,
            TutorProfileId = tutorId,
            SubjectId = dto.SubjectId,
            Title = dto.Title.Trim(),
            Description = dto.Description,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _db.Courses.Add(row);
        await _db.SaveChangesAsync(cancellationToken);
        return (await ListCoursesAsync(locationId, userId, role, cancellationToken)).First(c => c.Id == row.Id);
    }

    public async Task<IReadOnlyList<MaterialDto>> ListMaterialsAsync(Guid locationId, Guid? courseId, CancellationToken cancellationToken = default)
    {
        var q = _db.Materials.AsNoTracking().Where(m => _db.Courses.Any(c => c.Id == m.CourseId && c.LocationId == locationId));
        if (courseId is not null) q = q.Where(m => m.CourseId == courseId);
        return await q.OrderBy(m => m.Title).Select(m => new MaterialDto
        {
            Id = m.Id,
            CourseId = m.CourseId,
            Title = m.Title,
            ExternalUrl = m.ExternalUrl,
            StoragePath = m.StoragePath
        }).ToListAsync(cancellationToken);
    }

    public async Task<MaterialDto> AddMaterialAsync(Guid locationId, Guid courseId, string title, string? externalUrl, string? storagePath, CancellationToken cancellationToken = default)
    {
        if (!await _db.Courses.AnyAsync(c => c.Id == courseId && c.LocationId == locationId, cancellationToken))
        {
            throw new InvalidOperationException("Course not found.");
        }

        var row = new Material
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Title = title,
            ExternalUrl = externalUrl,
            StoragePath = storagePath,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.Materials.Add(row);
        await _db.SaveChangesAsync(cancellationToken);
        return new MaterialDto { Id = row.Id, CourseId = row.CourseId, Title = row.Title, ExternalUrl = row.ExternalUrl, StoragePath = row.StoragePath };
    }

    public async Task<IReadOnlyList<AssignmentDto>> ListAssignmentsAsync(Guid locationId, Guid userId, string role, CancellationToken cancellationToken = default)
    {
        var q = _db.Assignments.AsNoTracking().Include(a => a.Course).Include(a => a.Submissions).ThenInclude(s => s.Grade)
            .Include(a => a.Submissions).ThenInclude(s => s.Student)
            .Where(a => a.Course.LocationId == locationId);
        if (role == Roles.Parent)
        {
            q = q.Where(a => a.Course.Enrollments.Any(e => e.Student.ParentUserId == userId));
        }
        if (role == Roles.Tutor)
        {
            var tutorId = await TutorId(locationId, userId, cancellationToken);
            q = q.Where(a => a.Course.TutorProfileId == tutorId);
        }

        var list = await q.OrderBy(a => a.DueAt).ToListAsync(cancellationToken);
        return list.SelectMany(a =>
        {
            if (a.Submissions.Count == 0)
            {
                return new[]
                {
                    new AssignmentDto
                    {
                        Id = a.Id, CourseId = a.CourseId, CourseTitle = a.Course.Title, Title = a.Title,
                        Instructions = a.Instructions, DueAt = a.DueAt, MaxScore = a.MaxScore
                    }
                };
            }

            return a.Submissions.Select(s => new AssignmentDto
            {
                Id = a.Id,
                CourseId = a.CourseId,
                CourseTitle = a.Course.Title,
                Title = a.Title,
                Instructions = a.Instructions,
                DueAt = a.DueAt,
                MaxScore = a.MaxScore,
                SubmissionId = s.Id,
                SubmissionText = s.BodyText,
                Score = s.Grade?.Score,
                Feedback = s.Grade?.Feedback,
                StudentId = s.StudentId,
                StudentName = $"{s.Student.FirstName} {s.Student.LastName}".Trim()
            });
        }).ToList();
    }

    public async Task<AssignmentDto> CreateAssignmentAsync(Guid locationId, UpsertAssignmentDto dto, CancellationToken cancellationToken = default)
    {
        if (!await _db.Courses.AnyAsync(c => c.Id == dto.CourseId && c.LocationId == locationId, cancellationToken))
        {
            throw new InvalidOperationException("Course not found.");
        }

        var row = new Assignment
        {
            Id = Guid.NewGuid(),
            CourseId = dto.CourseId,
            Title = dto.Title,
            Instructions = dto.Instructions,
            DueAt = dto.DueAt,
            MaxScore = dto.MaxScore,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.Assignments.Add(row);
        await _db.SaveChangesAsync(cancellationToken);
        var course = await _db.Courses.AsNoTracking().FirstAsync(c => c.Id == dto.CourseId, cancellationToken);
        return new AssignmentDto { Id = row.Id, CourseId = row.CourseId, CourseTitle = course.Title, Title = row.Title, Instructions = row.Instructions, DueAt = row.DueAt, MaxScore = row.MaxScore };
    }

    public async Task<AssignmentDto> SubmitAssignmentAsync(Guid locationId, Guid assignmentId, SubmitAssignmentDto dto, CancellationToken cancellationToken = default)
    {
        var assignment = await _db.Assignments.Include(a => a.Course).FirstOrDefaultAsync(a => a.Id == assignmentId && a.Course.LocationId == locationId, cancellationToken)
            ?? throw new InvalidOperationException("Assignment not found.");
        var existing = await _db.Submissions.FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == dto.StudentId, cancellationToken);
        if (existing is null)
        {
            existing = new Submission
            {
                Id = Guid.NewGuid(),
                AssignmentId = assignmentId,
                StudentId = dto.StudentId,
                SubmittedAt = DateTimeOffset.UtcNow
            };
            _db.Submissions.Add(existing);
        }

        existing.BodyText = dto.BodyText;
        existing.StoragePath = dto.StoragePath;
        existing.SubmittedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return new AssignmentDto
        {
            Id = assignment.Id, CourseId = assignment.CourseId, CourseTitle = assignment.Course.Title, Title = assignment.Title,
            Instructions = assignment.Instructions, DueAt = assignment.DueAt, MaxScore = assignment.MaxScore,
            SubmissionId = existing.Id, SubmissionText = existing.BodyText, StudentId = existing.StudentId
        };
    }

    public async Task<AssignmentDto> GradeSubmissionAsync(Guid locationId, Guid submissionId, GradeDto dto, CancellationToken cancellationToken = default)
    {
        var sub = await _db.Submissions.Include(s => s.Assignment).ThenInclude(a => a.Course).Include(s => s.Grade).Include(s => s.Student)
            .FirstOrDefaultAsync(s => s.Id == submissionId && s.Assignment.Course.LocationId == locationId, cancellationToken)
            ?? throw new InvalidOperationException("Submission not found.");
        if (sub.Grade is null)
        {
            sub.Grade = new Grade { Id = Guid.NewGuid(), SubmissionId = sub.Id, GradedAt = DateTimeOffset.UtcNow };
            _db.Grades.Add(sub.Grade);
        }

        sub.Grade.Score = dto.Score;
        sub.Grade.Feedback = dto.Feedback;
        sub.Grade.GradedAt = DateTimeOffset.UtcNow;

        var progress = await _db.ProgressRows.FirstOrDefaultAsync(p => p.StudentId == sub.StudentId && p.CourseId == sub.Assignment.CourseId, cancellationToken);
        if (progress is null)
        {
            _db.ProgressRows.Add(new Progress
            {
                Id = Guid.NewGuid(),
                StudentId = sub.StudentId,
                CourseId = sub.Assignment.CourseId,
                AssignmentsGraded = 1,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }
        else
        {
            progress.AssignmentsGraded += 1;
            progress.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return new AssignmentDto
        {
            Id = sub.AssignmentId, CourseId = sub.Assignment.CourseId, CourseTitle = sub.Assignment.Course.Title,
            Title = sub.Assignment.Title, Instructions = sub.Assignment.Instructions, DueAt = sub.Assignment.DueAt,
            MaxScore = sub.Assignment.MaxScore, SubmissionId = sub.Id, SubmissionText = sub.BodyText,
            Score = sub.Grade.Score, Feedback = sub.Grade.Feedback, StudentId = sub.StudentId,
            StudentName = $"{sub.Student.FirstName} {sub.Student.LastName}".Trim()
        };
    }

    public async Task<IReadOnlyList<ProgressDto>> ListProgressAsync(Guid locationId, Guid userId, string role, CancellationToken cancellationToken = default)
    {
        var q = _db.ProgressRows.AsNoTracking().Include(p => p.Student).Include(p => p.Course)
            .Where(p => p.Course.LocationId == locationId);
        if (role == Roles.Parent) q = q.Where(p => p.Student.ParentUserId == userId);
        if (role == Roles.Tutor)
        {
            var tutorId = await TutorId(locationId, userId, cancellationToken);
            q = q.Where(p => p.Course.TutorProfileId == tutorId);
        }

        return await q.Select(p => new ProgressDto
        {
            StudentId = p.StudentId,
            StudentName = p.Student.FirstName + " " + p.Student.LastName,
            CourseId = p.CourseId,
            CourseTitle = p.Course.Title,
            MaterialsDone = p.MaterialsDone,
            AssignmentsGraded = p.AssignmentsGraded,
            LessonsAttended = p.LessonsAttended
        }).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdminUserDto>> ListUsersAsync(Guid locationId, CancellationToken cancellationToken = default)
    {
        return await _db.UserLocations.AsNoTracking().Where(ul => ul.LocationId == locationId)
            .Select(ul => new AdminUserDto
            {
                UserId = ul.UserId,
                Email = ul.User.Email,
                FirstName = ul.User.FirstName,
                LastName = ul.User.LastName,
                Role = ul.Role
            }).OrderBy(u => u.Role).ThenBy(u => u.Email).ToListAsync(cancellationToken);
    }

    private Task<Guid?> TutorId(Guid locationId, Guid userId, CancellationToken cancellationToken)
        => _db.TutorProfiles.Where(t => t.LocationId == locationId && t.UserId == userId).Select(t => (Guid?)t.Id).FirstOrDefaultAsync(cancellationToken);

    private static StudentDto MapStudent(Student s) => new()
    {
        Id = s.Id, ParentUserId = s.ParentUserId, FirstName = s.FirstName, LastName = s.LastName, GradeLevel = s.GradeLevel, Notes = s.Notes
    };

    private static TutorCardDto ToCard(TutorProfile t, string? subject, string? grade)
    {
        var subjects = string.Join(", ", t.TutorSubjects.Select(ts => ts.Subject.Name));
        var match = string.IsNullOrWhiteSpace(subject) || subjects.Contains(subject, StringComparison.OrdinalIgnoreCase);
        var gradeOk = string.IsNullOrWhiteSpace(grade) || t.TutorSubjects.Any(ts =>
            (string.IsNullOrEmpty(ts.LevelMin) || string.Compare(ts.LevelMin, grade, StringComparison.OrdinalIgnoreCase) <= 0)
            && (string.IsNullOrEmpty(ts.LevelMax) || string.Compare(ts.LevelMax, grade, StringComparison.OrdinalIgnoreCase) >= 0));
        var score = match && gradeOk ? t.Rating * 10 + t.ExperienceYears + (match ? 20 : 0) : -1;
        return new TutorCardDto
        {
            TutorProfileId = t.Id,
            UserId = t.UserId,
            FirstName = t.User.FirstName,
            LastName = t.User.LastName,
            Headline = t.Headline,
            Bio = t.Bio,
            HourlyRate = t.HourlyRate,
            Currency = t.Currency,
            ExperienceYears = t.ExperienceYears,
            OnlineOnly = t.OnlineOnly,
            Rating = t.Rating,
            ReviewCount = t.ReviewCount,
            AvatarPath = t.AvatarPath,
            Subjects = subjects,
            Score = score
        };
    }

    private static TutorProfileDto ToProfile(TutorProfile t)
    {
        var card = ToCard(t, null, null);
        return new TutorProfileDto
        {
            TutorProfileId = card.TutorProfileId,
            UserId = card.UserId,
            FirstName = card.FirstName,
            LastName = card.LastName,
            Headline = card.Headline,
            Bio = card.Bio,
            HourlyRate = card.HourlyRate,
            Currency = card.Currency,
            ExperienceYears = card.ExperienceYears,
            OnlineOnly = card.OnlineOnly,
            Rating = card.Rating,
            ReviewCount = card.ReviewCount,
            AvatarPath = card.AvatarPath,
            Subjects = card.Subjects,
            Score = card.Score,
            Availability = t.Availability.Select(a => new AvailabilityDto
            {
                Id = a.Id,
                Weekday = a.Weekday,
                StartTime = a.StartTime.ToString("HH:mm"),
                EndTime = a.EndTime.ToString("HH:mm")
            }).ToList()
        };
    }

    private static BookingDto MapBooking(Booking b) => new()
    {
        Id = b.Id,
        ParentUserId = b.ParentUserId,
        StudentId = b.StudentId,
        StudentName = $"{b.Student.FirstName} {b.Student.LastName}".Trim(),
        TutorProfileId = b.TutorProfileId,
        TutorName = $"{b.TutorProfile.User.FirstName} {b.TutorProfile.User.LastName}".Trim(),
        SubjectId = b.SubjectId,
        SubjectName = b.Subject?.Name ?? "",
        StartsAt = b.StartsAt,
        EndsAt = b.EndsAt,
        Status = b.Status,
        Price = b.Price,
        MeetingUrl = b.MeetingUrl,
        Note = b.Notes.OrderByDescending(n => n.CreatedAt).FirstOrDefault()?.Body,
        Present = b.Attendance?.Present
    };
}
