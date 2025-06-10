using System.ComponentModel.DataAnnotations;

namespace Entity.Enums;

public enum Role
{
    Admin = 1,
    [Display(Name ="Senior Manager")]
    Senior_Manager = 2,
    Manager = 3,
    Employee = 4
}
