
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Sardanapal.Share;

public static class SharedConfigurations
{
    public static void AddAutoMapper(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddScoped<IConfigurationProvider>(sp =>
        {
            return new MapperConfiguration(config =>
            {
                config.AddProfiles(assemblies
                    .SelectMany(x => x.GetTypes()
                        .Where(t => t.IsSubclassOf(typeof(Profile)) && !t.IsAbstract)
                        .Select(t => t.GetConstructors().First().Invoke(null) as Profile)
                        .ToArray()));
            });
        });

        services.AddScoped<IMapper>(sp =>
        {
            return new Mapper(sp.GetRequiredService<IConfigurationProvider>(), sp.GetService);
        });
    }
}
