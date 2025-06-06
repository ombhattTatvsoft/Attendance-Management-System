using System.ComponentModel.DataAnnotations;

namespace Entity.ViewModels;

public class UserLoginVM
{
    [MaxLength(50),EmailAddress,Required(ErrorMessage="Email is Required")]
    public string Email { get; set; } = null!;

    [MaxLength(50),Required(ErrorMessage="Password is Required")]
    public string Password { get; set; } = null!;
}
