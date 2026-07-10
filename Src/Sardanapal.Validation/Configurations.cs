
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Sardanapal.Validation;

public static class Configurations
{
    public static IServiceCollection AddFromAssemblyContaining<T>(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<T>();
        return services;
    }
}
