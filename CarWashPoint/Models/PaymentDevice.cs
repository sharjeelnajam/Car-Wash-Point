using Newtonsoft.Json;

namespace CarWashPoint.Models
{
    public class PaymentDevice
    {
        [JsonProperty("firmwareVersion")]
        public string FirmwareVersion { get; set; }

        [JsonProperty("isConnected")]
        public bool IsConnected { get; set; }

        [JsonProperty("paymentType")]
        public string PaymentType { get; set; } // "Bills" or "Coins"

        [JsonProperty("cashboxValue")]
        public int CashboxValue { get; set; } // Penny value (e.g., $100 = 10000)

        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; }

        [JsonProperty("storedValue")]
        public int StoredValue { get; set; }

        [JsonProperty("denominations")]
        public List<Denomination> Denominations { get; set; }
    }


}
