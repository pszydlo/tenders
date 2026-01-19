using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Tenders.Application.CommandQuery;

namespace Tenders.Application;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<RequestHandler>();
        return services;
    }
}
