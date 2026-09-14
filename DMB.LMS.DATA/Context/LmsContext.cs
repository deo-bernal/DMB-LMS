using Dmb.Lms.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dmb.Lms.Data.Context;

public class LmsContext : DbContext
{
    public LmsContext(DbContextOptions<LmsContext> options) : base(options)
    {
    }

    public DbSet<Agency> Agencies => Set<Agency>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<LmsUser> Users => Set<LmsUser>();
    public DbSet<UserLocation> UserLocations => Set<UserLocation>();
    public DbSet<AccountActivationToken> AccountActivationTokens => Set<AccountActivationToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<RevokedToken> RevokedTokens => Set<RevokedToken>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<TutorProfile> TutorProfiles => Set<TutorProfile>();
    public DbSet<TutorSubject> TutorSubjects => Set<TutorSubject>();
    public DbSet<TutorAvailability> TutorAvailabilities => Set<TutorAvailability>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<LessonNote> LessonNotes => Set<LessonNote>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<Progress> ProgressRows => Set<Progress>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Agency>(e =>
        {
            e.ToTable("lms_agencies");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Name).HasColumnName("name");
            e.Property(x => x.Slug).HasColumnName("slug");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<Location>(e =>
        {
            e.ToTable("lms_locations");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.AgencyId).HasColumnName("agency_id");
            e.Property(x => x.Name).HasColumnName("name");
            e.Property(x => x.Timezone).HasColumnName("timezone");
            e.Property(x => x.IsActive).HasColumnName("is_active");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.HasOne(x => x.Agency).WithMany(x => x.Locations).HasForeignKey(x => x.AgencyId);
        });

        modelBuilder.Entity<LmsUser>(e =>
        {
            e.ToTable("lms_users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.AgencyId).HasColumnName("agency_id");
            e.Property(x => x.Username).HasColumnName("username");
            e.Property(x => x.Email).HasColumnName("email");
            e.Property(x => x.FirstName).HasColumnName("first_name");
            e.Property(x => x.LastName).HasColumnName("last_name");
            e.Property(x => x.PasswordHash).HasColumnName("password_hash");
            e.Property(x => x.PasswordSalt).HasColumnName("password_salt");
            e.Property(x => x.ContactNo).HasColumnName("contact_no");
            e.Property(x => x.Activated).HasColumnName("activated");
            e.Property(x => x.IsSuperAdmin).HasColumnName("is_super_admin");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.HasOne(x => x.Agency).WithMany(x => x.Users).HasForeignKey(x => x.AgencyId);
        });

        modelBuilder.Entity<UserLocation>(e =>
        {
            e.ToTable("lms_user_locations");
            e.HasKey(x => new { x.UserId, x.LocationId });
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.LocationId).HasColumnName("location_id");
            e.Property(x => x.Role).HasColumnName("role");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasOne(x => x.User).WithMany(x => x.UserLocations).HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Location).WithMany(x => x.UserLocations).HasForeignKey(x => x.LocationId);
        });

        modelBuilder.Entity<AccountActivationToken>(e =>
        {
            e.ToTable("lms_account_activation_tokens");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.TokenHash).HasColumnName("token_hash");
            e.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            e.Property(x => x.UsedAt).HasColumnName("used_at");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<PasswordResetToken>(e =>
        {
            e.ToTable("lms_password_reset_tokens");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.TokenHash).HasColumnName("token_hash");
            e.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            e.Property(x => x.UsedAt).HasColumnName("used_at");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<RevokedToken>(e =>
        {
            e.ToTable("lms_revoked_tokens");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Jti).HasColumnName("jti");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<Student>(e =>
        {
            e.ToTable("lms_students");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.LocationId).HasColumnName("location_id");
            e.Property(x => x.ParentUserId).HasColumnName("parent_user_id");
            e.Property(x => x.FirstName).HasColumnName("first_name");
            e.Property(x => x.LastName).HasColumnName("last_name");
            e.Property(x => x.GradeLevel).HasColumnName("grade_level");
            e.Property(x => x.Notes).HasColumnName("notes");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.HasOne(x => x.Parent).WithMany().HasForeignKey(x => x.ParentUserId);
        });

        modelBuilder.Entity<Subject>(e =>
        {
            e.ToTable("lms_subjects");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.LocationId).HasColumnName("location_id");
            e.Property(x => x.Name).HasColumnName("name");
        });

        modelBuilder.Entity<TutorProfile>(e =>
        {
            e.ToTable("lms_tutor_profiles");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.LocationId).HasColumnName("location_id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.Headline).HasColumnName("headline");
            e.Property(x => x.Bio).HasColumnName("bio");
            e.Property(x => x.HourlyRate).HasColumnName("hourly_rate").HasPrecision(10, 2);
            e.Property(x => x.Currency).HasColumnName("currency");
            e.Property(x => x.ExperienceYears).HasColumnName("experience_years");
            e.Property(x => x.OnlineOnly).HasColumnName("online_only");
            e.Property(x => x.Rating).HasColumnName("rating").HasPrecision(3, 2);
            e.Property(x => x.ReviewCount).HasColumnName("review_count");
            e.Property(x => x.AvatarPath).HasColumnName("avatar_path");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<TutorSubject>(e =>
        {
            e.ToTable("lms_tutor_subjects");
            e.HasKey(x => new { x.TutorProfileId, x.SubjectId });
            e.Property(x => x.TutorProfileId).HasColumnName("tutor_profile_id");
            e.Property(x => x.SubjectId).HasColumnName("subject_id");
            e.Property(x => x.LevelMin).HasColumnName("level_min");
            e.Property(x => x.LevelMax).HasColumnName("level_max");
            e.HasOne(x => x.TutorProfile).WithMany(x => x.TutorSubjects).HasForeignKey(x => x.TutorProfileId);
            e.HasOne(x => x.Subject).WithMany().HasForeignKey(x => x.SubjectId);
        });

        modelBuilder.Entity<TutorAvailability>(e =>
        {
            e.ToTable("lms_tutor_availability");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.TutorProfileId).HasColumnName("tutor_profile_id");
            e.Property(x => x.Weekday).HasColumnName("weekday");
            e.Property(x => x.StartTime).HasColumnName("start_time");
            e.Property(x => x.EndTime).HasColumnName("end_time");
            e.HasOne(x => x.TutorProfile).WithMany(x => x.Availability).HasForeignKey(x => x.TutorProfileId);
        });

        modelBuilder.Entity<Booking>(e =>
        {
            e.ToTable("lms_bookings");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.LocationId).HasColumnName("location_id");
            e.Property(x => x.ParentUserId).HasColumnName("parent_user_id");
            e.Property(x => x.StudentId).HasColumnName("student_id");
            e.Property(x => x.TutorProfileId).HasColumnName("tutor_profile_id");
            e.Property(x => x.SubjectId).HasColumnName("subject_id");
            e.Property(x => x.StartsAt).HasColumnName("starts_at");
            e.Property(x => x.EndsAt).HasColumnName("ends_at");
            e.Property(x => x.Status).HasColumnName("status");
            e.Property(x => x.Price).HasColumnName("price").HasPrecision(10, 2);
            e.Property(x => x.MeetingUrl).HasColumnName("meeting_url");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId);
            e.HasOne(x => x.TutorProfile).WithMany().HasForeignKey(x => x.TutorProfileId);
            e.HasOne(x => x.Subject).WithMany().HasForeignKey(x => x.SubjectId);
        });

        modelBuilder.Entity<LessonNote>(e =>
        {
            e.ToTable("lms_lesson_notes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.BookingId).HasColumnName("booking_id");
            e.Property(x => x.AuthorUserId).HasColumnName("author_user_id");
            e.Property(x => x.Body).HasColumnName("body");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasOne<Booking>().WithMany(x => x.Notes).HasForeignKey(x => x.BookingId);
        });

        modelBuilder.Entity<Attendance>(e =>
        {
            e.ToTable("lms_attendance");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.BookingId).HasColumnName("booking_id");
            e.Property(x => x.StudentId).HasColumnName("student_id");
            e.Property(x => x.Present).HasColumnName("present");
            e.Property(x => x.MarkedAt).HasColumnName("marked_at");
            e.HasOne<Booking>().WithOne(x => x.Attendance).HasForeignKey<Attendance>(x => x.BookingId);
        });

        modelBuilder.Entity<Course>(e =>
        {
            e.ToTable("lms_courses");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.LocationId).HasColumnName("location_id");
            e.Property(x => x.TutorProfileId).HasColumnName("tutor_profile_id");
            e.Property(x => x.SubjectId).HasColumnName("subject_id");
            e.Property(x => x.Title).HasColumnName("title");
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.HasOne(x => x.TutorProfile).WithMany().HasForeignKey(x => x.TutorProfileId);
        });

        modelBuilder.Entity<Enrollment>(e =>
        {
            e.ToTable("lms_enrollments");
            e.HasKey(x => new { x.CourseId, x.StudentId });
            e.Property(x => x.CourseId).HasColumnName("course_id");
            e.Property(x => x.StudentId).HasColumnName("student_id");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasOne(x => x.Course).WithMany(x => x.Enrollments).HasForeignKey(x => x.CourseId);
            e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId);
        });

        modelBuilder.Entity<Material>(e =>
        {
            e.ToTable("lms_materials");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.CourseId).HasColumnName("course_id");
            e.Property(x => x.Title).HasColumnName("title");
            e.Property(x => x.ExternalUrl).HasColumnName("external_url");
            e.Property(x => x.StoragePath).HasColumnName("storage_path");
            e.Property(x => x.Mime).HasColumnName("mime");
            e.Property(x => x.SizeBytes).HasColumnName("size_bytes");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasOne<Course>().WithMany(x => x.Materials).HasForeignKey(x => x.CourseId);
        });

        modelBuilder.Entity<Assignment>(e =>
        {
            e.ToTable("lms_assignments");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.CourseId).HasColumnName("course_id");
            e.Property(x => x.Title).HasColumnName("title");
            e.Property(x => x.Instructions).HasColumnName("instructions");
            e.Property(x => x.DueAt).HasColumnName("due_at");
            e.Property(x => x.MaxScore).HasColumnName("max_score").HasPrecision(6, 2);
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasOne(x => x.Course).WithMany(x => x.Assignments).HasForeignKey(x => x.CourseId);
        });

        modelBuilder.Entity<Submission>(e =>
        {
            e.ToTable("lms_submissions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.AssignmentId).HasColumnName("assignment_id");
            e.Property(x => x.StudentId).HasColumnName("student_id");
            e.Property(x => x.BodyText).HasColumnName("body_text");
            e.Property(x => x.StoragePath).HasColumnName("storage_path");
            e.Property(x => x.SubmittedAt).HasColumnName("submitted_at");
            e.HasOne(x => x.Assignment).WithMany(x => x.Submissions).HasForeignKey(x => x.AssignmentId);
            e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId);
        });

        modelBuilder.Entity<Grade>(e =>
        {
            e.ToTable("lms_grades");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.SubmissionId).HasColumnName("submission_id");
            e.Property(x => x.Score).HasColumnName("score").HasPrecision(6, 2);
            e.Property(x => x.Feedback).HasColumnName("feedback");
            e.Property(x => x.GradedAt).HasColumnName("graded_at");
            e.HasOne<Submission>().WithOne(x => x.Grade).HasForeignKey<Grade>(x => x.SubmissionId);
        });

        modelBuilder.Entity<Progress>(e =>
        {
            e.ToTable("lms_progress");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.StudentId).HasColumnName("student_id");
            e.Property(x => x.CourseId).HasColumnName("course_id");
            e.Property(x => x.MaterialsDone).HasColumnName("materials_done");
            e.Property(x => x.AssignmentsGraded).HasColumnName("assignments_graded");
            e.Property(x => x.LessonsAttended).HasColumnName("lessons_attended");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId);
            e.HasOne(x => x.Course).WithMany().HasForeignKey(x => x.CourseId);
        });
    }
}
