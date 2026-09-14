namespace Dmb.Lms.Data.Entities;

public class Agency
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ICollection<Location> Locations { get; set; } = [];
    public ICollection<LmsUser> Users { get; set; } = [];
}

public class Location
{
    public Guid Id { get; set; }
    public Guid AgencyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Timezone { get; set; } = "Asia/Manila";
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Agency Agency { get; set; } = null!;
    public ICollection<UserLocation> UserLocations { get; set; } = [];
}

public class LmsUser
{
    public Guid Id { get; set; }
    public Guid AgencyId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public string? ContactNo { get; set; }
    public bool Activated { get; set; }
    public bool IsSuperAdmin { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Agency Agency { get; set; } = null!;
    public ICollection<UserLocation> UserLocations { get; set; } = [];
}

public class UserLocation
{
    public Guid UserId { get; set; }
    public Guid LocationId { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public LmsUser User { get; set; } = null!;
    public Location Location { get; set; } = null!;
}

public class AccountActivationToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public LmsUser User { get; set; } = null!;
}

public class PasswordResetToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public LmsUser User { get; set; } = null!;
}

public class RevokedToken
{
    public Guid Id { get; set; }
    public string Jti { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class ExternalLogin
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string ProviderUserId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public LmsUser User { get; set; } = null!;
}

public class PendingExternalLogin
{
    public Guid Id { get; set; }
    public string Ticket { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string ProviderUserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Client { get; set; } = "web";
    public string? ReturnPath { get; set; }
    public string? CodeHash { get; set; }
    public DateTimeOffset? CodeExpiresAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
