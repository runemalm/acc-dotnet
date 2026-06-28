using Microsoft.AspNetCore.Diagnostics;

namespace ACC.Host.Errors;

public static class ErrorHandlingRegistration
{
    public static IServiceCollection AddHostErrorHandling(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<UnhandledExceptionHandler>();

        return services;
    }
}
