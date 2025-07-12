using Newtonsoft.Json;

namespace CarWashPoint
{
    public class TransactionLog
    {
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("transactionID")]
        public string TransactionId { get; set; }

        [JsonProperty("transactionValue")]
        public int Value { get; set; } // Penny value
    }
}
