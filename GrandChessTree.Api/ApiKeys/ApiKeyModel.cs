using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using GrandChessTree.Api.Accounts;
using GrandChessTree.Shared.Api;

namespace GrandChessTree.Api.ApiKeys;


[Table("api_keys")]
public class ApiKeyModel
{

    [Key]
    [Column("id")]
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [Column("created_at")]
    [JsonPropertyName("created_at")]
    public required long CreatedAt { get; set; }

    [Column("apikey_tail")]
    [JsonPropertyName("apikey_tail")]
    public required string ApiKeyTail { get; set; } = string.Empty;

    [Column("role")]
    [JsonPropertyName("role")]
    public required AccountRole Role { get; set; } = AccountRole.User;

    [JsonPropertyName("account_id")]
    [ForeignKey("Account")]
    [Column("account_id")]
    public required long AccountId { get; set; }

    [JsonIgnore] public AccountModel Account { get; set; } = default!;

    #region Versioning

    [Timestamp] public uint Version { get; set; }

    #endregion
}