using Newtonsoft.Json;

namespace CarWashPoint.Models
{
    public class SystemData
    {
        [JsonProperty("systemVersion")]
        public string SystemVersion { get; set; }

        [JsonProperty("lowFloatWarnPercent")]
        public int LowFloatWarnPercent { get; set; }

        [JsonProperty("maxPayoutLevel")]
        public int MaxPayoutLevel { get; set; }

        [JsonProperty("paymentDevices")]
        public List<PaymentDevice> PaymentDevices { get; set; }
    }
}
