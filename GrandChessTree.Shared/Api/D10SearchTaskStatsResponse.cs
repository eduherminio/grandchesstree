using System.Text.Json.Serialization;

namespace GrandChessTree.Shared.Api;

public class D10SearchTaskStatsResponse
{
    [JsonPropertyName("nps")]
    public float Nps { get; set; }

    [JsonPropertyName("tpm")]
    public float Tpm { get; set; }
}