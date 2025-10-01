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
                var sampleInvoice = new InvoiceDto
                {
                    CustomerName = "John Smith",
                    CustomerEmail = "john.smith@email.com",
                    CustomerPhone = "+44 123 456 7890",
                    VehicleType = "Car",
                    LicensePlate = "ABC123",
                    InvoiceNumber = "INV-2024-001",
                    InvoiceDate = DateTime.Now,
                    ServiceItems = new List<ServiceItem>
                    {
                        new ServiceItem { Description = "Basic Wash, Wheels & Blow Dry", Quantity = 1, UnitPrice = 8.00m, TotalPrice = 8.00m },
                        new ServiceItem { Description = "Tyre Shine", Quantity = 1, UnitPrice = 2.00m, TotalPrice = 2.00m },
                        new ServiceItem { Description = "Tango Spray Wax", Quantity = 1, UnitPrice = 5.00m, TotalPrice = 5.00m },
                        new ServiceItem { Description = "Vacuum Interior", Quantity = 1, UnitPrice = 5.00m, TotalPrice = 5.00m }
                    },
                    Subtotal = 20.00m,
                    TaxRate = 0.20m,
                    TaxAmount = 4.00m,
                    TotalAmount = 24.00m,
                    PaymentMethod = "Card Payment",
                    Notes = "Vehicle was in good condition. Customer requested extra attention to wheels."
                };

                return Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        // Header Section
                        page.Header()
                            .Height(120)
                            .Background(Colors.Blue.Medium)
                            .Padding(20)
                            .Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("DR WASH")
                                        .FontSize(24)
                                        .FontColor(Colors.White)
                                        .Bold();
                                    col.Item().Text("Professional Vehicle Care Services")
                                        .FontSize(12)
                                        .FontColor(Colors.White);
                                    col.Item().PaddingTop(10).Text("123 Main Street, City Center")
                                        .FontSize(10)
                                        .FontColor(Colors.White);
                                    col.Item().Text("Phone: +44 123 456 7890 | Email: info@carwashpoint.com")
                                        .FontSize(10)
                                        .FontColor(Colors.White);
                                });
                                row.ConstantItem(120).AlignRight().Placeholder();
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
                                        invoiceCol.Item().Text($"Invoice #: {sampleInvoice.InvoiceNumber}")
                                            .FontSize(12)
                                            .Bold();
                                        invoiceCol.Item().Text($"Date: {sampleInvoice.InvoiceDate:dd/MM/yyyy}")
                                            .FontSize(12);
                                        invoiceCol.Item().Text($"Time: {sampleInvoice.InvoiceDate:HH:mm}")
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
                                        customerCol.Item().PaddingTop(5).Text(sampleInvoice.CustomerName)
                                            .FontSize(12)
                                            .Bold();
                                        if (!string.IsNullOrEmpty(sampleInvoice.CustomerEmail))
                                            customerCol.Item().Text(sampleInvoice.CustomerEmail)
                                                .FontSize(10);
                                        if (!string.IsNullOrEmpty(sampleInvoice.CustomerPhone))
                                            customerCol.Item().Text(sampleInvoice.CustomerPhone)
                                                .FontSize(10);
                                    });

                                    row.ConstantItem(200).Column(vehicleCol =>
                                    {
                                        vehicleCol.Item().Text("VEHICLE DETAILS:")
                                            .FontSize(14)
                                            .Bold()
                                            .FontColor(Colors.Grey.Medium);
                                        vehicleCol.Item().PaddingTop(5).Text($"Type: {sampleInvoice.VehicleType}")
                                            .FontSize(12);
                                        vehicleCol.Item().Text($"License Plate: {sampleInvoice.LicensePlate}")
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
                                        header.Cell().Background(Colors.Grey.Medium)
                                            .Padding(8)
                                            .Text("DESCRIPTION")
                                            .FontSize(11)
                                            .Bold()
                                            .FontColor(Colors.Grey.Medium);
                                        header.Cell().Background(Colors.Grey.Medium)
                                            .Padding(8)
                                            .AlignCenter()
                                            .Text("QTY")
                                            .FontSize(11)
                                            .Bold()
                                            .FontColor(Colors.Grey.Medium);
                                        header.Cell().Background(Colors.Grey.Medium)
                                            .Padding(8)
                                            .AlignRight()
                                            .Text("UNIT PRICE")
                                            .FontSize(11)
                                            .Bold()
                                            .FontColor(Colors.Grey.Medium);
                                        header.Cell().Background(Colors.Grey.Medium)
                                            .Padding(8)
                                            .AlignRight()
                                            .Text("TOTAL")
                                            .FontSize(11)
                                            .Bold()
                                            .FontColor(Colors.Grey.Medium);
                                    });

                                    // Service Items
                                    foreach (var item in sampleInvoice.ServiceItems)
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
                                        totalsCol.Item().Row(totalRow =>
                                        {
                                            totalRow.RelativeItem().Text("Subtotal:")
                                                .FontSize(12);
                                            totalRow.ConstantItem(80).AlignRight()
                                                .Text($"£{sampleInvoice.Subtotal:F2}")
                                                .FontSize(12);
                                        });

                                        totalsCol.Item().Row(totalRow =>
                                        {
                                            totalRow.RelativeItem().Text($"VAT ({(sampleInvoice.TaxRate * 100):F0}%):")
                                                .FontSize(12);
                                            totalRow.ConstantItem(80).AlignRight()
                                                .Text($"£{sampleInvoice.TaxAmount:F2}")
                                                .FontSize(12);
                                        });

                                        totalsCol.Item().PaddingTop(5).Row(totalRow =>
                                        {
                                            totalRow.RelativeItem().Text("TOTAL:")
                                                .FontSize(14)
                                                .Bold()
                                                .FontColor(Colors.Blue.Medium);
                                            totalRow.ConstantItem(80).AlignRight()
                                                .Text($"£{sampleInvoice.TotalAmount:F2}")
                                                .FontSize(14)
                                                .Bold()
                                                .FontColor(Colors.Blue.Medium);
                                        });
                                    });
                                });

                                col.Item().PaddingVertical(20);

                                // Payment Information
                                if (!string.IsNullOrEmpty(sampleInvoice.PaymentMethod))
                                {
                                    col.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text($"Payment Method: {sampleInvoice.PaymentMethod}")
                                            .FontSize(12)
                                            .Bold();
                                    });
                                }

                                // Notes
                                if (!string.IsNullOrEmpty(sampleInvoice.Notes))
                                {
                                    col.Item().PaddingTop(20).Text("Notes:")
                                        .FontSize(12)
                                        .Bold()
                                        .FontColor(Colors.Grey.Medium);
                                    col.Item().PaddingTop(5).Text(sampleInvoice.Notes)
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
