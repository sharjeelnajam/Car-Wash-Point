using Newtonsoft.Json;

namespace CarWashPoint.Responses.Logs.TransactionList
{
    public class TransactionList
    {
        [JsonProperty("responseCode")]
        public int ResponseCode { get; set; }

        [JsonProperty("responseLogCount")]
        public int ResponseLogCount { get; set; }

        [JsonProperty("transactionLogs")]
        public List<TransactionLog> Logs { get; set; }
    }

    public class TransactionLog
    {
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("transactionID")]
        public string TransactionId { get; set; }

        [JsonProperty("transactionType")]
        public string TransactionType { get; set; }

        [JsonProperty("transactionValue")]
        public int Value { get; set; } // Penny value
    }
}
