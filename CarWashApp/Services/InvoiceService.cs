using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace CarWashApp.Services
{
    /// <summary>
    /// Builds the customer receipt as a narrow, centred, typewriter-style PDF (like a card-terminal slip),
    /// sized for a receipt printer. Width and business details can be changed in appsettings:
    ///   "Receipt": { "WidthMm": 80, "Name": "DR. WASH", "AddressLine1": "...", "AddressLine2": "...", "Footer": "THANK YOU!" }
    /// </summary>
    public class InvoiceService(IConfiguration configuration)
    {
        private const string FontFamily = "Courier New";
        private const float FontSize = 9;
        private const float Margin = 8;

        public Task<byte[]> GenerateInvoice(InvoiceDto invoiceDto)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            return Task.FromResult(CreateDocument(invoiceDto).GeneratePdf());
        }

        public IDocument CreateDocument(InvoiceDto invoice)
        {
            var widthMm = configuration.GetValue("Receipt:WidthMm", 80);
            var widthPt = widthMm * 72f / 25.4f;

            // Courier characters are 0.6 of the font size wide, so this is how many fit on one line.
            var lineChars = (int)Math.Floor((widthPt - 2 * Margin) / (FontSize * 0.6f));
            var heavyRule = new string('=', lineChars);
            var lightRule = new string('-', lineChars);

            var name = configuration["Receipt:Name"] ?? "DR. WASH";
            var address1 = configuration["Receipt:AddressLine1"] ?? "GLASGOW";
            var address2 = configuration["Receipt:AddressLine2"] ?? "G42 7BH 142 CATHCART ROAD";
            var footer = configuration["Receipt:Footer"] ?? "THANK YOU!";

            var when = invoice.InvoiceDate == default ? DateTime.Now : invoice.InvoiceDate;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.ContinuousSize(widthPt);
                    page.Margin(Margin);
                    page.DefaultTextStyle(x => x.FontFamily(FontFamily).FontSize(FontSize));

                    page.Content().Column(col =>
                    {
                        void Centre(string text, bool bold = false) =>
                            col.Item().AlignCenter().Text(t => { var s = t.Span(text); if (bold) s.Bold(); });

                        void Pair(string left, string right, bool bold = false) =>
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text(t => { var s = t.Span(left); if (bold) s.Bold(); });
                                row.AutoItem().PaddingLeft(6).Text(t => { var s = t.Span(right); if (bold) s.Bold(); });
                            });

                        void Gap() => col.Item().Height(6);

                        // Header
                        col.Item().Text(heavyRule);
                        Centre(name, bold: true);
                        col.Item().Text(heavyRule);

                        Centre(address1);
                        Centre(address2);
                        Gap();

                        Pair($"DATE: {when:dd/MM/yy}", $"TIME: {when:HH:mm:ss}");
                        col.Item().Text($"INVOICE: {invoice.InvoiceNumber}");
                        Gap();

                        // Vehicle
                        if (!string.IsNullOrWhiteSpace(invoice.VehicleType))
                            col.Item().Text($"VEHICLE: {invoice.VehicleType.ToUpperInvariant()}");
                        if (!string.IsNullOrWhiteSpace(invoice.LicensePlate))
                            col.Item().Text($"PLATE: {invoice.LicensePlate.ToUpperInvariant()}");
                        Gap();

                        // Items
                        col.Item().Text(lightRule);
                        foreach (var item in invoice.ServiceItems)
                        {
                            var description = (item.Description ?? "").ToUpperInvariant();
                            if (item.TotalPrice == 0m)
                                col.Item().Text($"  {description}");   // included extras: no price
                            else
                                Pair(description, item.TotalPrice.ToString("C"));
                        }
                        col.Item().Text(lightRule);

                        // Total and payment
                        Pair("TOTAL", invoice.TotalAmount.ToString("C"), bold: true);
                        Gap();
                        Pair("PAYMENT", (invoice.PaymentMethod ?? "").ToUpperInvariant());
                        Gap();

                        // Footer
                        col.Item().Text(heavyRule);
                        Centre(footer, bold: true);
                        col.Item().Text(heavyRule);
                    });
                });
            });
        }
    }

    public class InvoiceDto
    {
        public string CustomerName { get; set; } = "";
        public string CustomerEmail { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public string LicensePlate { get; set; } = "";
        public string InvoiceNumber { get; set; } = "";
        public DateTime InvoiceDate { get; set; }
        public List<ServiceItem> ServiceItems { get; set; } = new List<ServiceItem>();
        public decimal Subtotal { get; set; }
        public decimal TaxRate { get; set; } = 0.20m; // 20% VAT
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string Notes { get; set; } = "";
    }

    public class ServiceItem
    {
        public string Description { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
