using Newtonsoft.Json;

namespace CarWashPoint.Responses.Logs.TransactionListDateTime;

public class TransactionListDateTime
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

    [JsonProperty("uid")]
    public string Uid { get; set; }
}

public class Summary
{
    [JsonProperty("cashPaidIn")]
    public decimal CashPaidIn { get; set; }

    [JsonProperty("cashPaidOut")]
    public decimal CashPaidOut { get; set; }

    [JsonProperty("paymentsBalance")]
    public decimal PaymentsBalance { get; set; }

    [JsonProperty("paymentsCancelled")]
    public decimal PaymentsCancelled { get; set; }

    [JsonProperty("paymentsRequested")]
    public decimal PaymentsRequested { get; set; }

    [JsonProperty("refundsOut")]
    public decimal RefundsOut { get; set; }

    [JsonProperty("totalRequested")]
    public decimal TotalRequested { get; set; }
}
