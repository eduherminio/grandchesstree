using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GrandChessTree.Api.D10Search
{

    public class SearchResult
    {
        [Column("nodes")]
        [JsonPropertyName("nodes")]
        public ulong Nodes { get; set; }
        [Column("captures")]
        [JsonPropertyName("captures")]
        public ulong Captures { get; set; }
        [Column("enpassants")]
        [JsonPropertyName("enpassants")]
        public ulong Enpassants { get; set; }
        [Column("castles")]
        [JsonPropertyName("castles")]
        public ulong Castles { get; set; }
        [Column("promotions")]
        [JsonPropertyName("promotions")]
        public ulong Promotions { get; set; }
        [Column("direct_checks")]
        [JsonPropertyName("direct_checks")]
        public ulong DirectChecks { get; set; }
        [Column("single_discovered_check")]
        [JsonPropertyName("single_discovered_check")]
        public ulong SingleDiscoveredCheck { get; set; }
        [Column("direct_discovered_check")]
        [JsonPropertyName("direct_discovered_check")]
        public ulong DirectDiscoveredCheck { get; set; }
        [Column("double_discovered_check")]
        [JsonPropertyName("double_discovered_check")]
        public ulong DoubleDiscoveredCheck { get; set; }
        [Column("direct_checkmate")]
        [JsonPropertyName("direct_checkmate")]
        public ulong DirectCheckmate { get; set; }
        [Column("single_discovered_checkmate")]
        [JsonPropertyName("single_discovered_checkmate")]
        public ulong SingleDiscoveredCheckmate { get; set; }
        [Column("direct_discoverd_checkmate")]
        [JsonPropertyName("direct_discoverd_checkmate")]
        public ulong DirectDiscoverdCheckmate { get; set; }
        [Column("double_discoverd_checkmate")]
        [JsonPropertyName("double_discoverd_checkmate")]
        public ulong DoubleDiscoverdCheckmate { get; set; }
    }
}
