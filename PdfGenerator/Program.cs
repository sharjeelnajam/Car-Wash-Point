using QuestPDF.Companion;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PdfGenerator
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            Console.WriteLine("Generating PDF...");
            try
            {

                await Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(50);
                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Text("Car Wash Invosice").FontSize(30).Bold();
                            row.RelativeItem().Width(1, Unit.Inch).Image(Placeholders.Image(100, 100));
                        });//.Text("Car Wash Invoice").FontSize(30).Bold().AlignCenter();
                        page.Content().PaddingTop(50).Column(col =>
                        {
                            col.Item().Text($"Date & Time: {DateTime.Now.ToLocalTime()}").FontSize(14);
                            col.Item().Text($"Invoice #: 12345").FontSize(14);
                            col.Item().Text($"Customer Name: John Doe").FontSize(14);
                            col.Item().Text($"Service: Full Wash").FontSize(14);
                            col.Item().Text($"Total Amount: $25.00").FontSize(14).Bold();
                        });
                    });

                }).ShowInCompanionAsync();
            }
            catch (Exception e)
            {
                throw;
            }
            Console.WriteLine("PDF generation completed. Press any key to exit.");
        }
    }


}
