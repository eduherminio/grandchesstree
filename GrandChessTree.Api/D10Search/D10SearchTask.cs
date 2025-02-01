using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrandChessTree.Api.D10Search
{

    [Table("d10_search_tasks")]
    public class D10SearchTask
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("SearchItem")]
        [Column("search_item_id")]
        public ulong SearchItemId { get; set; }

        public virtual D10SearchItem SearchItem { get; set; } = null!;

        [Column("started_at")]
        public long StartedAt { get; set; } = 0;

        [Column("finished_at")]
        public long FinishedAt { get; set; } = 0;

        [Column("nps")]
        public float Nps { get; set; }
        [Column("nodes")]
        public ulong Nodes { get; set; }
        [Column("captures")]
        public ulong Captures { get; set; }
        [Column("enpassants")]
        public ulong Enpassant { get; set; }
        [Column("castles")]
        public ulong Castles { get; set; }
        [Column("promotions")]
        public ulong Promotions { get; set; }
        [Column("direct_checks")]
        public ulong DirectCheck { get; set; }
        [Column("single_discovered_check")]
        public ulong SingleDiscoveredCheck { get; set; }
        [Column("direct_discovered_check")]
        public ulong DirectDiscoveredCheck { get; set; }
        [Column("double_discovered_check")]
        public ulong DoubleDiscoveredCheck { get; set; }
        [Column("direct_checkmate")]
        public ulong DirectCheckmate { get; set; }
        [Column("single_discovered_checkmate")]
        public ulong SingleDiscoveredCheckmate { get; set; }
        [Column("direct_discoverd_checkmate")]
        public ulong DirectDiscoverdCheckmate { get; set; }
        [Column("double_discoverd_checkmate")]
        public ulong DoubleDiscoverdCheckmate { get; set; }
    }
}
