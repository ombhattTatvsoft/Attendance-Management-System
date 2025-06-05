using System;
using System.Collections.Generic;

namespace Entity.Models;

public partial class User
{
    public int Userid { get; set; }

    public int Roleid { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int? Managerid { get; set; }

    public string? Department { get; set; }

    public bool Isdeleted { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public int Createdby { get; set; }

    public int? Updatedby { get; set; }

    public virtual ICollection<Attendance> AttendanceApprovedbyNavigations { get; } = new List<Attendance>();

    public virtual ICollection<Attendance> AttendanceUsers { get; } = new List<Attendance>();

    public virtual ICollection<User> InverseManager { get; } = new List<User>();

    public virtual ICollection<Leave> LeaveApprovedbyNavigations { get; } = new List<Leave>();

    public virtual ICollection<Leave> LeaveUsers { get; } = new List<Leave>();

    public virtual User? Manager { get; set; }

    public virtual Role Role { get; set; } = null!;
}
