using ACC.Authority.Application.Policies;
using ACC.Authority.Application.Ports.AccountingSubject;
using ACC.Authority.Application.Ports.Identity;
using ACC.Authority.Application.Ports.ReadModels.RoleAssignment;
using ACC.Authority.Application.UseCases.AssignRole;
using ACC.Authority.Application.UseCases.EstablishInitialOwner;
using ACC.Authority.Application.UseCases.RevokeRole;
using ACC.Authority.Application.UseCases.ViewUserRoles;
using ACC.Authority.Domain.Aggregates;
using ACC.Authority.Domain.Powers;
using ACC.Authority.Infrastructure.Adapters.AccountingSubject;
using ACC.Authority.Infrastructure.Adapters.Identity;
using ACC.Authority.Infrastructure.Endpoints;
using ACC.Authority.Infrastructure.ReadModels.RoleAssignment;
using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.EventSourcing.Memory;
using ACC.BuildingBlocks.EventSourcing.Postgres;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACC.Authority;

public static class ModuleRegistration
{
    public static IServiceCollection AddAuthority(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthorityApplication();

        var persistenceProvider = configuration["Persistence:Provider"] ?? "Memory";

        return persistenceProvider.Trim().ToUpperInvariant() switch
        {
            "POSTGRES" => services.AddAuthorityPostgresPersistence(),
            "MEMORY" => services.AddAuthorityMemoryPersistence(),
            _ => throw new InvalidOperationException(
                $"Unsupported persistence provider '{persistenceProvider}' for Authority.")
        };
    }

    public static IServiceCollection AddAuthorityApplication(this IServiceCollection services)
    {
        services.AddTransient(provider => new EventSourcedRepository<RoleAssignment>(
            provider.GetRequiredService<IEventStore>(),
            RoleAssignment.Rehydrate));
        services.AddTransient<RoleAssignmentProjection>();
        services.AddTransient<AssignRoleHandler>();
        services.AddTransient<EstablishInitialOwnerHandler>();
        services.AddTransient<RevokeRoleHandler>();
        services.AddTransient<ViewUserRolesHandler>();
        services.AddTransient<IRecognizedAccountingSubjectPort, RecognizedAccountingSubjectAdapter>();
        services.AddTransient<IRecognizedUserPort, RecognizedUserAdapter>();
        services.AddSingleton<RolePowerPolicy>();
        services.AddTransient<IAuthorityPolicy, AuthorityPolicy>();

        return services;
    }

    public static IServiceCollection AddAuthorityMemoryPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IEventStore, InMemoryEventStore>();
        services.AddSingleton<IRoleAssignmentStore, InMemoryRoleAssignmentStore>();

        return services;
    }

    public static IServiceCollection AddAuthorityPostgresPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IEventStore, PostgresEventStore>();
        services.AddSingleton<IRoleAssignmentStore, PostgresRoleAssignmentStore>();

        return services;
    }

    public static IEndpointRouteBuilder MapAuthority(this IEndpointRouteBuilder endpoints)
    {
        var authority = endpoints.MapGroup("/authority");

        authority.MapAuthorityEndpoints();

        return endpoints;
    }
}
