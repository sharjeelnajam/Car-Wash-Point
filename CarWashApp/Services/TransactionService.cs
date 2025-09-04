using CarWashApp.Models;
using CarWashApp.Models.DTOs;
using CarWashApp.Models.Mapping;
using CarWashApp.Repositories;
using TransactionWashApp.Repositories;

namespace CarWashApp.Services
{
    public interface ITransactionService
    {
        TransactionDto GetTransactionById(int id);
        List<TransactionDto> GetAllTransactions();
        void AddTransaction(TransactionDto transaction);
        void UpdateTransaction(TransactionDto transaction);
    }
    public class TransactionService(ITransactionRepository transactionRepository) : ITransactionService
    {
        public void AddTransaction(TransactionDto transaction)
        {
            transactionRepository.AddTransaction(transaction.ToEntity());
        }

        public List<TransactionDto> GetAllTransactions()
        {
            var transactions = transactionRepository.GetAllTransactions();
            return transactions.Select(c => c.ToDto()).ToList();
        }

        public TransactionDto GetTransactionById(int id)
        {
            var transactionDto = transactionRepository.GetTransactionById(id);
            return transactionDto?.ToDto();
        }

        public void UpdateTransaction(TransactionDto transaction)
        {
            var existingTransaction = transactionRepository.GetTransactionById(transaction.Id);
            if (existingTransaction != null)
            {
                existingTransaction.Amount = transaction.Amount;
                existingTransaction.Description = transaction.Description;
                existingTransaction.PaymentMethod = transaction.PaymentMethod;
                existingTransaction.Date = transaction.Date;
                transactionRepository.UpdateTransaction(existingTransaction);
            }
        }
    }
}
