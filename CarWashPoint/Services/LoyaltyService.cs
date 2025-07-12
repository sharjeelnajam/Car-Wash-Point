namespace CarWashPoint.Services
{
    public class LoyaltyService
    {
        private readonly CashGenicService _cashGenicService;

        public LoyaltyService(CashGenicService cashGenicService)
        {
            _cashGenicService = cashGenicService;
        }

        public async Task<bool> CheckFreeWashEligibility(string customerId)
        {
            var logs = await _cashGenicService.GetTransactionLogsAsync();
            var paidWashes = logs.Logs.Count(log => log.Value > 0); // Filter paid transactions
            return paidWashes >= 5; // Free after 5 paid washes
        }
    }
}
