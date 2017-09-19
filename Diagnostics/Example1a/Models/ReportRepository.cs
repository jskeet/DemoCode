using Example1a.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Example1a.Models
{
    public class ReportRepository : IReportRepository
    {
        private readonly ReportDbConfig config;

        public ReportRepository(IOptions<ReportDbConfig> options)
        {
            this.config = options.Value;
        }

        public Report GetReport(DateTime date)
        {
            var report = new Report { Date = date };
            using (var conn = new SqlConnection(config.ConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT * FROM REPORTLINES WHERE REPORTDATE=@date", conn))
                {
                    cmd.Parameters.Add("date", SqlDbType.Date).Value = date;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            report.Lines.Add(new ReportLine
                            {
                                Price = (decimal)reader["price"],
                                ProductName = (string)reader["productname"],
                                Quantity = (int)reader["quantity"]
                            });
                        }
                    }
                }
            }
            return report;
        }
    }
}
