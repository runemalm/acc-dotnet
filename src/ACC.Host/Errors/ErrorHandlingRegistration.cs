using ACC.BuildingBlocks.AspNetCore.Errors;
using Microsoft.AspNetCore.Diagnostics;

namespace ACC.Host.Errors;

public static class ErrorHandlingRegistration
{
    public static IServiceCollection AddHostErrorHandling(this IServiceCollection services)
    {
        services.AddExpectedExceptionHandling();
        services.AddExceptionHandler<UnexpectedExceptionHandler>();

        return services;
    }
}
