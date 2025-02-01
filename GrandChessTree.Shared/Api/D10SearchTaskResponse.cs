using System.Text.Json.Serialization;

namespace GrandChessTree.Shared.Api;
public class D10SearchTaskResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("search_item_id")]
    public ulong SearchItemId { get; set; }
}
