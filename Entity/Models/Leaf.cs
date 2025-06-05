using System;
using System.Collections.Generic;

namespace Entity.Models;

public partial class Leave
{
    public int Leaveid { get; set; }

    public int Userid { get; set; }

    public DateOnly Fromdate { get; set; }

    public DateOnly Todate { get; set; }

    public string Reason { get; set; } = null!;

    public bool? Isapproved { get; set; }

    public int? Approvedby { get; set; }

    public string? Comment { get; set; }

    public bool Isdeleted { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public int Createdby { get; set; }

    public int? Updatedby { get; set; }

    public virtual User? ApprovedbyNavigation { get; set; }

    public virtual User User { get; set; } = null!;
}
