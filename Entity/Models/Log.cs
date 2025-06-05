using System;
using System.Collections.Generic;

namespace Entity.Models;

public partial class Log
{
    public int Logid { get; set; }

    public string Logmessage { get; set; } = null!;

    public DateTime? Createdat { get; set; }
}
