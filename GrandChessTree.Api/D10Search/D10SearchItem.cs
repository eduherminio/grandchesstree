using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrandChessTree.Api.D10Search
{
    [Table("d10_search_items")]
    public class D10SearchItem
    {
        [Key]
        [Column("id")]
        public required ulong Id { get; set; }

        [Column("available_at")]
        public long AvailableAt { get; set; } = 0;

        [Column("pass_count")]
        public uint PassCount { get; set; } = 0;

        [Column("confirmed")]
        public bool Confirmed { get; set; }

        public virtual List<D10SearchTask> SearchTasks { get; set; } = new();
    }
}
