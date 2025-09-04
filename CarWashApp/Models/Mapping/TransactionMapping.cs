using CarWashApp.Models.DTOs;

namespace CarWashApp.Models.Mapping
{
    public static class TransactionMapping
    {
        public static TransactionDto ToDto(this Transaction transaction)
        {
            if (transaction == null) return null;
            return new TransactionDto
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                Date = transaction.Date,
                PaymentMethod = transaction.PaymentMethod,
                Description = transaction.Description
            };
        }
        public static Transaction ToEntity(this TransactionDto transaction)
        {
            if (transaction == null) return null;
            return new Transaction
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                Date = transaction.Date,
                PaymentMethod = transaction.PaymentMethod,
                Description = transaction.Description
            };
        }
    }
}
