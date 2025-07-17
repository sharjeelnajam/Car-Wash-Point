namespace CarWashPoint.Models
{
    public class Customer
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string QRCode { get; set; }           // Or NFC tag identifier
        public int PaidWashCount { get; set; }

        public bool IsEligibleForFreeWash => PaidWashCount > 0 && PaidWashCount % 5 == 0;
    }

}
