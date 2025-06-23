using System.ComponentModel.DataAnnotations;

namespace Entity.ViewModels;

public class UpsertDepartmentVM
{
    public int DepartmentId { get; set; }
    
    [RegularExpression(@"^[A-Za-z]+(?:\s[A-Za-z]+)*$", ErrorMessage = "Enter valid Name")]
    [MaxLength(100), Required(ErrorMessage = "Name is Required")]
    public string DepartmentName { get; set; } = null!;
}
