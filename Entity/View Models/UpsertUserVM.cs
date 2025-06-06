using System.ComponentModel.DataAnnotations;

namespace Entity.ViewModels;

public class UpsertUserVM
{
    public int UserId { get; set; }

    [Required(ErrorMessage = "Role is Required")]
    public int RoleId { get; set; }

    [RegularExpression(@"^[A-Za-z]+(?:\s[A-Za-z]+)*$", ErrorMessage = "Enter valid Name")]
    [MaxLength(50), Required(ErrorMessage = "Name is Required")]
    public string Name { get; set; } = null!;

    [MaxLength(50), EmailAddress, Required(ErrorMessage = "Email is Required")]
    [RegularExpression(@"^\S+$", ErrorMessage = "Enter valid Email")]
    public string Email { get; set; } = null!;

    [MaxLength(50), Required(ErrorMessage = "Password is Required")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d\s]).{8,}$",
    ErrorMessage = "Password must be at least 8 characters and include uppercase, lowercase, number, and special character")]
    public string Password { get; set; } = null!;
    public int? ManagerId { get; set; }
    public int Department { get; set; }
}
