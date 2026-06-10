using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.EventSourcing.Memory;
using ACC.Identity.Application.Ports.Communication;
using ACC.Identity.Application.Ports.ReadModels.User;
using ACC.Identity.Application.Ports.Security;
using ACC.Identity.Application.UseCases.AuthenticateUser;
using ACC.Identity.Application.UseCases.RegisterUser;
using ACC.Identity.Application.UseCases.ResendVerification;
using ACC.Identity.Application.UseCases.VerifyEmail;
using ACC.Identity.Domain.Aggregates;
using ACC.Identity.Infrastructure.Communication;
using ACC.Identity.Infrastructure.Endpoints;
using ACC.Identity.Infrastructure.ReadModels.User;
using ACC.Identity.Infrastructure.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACC.Identity;

public static class ModuleRegistration
{
    public static IServiceCollection AddIdentity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddIdentityApplication();

        var persistenceProvider = configuration["Persistence:Provider"] ?? "Memory";

        _ = persistenceProvider.Trim().ToUpperInvariant() switch
        {
            "MEMORY" => services.AddIdentityMemoryPersistence(),
            "POSTGRES" => throw new NotSupportedException(
                "Postgres persistence has not been implemented for Identity yet."),
            _ => throw new InvalidOperationException(
                $"Unsupported persistence provider '{persistenceProvider}' for Identity.")
        };

        services.AddIdentityEmail(configuration);

        return services;
    }

    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        services.AddTransient(provider => new EventSourcedRepository<User>(
            provider.GetRequiredService<IEventStore>(),
            User.Rehydrate));
        services.AddTransient<UserProjection>();
        services.AddTransient<AuthenticateUserHandler>();
        services.AddTransient<RegisterUserHandler>();
        services.AddTransient<ResendVerificationHandler>();
        services.AddTransient<VerifyEmailHandler>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IEmailVerificationTokenGenerator, EmailVerificationTokenGenerator>();
        services.AddSingleton<IAuthenticationTokenIssuer, JwtAuthenticationTokenIssuer>();

        return services;
    }

    public static IServiceCollection AddIdentityMemoryPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IEventStore, InMemoryEventStore>();
        services.AddSingleton<IUserStore, InMemoryUserStore>();

        return services;
    }

    public static IServiceCollection AddIdentityEmail(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = configuration["Email:Provider"] ?? "Console";

        return provider.Trim().ToUpperInvariant() switch
        {
            "RESEND" => services.AddIdentityResendEmail(),
            _ => services.AddIdentityConsoleEmail()
        };
    }

    public static IServiceCollection AddIdentityConsoleEmail(this IServiceCollection services)
    {
        services.AddSingleton<IIdentityEmailSender, ConsoleIdentityEmailSender>();

        return services;
    }

    public static IServiceCollection AddIdentityResendEmail(this IServiceCollection services)
    {
        services.AddHttpClient<IIdentityEmailSender, ResendIdentityEmailSender>();

        return services;
    }

    public static IEndpointRouteBuilder MapIdentity(this IEndpointRouteBuilder endpoints)
    {
        var identity = endpoints.MapGroup("/identity");

        identity.MapIdentityEndpoints();

        return endpoints;
    }
}
