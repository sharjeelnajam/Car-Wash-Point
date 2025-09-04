using CarWashApp;
using CarWashApp.Models;

namespace TransactionWashApp.Repositories
{
    public interface ITransactionRepository
    {
        void AddTransaction(Transaction transaction);
        Transaction GetTransactionById(int id);
        IEnumerable<Transaction> GetAllTransactions();
        void UpdateTransaction(Transaction transaction);
        void DeleteTransaction(int id);
    }
    public class TransactionRepository(CarWashAppDbContext context) : ITransactionRepository
    {
        public void AddTransaction(Transaction transaction)
        {
            context.Add(transaction);
            context.SaveChanges();
        }

        public void DeleteTransaction(int id)
        {
            var transaction = context.Transactions.FirstOrDefault(c => c.Id == id);
            if (transaction != null)
            {
                context.Transactions.Remove(transaction);
                context.SaveChanges();
            }
        }

        public IEnumerable<Transaction> GetAllTransactions()
        {
            return context.Transactions.ToList();
        }

        public Transaction GetTransactionById(int id)
        {
            return context.Transactions.FirstOrDefault(c => c.Id == id);
        }

        public void UpdateTransaction(Transaction transaction)
        {
            context.Update(transaction);
            context.SaveChanges();
        }
    }
}
