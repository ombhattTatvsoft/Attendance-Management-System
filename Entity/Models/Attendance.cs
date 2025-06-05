using System;
using System.Collections.Generic;

namespace Entity.Models;

public partial class Attendance
{
    public int Attendanceid { get; set; }

    public int Userid { get; set; }

    public DateOnly Date { get; set; }

    public TimeOnly? Intime { get; set; }

    public TimeOnly? Outtime { get; set; }

    public string Status { get; set; } = null!;

    public bool? Isapproved { get; set; }

    public int? Approvedby { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public int Createdby { get; set; }

    public int? Updatedby { get; set; }

    public virtual User? ApprovedbyNavigation { get; set; }

    public virtual User User { get; set; } = null!;
}
