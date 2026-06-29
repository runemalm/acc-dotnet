using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace ACC.BuildingBlocks.AspNetCore.Errors;

public static class ErrorHandlingRegistration
{
    public static IServiceCollection AddExpectedExceptionHandling(
        this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<ExpectedExceptionHandler>();

        return services;
    }
}
