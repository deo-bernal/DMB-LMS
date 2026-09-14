namespace Dmb.Lms.Model;

public static class Roles
{
    public const string Owner = "owner";
    public const string Admin = "admin";
    public const string Tutor = "tutor";
    public const string Parent = "parent";

    public static readonly string[] All = [Owner, Admin, Tutor, Parent];
    public static readonly string[] Staff = [Owner, Admin];
}
