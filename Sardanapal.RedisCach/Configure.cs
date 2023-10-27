using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Sardanapal.RedisCache;

public static class Configure
{
    public static void AddSardanapalRedisCach(this IServiceCollection services, string connString)
    {
        services.AddSingleton<IConnectionMultiplexer>(opt => ConnectionMultiplexer.Connect(connString));
    }
}