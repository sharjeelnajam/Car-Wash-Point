using Newtonsoft.Json;

namespace CarWashPoint
{
    public class LogResponse
    {
        [JsonProperty("transactionLogs")]
        public List<TransactionLog> Logs { get; set; }
    }
}
