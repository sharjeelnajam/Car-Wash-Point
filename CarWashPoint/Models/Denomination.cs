using Newtonsoft.Json;

namespace CarWashPoint.Models
{
    public class Denomination
    {
        [JsonProperty("value")]
        public int Value { get; set; } // Penny value (e.g., $5 = 500)

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("storedLevel")]
        public int StoredLevel { get; set; } // Current count in payout store
    }
}
