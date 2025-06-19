using System.ComponentModel.DataAnnotations;
using Entity.Models;

namespace Entity.ViewModels;

public class LeaveVM
{
    public int LeaveId { get; set; }

    public int UserId { get; set; }

    [Required(ErrorMessage = "From date is required")]
    public DateOnly FromDate { get; set; }

    [Required(ErrorMessage = "To date is required")]
    public DateOnly ToDate { get; set; }

    [Required, MaxLength(500)]
    [RegularExpression(@"^[A-Za-z]+(?:\s[A-Za-z]+)*$", ErrorMessage = "Enter valid Reason")]
    public string Reason { get; set; } = null!;

    public bool? IsApproved { get; set; }

    public string? Comment { get; set; }

    public decimal FromDateLeaveDuration { get; set; }

    public decimal ToDateLeaveDuration { get; set; }

    [Required]
    [Range(0.5, 10, ErrorMessage = "Max 10 Leave allowed, ensure selected dates are not holiday already")]
    public decimal TotalLeave { get; set; }
    public virtual User? ApprovedByNavigation { get; set; }

    public virtual User? User { get; set; } = null!;
}
