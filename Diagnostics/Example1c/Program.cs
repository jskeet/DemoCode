using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Example1c
{
    class Program
    {
        static void Main()
        {
            var dateTime = new DateTime(2017, 5, 6);
            var report = new Report
            {
                Date = dateTime,
                Lines = {
                    new ReportLine { ProductName = "ProductA", Price = 10.95m, Quantity = 5 },
                    new ReportLine { ProductName = "ProductB", Price = 100m, Quantity = 3 },
                    new ReportLine { ProductName = "ProductC", Price = 1.23m, Quantity = 67 },
                }
            };

            // Code from ReportsController
            var document = new XDocument(new XElement("Report",
                new XAttribute("date", report.Date),
                report.Lines.Select(line => new XElement("Line",
                    new XAttribute("productName", line.ProductName),
                    new XAttribute("price", line.Price),
                    new XAttribute("quantity", line.Quantity))
               ))
            );

            Console.WriteLine(document);
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
