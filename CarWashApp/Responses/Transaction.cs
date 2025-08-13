using Newtonsoft.Json;

namespace CarWashApp.Responses
{
    public class Transaction
    {
        [JsonProperty("transactionType")]
        public string TransactionType { get; set; }  // e.g., "PayAmount"

        [JsonProperty("requestAmount")]
        public int RequestAmount { get; set; }

        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty("cashIn")]
        public int CashIn { get; set; }

        [JsonProperty("cashOut")]
        public int CashOut { get; set; }

        [JsonProperty("timeStamp")]
        public DateTime Timestamp { get; set; }
    }

}
