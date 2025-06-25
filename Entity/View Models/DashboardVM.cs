using Entity.Models;

namespace Entity.ViewModels;

public class DashboardVM
{
    public AdminDashboard? AdminDashboard { get; set; }
    public DaysByAttendance? UserDashboard { get; set; }
    public List<User> Team { get; set; } = new List<User>();
}
public class AdminDashboard
{
    public int TotalEmployee { get; set; }
    public int PresentEmployee { get; set; }
    public int AbsentEmployee { get; set; }
    public int SeniorManagers { get; set; }
    public int Managers { get; set; }
    public int Employees { get; set; }
}
public class DaysByAttendance
{
    public double PresentDays { get; set; }
    public double FullAbsentDays { get; set; }
    public double HalfDays { get; set; }
    public double Holidays { get; set; }
    public double TotalDays { get; set; }
}