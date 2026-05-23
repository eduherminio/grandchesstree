using System.Security.Cryptography;
using System.Text;
using GrandChessTree.Api.Accounts;
using GrandChessTree.Shared.Api;

namespace GrandChessTree.Api.ApiKeys;

public class CachedApiKey
{
    public required string Id { get; set; }
    public required long AccountId { get; set; }
    public required string ApiKeyTail { get; set; }
    public required AccountRole Role { get; set; }
    public bool IsValid(HttpContext httpContext)
    {
        var apiKey = httpContext.GetApiKey();

        if (apiKey == null)
        {
            return false;
        }

        if (!apiKey.EndsWith(ApiKeyTail))
        {
            return false;
        }

        if (HashApiKey(apiKey) != Id)
        {
            return false;
        }

        return true;
    }

    public static string HashApiKey(string input)
    {
        using var sha256 = SHA256.Create();

        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        var stringBuilder = new StringBuilder();
        foreach (var b in hashBytes)
        {
            stringBuilder.Append(b.ToString("x2"));
        }

        return stringBuilder.ToString();
    }
}
