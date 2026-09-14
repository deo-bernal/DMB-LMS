using Dmb.Lms.Model;
using Xunit;

namespace Dmb.Lms.Test;

public class RolesTests
{
    [Fact]
    public void Marketplace_roles_include_parent_and_tutor()
    {
        Assert.Contains(Roles.Owner, Roles.All);
        Assert.Contains(Roles.Admin, Roles.All);
        Assert.Contains(Roles.Tutor, Roles.All);
        Assert.Contains(Roles.Parent, Roles.All);
        Assert.Equal(4, Roles.All.Length);
    }
}
