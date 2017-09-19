using System;
using System.Collections.Generic;

namespace Example1a.Models
{
    public class Report
    {
        public DateTime Date { get; set; }
        public List<ReportLine> Lines { get; } = new List<ReportLine>();
    }
}
