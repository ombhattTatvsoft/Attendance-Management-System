using System.ComponentModel.DataAnnotations;

namespace Entity.ViewModels;

public class HolidayVM
{
    public int HolidayId { get; set; }

    [RegularExpression(@"^[A-Za-z]+(?:\s[A-Za-z]+)*$", ErrorMessage = "Enter valid Name")]
    public string HolidayName { get; set; } = null!;

    [Required]
    [DataType(DataType.Date)]
    public DateOnly HolidayDate { get; set; }
}
