using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Example1b
{
    class Program
    {
        static void Main()
        {
            var report = GetReport(new DateTime(2017, 5, 6));
            Console.WriteLine(report.Date);
            foreach (var line in report.Lines)
            {
                Console.WriteLine($"{line.ProductName}: {line.Price} (x{line.Quantity})");
            }
        }

        public static Report GetReport(DateTime date)
        {
            var report = new Report { Date = date };
            using (var conn = new SqlConnection("Hard-coded connection string here"))
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

    public class Report
    {
        public DateTime Date { get; set; }
        public List<ReportLine> Lines { get; } = new List<ReportLine>();
    }

    public class ReportLine
    {
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
