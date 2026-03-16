using Microsoft.Extensions.DependencyInjection;

namespace CoreERP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
