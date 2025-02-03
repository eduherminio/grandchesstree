namespace GrandChessTree.Api.ApiKeys;

public static class HttpContextExtensions
{
    public static readonly string ApiHeaderKey = "X-API-Key";

    public static string? GetApiKey(this HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(ApiHeaderKey, out var values))
        {
            return null;
        }

        var apiKey = values.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new Exception("Api key was empty.");
        }

        return apiKey;
    }
}