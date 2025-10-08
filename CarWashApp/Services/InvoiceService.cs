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
                // Use the actual invoice data passed to the method
                var invoiceData = invoiceDto;

                // ✅ Resolve image path under wwwroot/img/
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "invoice-header.WEBP");
                if (!File.Exists(imagePath))
                    throw new FileNotFoundException($"Invoice header image not found: {imagePath}");

                return Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        // ✅ Header Section with image
                        page.Header()
                            .Height(120)
                            .Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("DR WASH")
                                        .FontSize(24)
                                        .FontColor(Colors.Blue.Medium)
                                        .Bold();
                                    col.Item().Text("Professional Vehicle Care Services")
                                        .FontSize(12)
                                        .FontColor(Colors.Grey.Medium);
                                    col.Item().PaddingTop(10).Text("123 Main Street, City Center")
                                        .FontSize(10)
                                        .FontColor(Colors.Grey.Medium);
                                    col.Item().Text("Professional Vehicle Care Services")
                                        .FontSize(10)
                                        .FontColor(Colors.Grey.Medium);
                                });

                                // ✅ Add header logo image
                                row.ConstantItem(120)
                                   .AlignRight()
                                   .Image(imagePath)
                                   .FitArea();
                            });

                        // Invoice Title and Details
                        page.Content()
                            .PaddingTop(20)
                            .Column(col =>
                            {
                                // Invoice Title
                                col.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("INVOICE")
                                        .FontSize(28)
                                        .Bold()
                                        .FontColor(Colors.Blue.Medium);
                                    row.ConstantItem(200).AlignRight().Column(invoiceCol =>
                                    {
                                        invoiceCol.Item().Text($"Invoice #: {invoiceData.InvoiceNumber}")
                                            .FontSize(12)
                                            .Bold();
                                        invoiceCol.Item().Text($"Date: {invoiceData.InvoiceDate:dd/MM/yyyy}")
                                            .FontSize(12);
                                        invoiceCol.Item().Text($"Time: {invoiceData.InvoiceDate:HH:mm}")
                                            .FontSize(12);
                                    });
                                });

                                col.Item().PaddingVertical(20);

                                // Customer Information
                                col.Item().Row(row =>
                                {
                                    row.RelativeItem().Column(customerCol =>
                                    {
                                        customerCol.Item().Text("BILL TO:")
                                            .FontSize(14)
                                            .Bold()
                                            .FontColor(Colors.Grey.Medium);
                                        customerCol.Item().PaddingTop(5).Text(invoiceData.CustomerName)
                                            .FontSize(12)
                                            .Bold();
                                        // Email and phone removed per requirement
                                    });

                                    row.ConstantItem(200).Column(vehicleCol =>
                                    {
                                        vehicleCol.Item().Text("VEHICLE DETAILS:")
                                            .FontSize(14)
                                            .Bold()
                                            .FontColor(Colors.Grey.Medium);
                                        vehicleCol.Item().PaddingTop(5).Text($"Type: {invoiceData.VehicleType}")
                                            .FontSize(12);
                                        vehicleCol.Item().Text($"License Plate: {invoiceData.LicensePlate}")
                                            .FontSize(12)
                                            .Bold();
                                    });
                                });

                                col.Item().PaddingVertical(20);

                                // Services Table
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3); // Description
                                        columns.ConstantColumn(60); // Qty
                                        columns.ConstantColumn(80); // Unit Price
                                        columns.ConstantColumn(80); // Total
                                    });

                                    // Table Header
                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Lighten2)
                                            .Padding(8)
                                            .Text("DESCRIPTION")
                                            .FontSize(11)
                                            .Bold();
                                        header.Cell().Background(Colors.Grey.Lighten2)
                                            .Padding(8)
                                            .AlignCenter()
                                            .Text("QTY")
                                            .FontSize(11)
                                            .Bold();
                                        header.Cell().Background(Colors.Grey.Lighten2)
                                            .Padding(8)
                                            .AlignRight()
                                            .Text("UNIT PRICE")
                                            .FontSize(11)
                                            .Bold();
                                        header.Cell().Background(Colors.Grey.Lighten2)
                                            .Padding(8)
                                            .AlignRight()
                                            .Text("TOTAL")
                                            .FontSize(11)
                                            .Bold();
                                    });

                                    // Service Items
                                    foreach (var item in invoiceData.ServiceItems)
                                    {
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                            .Padding(8)
                                            .Text(item.Description)
                                            .FontSize(10);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                            .Padding(8)
                                            .AlignCenter()
                                            .Text(item.Quantity.ToString())
                                            .FontSize(10);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                            .Padding(8)
                                            .AlignRight()
                                            .Text($"£{item.UnitPrice:F2}")
                                            .FontSize(10);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                            .Padding(8)
                                            .AlignRight()
                                            .Text($"£{item.TotalPrice:F2}")
                                            .FontSize(10);
                                    }
                                });

                                col.Item().PaddingVertical(20);

                                // Totals Section
                                col.Item().Row(row =>
                                {
                                    row.RelativeItem(); // Empty space
                                    row.ConstantItem(200).Column(totalsCol =>
                                    {
                                        // Subtotal
                                        totalsCol.Item().Row(subtotalRow =>
                                        {
                                            subtotalRow.RelativeItem().Text("SUBTOTAL:")
                                                .FontSize(12)
                                                .Bold();
                                            subtotalRow.ConstantItem(80).AlignRight()
                                                .Text($"${invoiceData.Subtotal:F2}")
                                                .FontSize(12)
                                                .Bold();
                                        });

                                        // Tax
                                        totalsCol.Item().Row(taxRow =>
                                        {
                                            taxRow.RelativeItem().Text($"TAX ({(invoiceData.TaxRate * 100):F0}%):")
                                                .FontSize(12)
                                                .Bold();
                                            taxRow.ConstantItem(80).AlignRight()
                                                .Text($"${invoiceData.TaxAmount:F2}")
                                                .FontSize(12)
                                                .Bold();
                                        });

                                        // Total
                                        totalsCol.Item().Row(totalRow =>
                                        {
                                            totalRow.RelativeItem().Text("TOTAL:")
                                                .FontSize(14)
                                                .Bold()
                                                .FontColor(Colors.Blue.Medium);
                                            totalRow.ConstantItem(80).AlignRight()
                                                .Text($"£{invoiceData.TotalAmount:F2}")
                                                .FontSize(14)
                                                .Bold()
                                                .FontColor(Colors.Blue.Medium);
                                        });
                                    });
                                });

                                col.Item().PaddingVertical(20);

                                // Payment Information
                                if (!string.IsNullOrEmpty(invoiceData.PaymentMethod))
                                {
                                    col.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text($"Payment Method: {invoiceData.PaymentMethod}")
                                            .FontSize(12)
                                            .Bold();
                                    });
                                }

                                // Notes
                                if (!string.IsNullOrEmpty(invoiceData.Notes))
                                {
                                    col.Item().PaddingTop(20).Text("Notes:")
                                        .FontSize(12)
                                        .Bold()
                                        .FontColor(Colors.Grey.Medium);
                                    col.Item().PaddingTop(5).Text(invoiceData.Notes)
                                        .FontSize(10);
                                }

                                col.Item().PaddingTop(30);

                                // Footer
                                col.Item().BorderTop(1).BorderColor(Colors.Grey.Medium)
                                    .PaddingTop(15)
                                    .Row(footerRow =>
                                    {
                                        footerRow.RelativeItem().Column(footerCol =>
                                        {
                                            footerCol.Item().Text("Thank you for choosing Car Wash Point!")
                                                .FontSize(12)
                                                .Bold()
                                                .FontColor(Colors.Blue.Medium);
                                            footerCol.Item().PaddingTop(5).Text("Terms: Payment due upon receipt. Services are non-refundable.")
                                                .FontSize(9)
                                                .FontColor(Colors.Grey.Medium);
                                        });
                                        footerRow.ConstantItem(150).AlignRight().Column(footerCol =>
                                        {
                                            footerCol.Item().Text("Car Wash Point")
                                                .FontSize(10)
                                                .Bold();
                                            footerCol.Item().Text("Professional Vehicle Care")
                                                .FontSize(8)
                                                .FontColor(Colors.Grey.Medium);
                                        });
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
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string VehicleType { get; set; }
        public string LicensePlate { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public List<ServiceItem> ServiceItems { get; set; } = new List<ServiceItem>();
        public decimal Subtotal { get; set; }
        public decimal TaxRate { get; set; } = 0.20m; // 20% VAT
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string Notes { get; set; }
    }

    public class ServiceItem
    {
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
