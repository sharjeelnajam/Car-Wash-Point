using Newtonsoft.Json;

namespace CarWashPoint.Models
{
    public class CashStatus
    {

        [JsonProperty("coinsStored")]
        public int CoinsStored { get; set; }

        [JsonProperty("coinsCashbox")]
        public int CoinsCashbox { get; set; }

        [JsonProperty("billsStored")]
        public int BillsStored { get; set; }

        [JsonProperty("billsCashbox")]
        public int BillsCashbox { get; set; }

        [JsonProperty("collectableCoinValue")]
        public int CollectableCoinValue { get; set; }

        [JsonProperty("collectableBillValue")]
        public int CollectableBillValue { get; set; }

        [JsonProperty("totalSystemCash")]
        public int TotalSystemCash { get; set; }

        [JsonProperty("totalCollectableCash")]
        public int TotalCollectableCash { get; set; }
    }

}
