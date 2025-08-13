namespace CarWashApp.Models
{
    public class Transaction
    {
        public int Id { get; set; }             // Primary key
        public decimal Amount { get; set; }     // Amount of cash
        public DateTime Date { get; set; }      // Date of transaction
        public PaymentMethod PaymentMethod { get; set; }  // e.g., Cash, Card, etc.
        public string Description { get; set; } // Optional: what the payment is for
    }

    public enum PaymentMethod
    {
        Cash,
        Card,
        Other
    }
}
