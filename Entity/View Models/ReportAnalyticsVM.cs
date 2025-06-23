using Entity.Models;

namespace Entity.ViewModels;

public class ReportAnalyticsVM
{
    public User User { get; set; } = null!;
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int HalfPresentCount { get; set; }
    public double AttendancePercentage { get; set; }
}
