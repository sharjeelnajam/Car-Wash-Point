using Newtonsoft.Json;

namespace CarWashPoint.Models
{
    public class Denomination
    {
        [JsonProperty("country")]
        public string Country { get; set; }         // e.g., "EUR"

        [JsonProperty("value")]
        public int Value { get; set; }              // e.g., 500 = €5.00

        [JsonProperty("storedLevel")]
        public int StoredLevel { get; set; }        // Quantity of units

        [JsonProperty("storedValue")]
        public int StoredValue { get; set; }        // Total value in cents
    }

}
