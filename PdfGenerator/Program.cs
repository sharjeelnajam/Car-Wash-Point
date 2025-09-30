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

                            row.RelativeItem().AlignMiddle().AlignLeft().Text("Car Wash Invosice").FontSize(30).Bold();
                            row.ConstantItem(150).Image(Placeholders.Image(100, 100));
                        });
                        page.Content().PaddingTop(100).AlignCenter().Column(col =>
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(200);
                                });

                                table.Cell().Text("Date & Time:").FontSize(14).Bold();
                                table.Cell().Text($"{DateTime.Now.ToLocalTime()}").FontSize(14);

                                table.Cell().PaddingVertical(10).Text("Invoice #:").FontSize(14).Bold();
                                table.Cell().PaddingVertical(10).Text("12345").FontSize(14);

                                table.Cell().PaddingVertical(10).Text("Customer Name:").FontSize(14).Bold();
                                table.Cell().PaddingVertical(10).Text("John Doe").FontSize(14);

                                table.Cell().PaddingVertical(10).Text("Service:").FontSize(14).Bold();
                                table.Cell().PaddingVertical(10).Text("Full Wash").FontSize(14);

                                table.Cell().PaddingVertical(10).Text("Total Amount:").FontSize(14).Bold();
                                table.Cell().PaddingVertical(10).Text("8$").FontSize(14).Bold();
                            });
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
