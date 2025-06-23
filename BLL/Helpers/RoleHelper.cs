using Entity.Enums;

namespace BLL.Helpers;

public static class RoleHelper
{
    public static string ToDbString(RoleEnum role)
    {
        return role switch
        {
            RoleEnum.Admin => "Admin",
            RoleEnum.SeniorManager => "Senior Manager",
            RoleEnum.Manager => "Manager",
            RoleEnum.Employee => "Employee",
            _ => "Unknown Role"
        };
    }
}
