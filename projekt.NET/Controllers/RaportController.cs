using Microsoft.AspNetCore.Mvc;
using projekt.NET.Data;
using projekt.NET.Reports;
using QuestPDF.Fluent;

namespace projekt.NET.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult DownloadLibraryPdf()
        {
            // Później będzie tu pobieranie danych z bazy, ale na razie korzystamy z DataStorage
            // Pobiera dane z klasy DataStorage
            var games = DataStorage.MyGames;

            var report = new GamesReport(games);
            byte[] pdfBytes = report.GeneratePdf();

            return File(pdfBytes, "application/pdf", "Moja_Biblioteka.pdf");
        }
    }
}