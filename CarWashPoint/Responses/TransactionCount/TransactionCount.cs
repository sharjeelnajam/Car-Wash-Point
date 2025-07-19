using Newtonsoft.Json;

namespace CarWashPoint.Responses.TransactionCount;

public class BasicResponse
{
    [JsonProperty("indexSize")]
    public int IndexSize { get; set; }

    [JsonProperty("responseCode")]
    public int ResponseCode { get; set; }

    [JsonProperty("responseLogCount")]
    public int ResponseLogCount { get; set; }
}
