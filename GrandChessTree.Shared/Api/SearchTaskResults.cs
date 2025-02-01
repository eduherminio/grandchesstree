using System.Text.Json.Serialization;

namespace GrandChessTree.Shared.Api;

public class SearchTaskResultBatch
{
    [JsonPropertyName("results")]
    public SearchTaskResults[] Results { get; set; }
}
public class SearchTaskResults
{
    [JsonPropertyName("id")]
    public required int Id { get; set; }

    [JsonPropertyName("search_item_id")]
    public required ulong SearchItemId { get; set; }

    [JsonPropertyName("nodes")]
    public required ulong Nodes { get; set; }
    [JsonPropertyName("captures")]
    public required ulong Captures { get; set; }
    [JsonPropertyName("enpassants")]
    public required ulong Enpassant { get; set; }
    [JsonPropertyName("castles")]
    public required ulong Castles { get; set; }
    [JsonPropertyName("promotions")]
    public required ulong Promotions { get; set; }
    [JsonPropertyName("direct_checks")]
    public required ulong DirectCheck { get; set; }
    [JsonPropertyName("single_discovered_check")]
    public required ulong SingleDiscoveredCheck { get; set; }
    [JsonPropertyName("direct_discovered_check")]
    public required ulong DirectDiscoveredCheck { get; set; }
    [JsonPropertyName("double_discovered_check")]
    public required ulong DoubleDiscoveredCheck { get; set; }
    [JsonPropertyName("direct_checkmate")]
    public required ulong DirectCheckmate { get; set; }
    [JsonPropertyName("single_discovered_checkmate")]
    public required ulong SingleDiscoveredCheckmate { get; set; }
    [JsonPropertyName("direct_discoverd_checkmate")]
    public required ulong DirectDiscoverdCheckmate { get; set; }
    [JsonPropertyName("double_discoverd_checkmate")]
    public required ulong DoubleDiscoverdCheckmate { get; set; }


}
