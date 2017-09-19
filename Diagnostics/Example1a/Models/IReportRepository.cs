using System;

namespace Example1a.Models
{
    public interface IReportRepository
    {
        Report GetReport(DateTime date);
    }
}
