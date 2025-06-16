namespace Entity.ViewModels;

public class UpsertRegularize
{
    public int UserId{ get; set; }
    public DateOnly AttendanceDate { get; set; }
    public string SubmittedAttendance { get; set; } = null!;
    public string CorrectAttendance { get; set; } = null!;
}
