using Newtonsoft.Json;

namespace CarWashPoint.Responses.TransactionLog;

public class TransactionLog
{
    [JsonProperty("responseCode")]
    public int ResponseCode { get; set; }

    [JsonProperty("responseData")]
    public string ResponseData { get; set; }

    [JsonProperty("responseLogList")]
    public List<LogEntry> ResponseLogList { get; set; }

    [JsonProperty("summary")]
    public Summary Summary { get; set; }
}

public class LogEntry
{
    [JsonProperty("timestamp")]
    public string Timestamp { get; set; }

    [JsonProperty("transactionType")]
    public string TransactionType { get; set; }

    [JsonProperty("transactionValue")]
    public decimal TransactionValue { get; set; }

    [JsonProperty("typeCode")]
    public int TypeCode { get; set; }
}

public class Summary
{
    [JsonProperty("complete")]
    public bool Complete { get; set; }

    [JsonProperty("payinBalance")]
    public decimal PayinBalance { get; set; }

    [JsonProperty("payinReceived")]
    public decimal PayinReceived { get; set; }

    [JsonProperty("payinRequested")]
    public decimal PayinRequested { get; set; }

    [JsonProperty("payoutBalance")]
    public decimal PayoutBalance { get; set; }

    [JsonProperty("payoutMade")]
    public decimal PayoutMade { get; set; }

    [JsonProperty("payoutRequested")]
    public decimal PayoutRequested { get; set; }

    [JsonProperty("transactionBalance")]
    public decimal TransactionBalance { get; set; }

    [JsonProperty("uid")]
    public string Uid { get; set; }
}
