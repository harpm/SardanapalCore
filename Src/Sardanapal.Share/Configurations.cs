
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Sardanapal.Share;

public static class SharedConfigurations
{
    public static void AddAutoMapper(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddSingleton<IConfigurationProvider>(sp =>
        {
            return new MapperConfiguration(config =>
            {
                var profiles = assemblies
                    .SelectMany(x => x.GetTypes())
                    .Where(t => t.IsSubclassOf(typeof(Profile)) && !t.IsAbstract)
                    .Select(t => Activator.CreateInstance(t) as Profile)
                    .Where(p => p != null)
                    .ToArray();

                config.AddProfiles(profiles);
            });
        });

        services.AddSingleton<IMapper>(sp =>
        {
            return new Mapper(sp.GetRequiredService<IConfigurationProvider>(), sp.GetService);
        });
    }
}
