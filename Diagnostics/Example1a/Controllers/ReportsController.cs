using Example1a.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Example1a.Controllers
{
    /// <summary>
    /// Controller for generating / downloading reports
    /// </summary>
    public class ReportsController : Controller
    {
        private IReportRepository repository;

        public ReportsController(IReportRepository repository)
        {
            this.repository = repository;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult DownloadXml(string date)
        {
            DateTime dateTime = DateTime.Parse(date);
            var report = repository.GetReport(dateTime);
            var document = new XDocument(new XElement("Report",
                new XAttribute("date", report.Date),
                report.Lines.Select(line => new XElement("Line",
                    new XAttribute("productName", line.ProductName),
                    new XAttribute("price", line.Price),
                    new XAttribute("quantity", line.Quantity))
               ))
            );
            var stream = new MemoryStream();
            document.Save(stream);
            return File(stream.ToArray(), "text/xml", "report.xml");
        }
    }
}
