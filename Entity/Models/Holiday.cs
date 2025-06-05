using System;
using System.Collections.Generic;

namespace Entity.Models;

public partial class Holiday
{
    public int Holidayid { get; set; }

    public string Holidayname { get; set; } = null!;

    public DateOnly Holidaydate { get; set; }

    public bool Isdeleted { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public int Createdby { get; set; }

    public int? Updatedby { get; set; }
}
