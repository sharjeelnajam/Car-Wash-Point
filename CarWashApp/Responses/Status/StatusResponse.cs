using System.Text.Json.Serialization;

namespace CarWashApp.Responses.Status
{
    public class StatusResponse
    {
        [JsonPropertyName("cashStatus")]
        public CashStatus CashStatus { get; set; }

        [JsonPropertyName("events")]
        public List<Event> Events { get; set; }

        [JsonPropertyName("faceDeviceState")]
        public string FaceDeviceState { get; set; }
    }

    public class CashStatus
    {
        [JsonPropertyName("billsCashbox")]
        public int BillsCashbox { get; set; }

        [JsonPropertyName("billsStored")]
        public int BillsStored { get; set; }

        [JsonPropertyName("coinsCashbox")]
        public int CoinsCashbox { get; set; }

        [JsonPropertyName("coinsStored")]
        public int CoinsStored { get; set; }

        [JsonPropertyName("collectableBillValue")]
        public int CollectableBillValue { get; set; }

        [JsonPropertyName("collectableCoinValue")]
        public int CollectableCoinValue { get; set; }

        [JsonPropertyName("totalCollectableCash")]
        public int TotalCollectableCash { get; set; }

        [JsonPropertyName("totalSystemCash")]
        public int TotalSystemCash { get; set; }
    }

    public class Event
    {
        [JsonPropertyName("errorCondition")]
        public string ErrorCondition { get; set; }

        [JsonPropertyName("event")]
        public string EventType { get; set; }

        [JsonPropertyName("lowFloatLevelWarning")]
        public bool LowFloatLevelWarning { get; set; }

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }

        [JsonPropertyName("transaction")]
        public Transaction Transaction { get; set; }

        [JsonPropertyName("value")]
        public int Value { get; set; }
    }

    public class Transaction
    {
        [JsonPropertyName("cash_in")]
        public int CashIn { get; set; }

        [JsonPropertyName("cash_out")]
        public int CashOut { get; set; }

        [JsonPropertyName("request_amount")]
        public int RequestAmount { get; set; }

        [JsonPropertyName("transaction_id")]
        public string TransactionId { get; set; }

        [JsonPropertyName("transaction_type")]
        public string TransactionType { get; set; }
    }

    public enum EventType
    {
        SystemStartup,
        BillOutOfService,
        CoinOutOfService,
        BillCoinOutOfService,
        Idle,
        NewPayment,
        Waiting,
        PayInStarted,
        Payment,
        PayInComplete,
        Change,
        PaymentComplete,
        NewRefund,
        Refund,
        RefundComplete,
        NewRefill,
        Refill,
        RefillPaidIn,
        RefillComplete,
        NewCollect,
        Collect,
        CollectComplete,
        NewEmpty,
        Empty,
        EmptyComplete,
        CancelPayment,
        CancelComplete,
        StoppingPayment,
        StopComplete,
        StopCancelled,
        InsufficientFloat,
        PayoutFailed,
        RestoreTransaction,
        BillReject,
        PayOutError,
        WaitingForAge,
        AgeVerifyResult
    }
}
