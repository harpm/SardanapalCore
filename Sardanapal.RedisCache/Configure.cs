using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Sardanapal.RedisCache;

public static class Configure
{
    public static void AddSardanapalRedisCache(this IServiceCollection services, string connString)
    {
        services.AddSingleton<IConnectionMultiplexer>(opt => ConnectionMultiplexer.ConnectAsync(connString).GetAwaiter().GetResult());
    }
}