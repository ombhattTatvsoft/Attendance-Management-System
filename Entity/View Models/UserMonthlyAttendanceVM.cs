namespace Entity.ViewModels;

public class UserMonthlyAttendanceVM
{
    public int UserId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public List<UserAttendanceVM> AttendanceRecords { get; set; } = new();
}
public class UserAttendanceVM
{
    public DateOnly Date { get; set; }
    public string? Day { get; set; } = null!;
    public bool? IsHoliday { get; set; }
    public string? Status { get; set; }
    public bool? IsApproved { get; set; }
}
