using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using projekt.NET.Models;

namespace projekt.NET.Reports
{
    public class GamesReport : IDocument
    {
        public List<UserGame> Model { get; }

        public GamesReport(List<UserGame> model)
        {
            Model = model;
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(1, Unit.Centimetre);
                page.Header().Text("Raport Mojej Biblioteki Gier").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Tytuł
                        columns.RelativeColumn(2); // Status
                        columns.RelativeColumn(1); // Ocena
                        columns.RelativeColumn(1); // Czas gry
                    });

                    // Nagłówki
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Tytuł");
                        header.Cell().Element(CellStyle).Text("Status");
                        header.Cell().Element(CellStyle).Text("Ocena");
                        header.Cell().Element(CellStyle).Text("Godziny");

                        static IContainer CellStyle(IContainer container) =>
                            container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                    });

                    // Dane
                    foreach (var game in Model)
                    {
                        table.Cell().Element(ValueStyle).Text(game.Title);
                        table.Cell().Element(ValueStyle).Text(game.Status);
                        table.Cell().Element(ValueStyle).Text(game.Rating?.ToString() ?? "-");
                        table.Cell().Element(ValueStyle).Text($"{game.PlayTimeHours}h");

                        static IContainer ValueStyle(IContainer container) =>
                            container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Strona ");
                    x.CurrentPageNumber();
                });
            });
        }
    }
}