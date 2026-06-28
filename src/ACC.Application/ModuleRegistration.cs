using ACC.Application.Application.UseCases.CompleteOnboarding;
using ACC.Application.Infrastructure.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ACC.Application;

public static class ModuleRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddTransient<CompleteOnboardingHandler>();

        return services;
    }

    public static IEndpointRouteBuilder MapApplication(this IEndpointRouteBuilder endpoints)
    {
        var onboarding = endpoints.MapGroup("/onboarding").RequireAuthorization();
        onboarding.MapOnboardingEndpoints();

        return endpoints;
    }
}
