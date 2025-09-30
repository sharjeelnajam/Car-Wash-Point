using QuestPDF.Companion;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;

namespace CarWashApp.Services
{
    public class InvoiceService
    {
        public async Task<byte[]> GenerateInvoice(InvoiceDto invoiceDto)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            Console.WriteLine("Generating PDF...");
            try
            {

                return Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(50);
                        page.Header().Row(row =>
                        {
                            row.RelativeItem().AlignMiddle().AlignLeft().Text("Car Wash Invoice").FontSize(30).Bold();
                            row.ConstantItem(150).Image("wwwroot/img/logo.png");
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
                                table.Cell().PaddingVertical(10).Text(invoiceDto.CustomerName).FontSize(14);

                                table.Cell().PaddingVertical(10).Text("Service:").FontSize(14).Bold();
                                table.Cell().PaddingVertical(10).Text(invoiceDto.Service).FontSize(14);

                                table.Cell().PaddingVertical(10).Text("Total Amount:").FontSize(14).Bold();
                                table.Cell().PaddingVertical(10).Text(invoiceDto.Amount).FontSize(14).Bold();
                            });
                        });

                    });

                }).GeneratePdf();
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }

    public class InvoiceDto
    {
        public string CustomerName { get; set; }
        public string Service { get; set; }
        public string Amount { get; set; }

    }
}
