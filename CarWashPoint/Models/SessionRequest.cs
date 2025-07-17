using Newtonsoft.Json;

namespace CarWashPoint.Models
{
    public class SessionRequest
    {
        [JsonProperty("request")]
        public string Request { get; set; } // "PayAmount", "RefundAmount", etc.

        [JsonProperty("value")]
        public int? Value { get; set; } // Optional (e.g., amount in pennies)
    }
}
