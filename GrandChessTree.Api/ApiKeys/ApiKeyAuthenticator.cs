using System.Net;
using GrandChessTree.Api.Database;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using GrandChessTree.Shared.ApiKeys;
using GrandChessTree.Shared.Api;

namespace GrandChessTree.Api.ApiKeys;

public class ApiKeyAuthenticator
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ApiKeyAuthenticator> _logger;
    private readonly IMemoryCache _memoryCache;

    public ApiKeyAuthenticator(ILogger<ApiKeyAuthenticator> logger, IMemoryCache memoryCache,
        ApplicationDbContext dbContext)
    {
        _logger = logger;
        _memoryCache = memoryCache;
        _dbContext = dbContext;
    }

    public async Task<CachedApiKey?> GetApiKey(HttpContext httpContext,
       CancellationToken cancellationToken = default)
    {
        var k = httpContext.GetApiKey();
        if (string.IsNullOrEmpty(k))
        {
            return null;
        }

        var hashedApiKey = ApiKeyGenerator.HashApiKey(k);
        if (_memoryCache.TryGetValue(AuthCacheKeys.ApiKeyCacheToken(hashedApiKey), out var cacheObj) &&
            cacheObj is CachedApiKey apiKey)
        {
            if (!apiKey.IsValid(httpContext))
            {
                return null;
            }
            return apiKey;
        }

        var key = await _dbContext.ApiKeys
            .AsNoTracking()
            .Where(k => k.Id == hashedApiKey)
            .Select(k => new CachedApiKey
            {
                Id = k.Id,
                ApiKeyTail = k.ApiKeyTail,
                AccountId = k.AccountId,
                Role = k.Role,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (key == null)
        {
            return null;
        }

        _memoryCache.Set(AuthCacheKeys.ApiKeyCacheToken(hashedApiKey), key, AuthCacheKeys.LongExpiry);
        if (!key.IsValid(httpContext))
        {
            return null;
        }
        return key;
    }


    public async Task<bool> IsAuthenticated(HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        var k = httpContext.GetApiKey();
        if (string.IsNullOrEmpty(k))
        {
            return false;
        }

        var hashedApiKey = ApiKeyGenerator.HashApiKey(k);
        if (_memoryCache.TryGetValue(AuthCacheKeys.ApiKeyCacheToken(hashedApiKey), out var cacheObj) &&
            cacheObj is CachedApiKey apiKey)
        {
            return apiKey.IsValid(httpContext);
        }

        var key = await _dbContext.ApiKeys
            .AsNoTracking()
            .Where(k => k.Id == hashedApiKey)
            .Select(k => new CachedApiKey
            {
                Id = k.Id,
                ApiKeyTail = k.ApiKeyTail,
                AccountId = k.AccountId,
                Role = k.Role,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (key == null)
        {
            return false;
        }

        _memoryCache.Set(AuthCacheKeys.ApiKeyCacheToken(hashedApiKey), key, AuthCacheKeys.LongExpiry);

        return key.IsValid(httpContext);
    }

    public async Task<bool> IsAuthenticated(HttpContext httpContext, AccountRole role,
    CancellationToken cancellationToken = default)
    {
        var k = httpContext.GetApiKey();
        if (string.IsNullOrEmpty(k))
        {
            return false;
        }

        var hashedApiKey = ApiKeyGenerator.HashApiKey(k);
        if (_memoryCache.TryGetValue(AuthCacheKeys.ApiKeyCacheToken(hashedApiKey), out var cacheObj) &&
            cacheObj is CachedApiKey apiKey)
        {
            return apiKey.IsValid(httpContext) && apiKey.Role >= role;
        }

        var key = await _dbContext.ApiKeys
            .AsNoTracking()
            .Where(k => k.Id == hashedApiKey)
            .Select(k => new CachedApiKey
            {
                Id = k.Id,
                ApiKeyTail = k.ApiKeyTail,
                AccountId = k.AccountId,
                Role = k.Role,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (key == null)
        {
            return false;
        }

        _memoryCache.Set(AuthCacheKeys.ApiKeyCacheToken(hashedApiKey), key, AuthCacheKeys.LongExpiry);

        return key.IsValid(httpContext) && key.Role >= role;
    }
}


public class ApiKeyAuthorizeAttribute : TypeFilterAttribute
{
    public ApiKeyAuthorizeAttribute()
        : base(typeof(ApiKeyAuthFilter))
    {
    }
}

public class ApiKeyAuthFilter : IAsyncAuthorizationFilter
{
    private readonly ApiKeyAuthenticator _authorizer;
    private readonly ILogger<ApiKeyAuthFilter> _logger;

    public ApiKeyAuthFilter(ILogger<ApiKeyAuthFilter> logger, ApiKeyAuthenticator authorizer)
    {
        _logger = logger;
        _authorizer = authorizer;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        try
        {
            var isAuthenticated =
                await _authorizer.IsAuthenticated(context.HttpContext, context.HttpContext.RequestAborted);
            if (isAuthenticated)
            {
                return;
            }

            _logger.LogWarning("Unauthenticated");
            context.Result = new StatusCodeResult((int)HttpStatusCode.Unauthorized);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authorization filter error");
            context.Result = new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }
}


public class AdminAuthorizeAttribute : TypeFilterAttribute
{
    public AdminAuthorizeAttribute()
        : base(typeof(AdminAuthFilter))
    {
    }
}

public class AdminAuthFilter : IAsyncAuthorizationFilter
{
    private readonly ApiKeyAuthenticator _authorizer;
    private readonly ILogger<ApiKeyAuthFilter> _logger;

    public AdminAuthFilter(ILogger<ApiKeyAuthFilter> logger, ApiKeyAuthenticator authorizer)
    {
        _logger = logger;
        _authorizer = authorizer;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        try
        {
            var isAuthenticated =
                await _authorizer.IsAuthenticated(context.HttpContext, context.HttpContext.RequestAborted);
            if (isAuthenticated)
            {
                return;
            }

            _logger.LogWarning("Unauthenticated");
            context.Result = new StatusCodeResult((int)HttpStatusCode.Unauthorized);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authorization filter error");
            context.Result = new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }
}