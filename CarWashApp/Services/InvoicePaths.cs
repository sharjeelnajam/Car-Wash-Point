namespace CarWashApp.Services
{
    public static class InvoicePaths
    {
        /// <summary>
        /// Folder for scanned invoice images: the "InvoicePath" setting if present,
        /// otherwise an "Invoices" folder next to the app.
        /// </summary>
        public static string Resolve(IConfiguration configuration)
        {
            var configured = configuration["InvoicePath"];
            return string.IsNullOrWhiteSpace(configured)
                ? Path.Combine(AppContext.BaseDirectory, "Invoices")
                : configured;
        }
    }
}
