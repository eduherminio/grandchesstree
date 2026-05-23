using Microsoft.Extensions.Caching.Memory;

namespace GrandChessTree.Api.ApiKeys;


public static class AuthCacheKeys
{
     public static readonly MemoryCacheEntryOptions LongExpiry = new()
        { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) };


    public static string ApiKeyCacheToken(string apiKey)
    {
        return $"auth key:{apiKey}";
    }
}