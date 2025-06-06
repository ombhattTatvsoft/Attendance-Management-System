using System.ComponentModel.DataAnnotations;

namespace Entity.ViewModels;

public class ForgotPasswordVM
{
    [MaxLength(50),EmailAddress,Required(ErrorMessage="Email is Required")]
    public string Email { get; set; } = null!;
}
