using Newtonsoft.Json;

namespace CarWashPoint
{
    public class SessionResponse
    {
        [JsonProperty("responseCode")]
        public int ResponseCode { get; set; } // 0 = success

        [JsonProperty("responseData")]
        public string ResponseData { get; set; } // Transaction ID or status message
    }
}
