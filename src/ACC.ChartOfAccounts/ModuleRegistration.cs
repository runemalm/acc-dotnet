using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.EventSourcing.Memory;
using ACC.BuildingBlocks.EventSourcing.Postgres;
using ACC.ChartOfAccounts.Application.Ports.AccountingSubject;
using ACC.ChartOfAccounts.Application.Ports.Authority;
using ACC.ChartOfAccounts.Application.Ports.ReadModels.ChartOfAccounts;
using ACC.ChartOfAccounts.Application.Ports.Templates;
using ACC.ChartOfAccounts.Application.UseCases.AddAccount;
using ACC.ChartOfAccounts.Application.UseCases.AdoptChartOfAccounts;
using ACC.ChartOfAccounts.Application.UseCases.DeactivateAccount;
using ACC.ChartOfAccounts.Application.UseCases.ReactivateAccount;
using ACC.ChartOfAccounts.Application.UseCases.ViewChartOfAccounts;
using ACC.ChartOfAccounts.Application.UseCases.ViewChartOfAccountsTemplates;
using ACC.ChartOfAccounts.Infrastructure.Adapters.AccountingSubject;
using ACC.ChartOfAccounts.Infrastructure.Adapters.Authority;
using ACC.ChartOfAccounts.Infrastructure.Endpoints;
using ACC.ChartOfAccounts.Infrastructure.ReadModels.ChartOfAccounts;
using ACC.ChartOfAccounts.Infrastructure.Templates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACC.ChartOfAccounts;

public static class ModuleRegistration
{
    public static IServiceCollection AddChartOfAccounts(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddChartOfAccountsApplication();

        var persistenceProvider = configuration["Persistence:Provider"] ?? "Memory";

        return persistenceProvider.Trim().ToUpperInvariant() switch
        {
            "POSTGRES" => services.AddChartOfAccountsPostgresPersistence(),
            "MEMORY" => services.AddChartOfAccountsMemoryPersistence(),
            _ => throw new InvalidOperationException(
                $"Unsupported persistence provider '{persistenceProvider}' for ChartOfAccounts.")
        };
    }

    public static IServiceCollection AddChartOfAccountsApplication(this IServiceCollection services)
    {
        services.AddTransient(provider => new EventSourcedRepository<Domain.Aggregates.ChartOfAccounts>(
            provider.GetRequiredService<IEventStore>(),
            Domain.Aggregates.ChartOfAccounts.Rehydrate));
        services.AddTransient<ChartOfAccountsProjection>();
        services.AddTransient<AdoptChartOfAccountsHandler>();
        services.AddTransient<AddAccountHandler>();
        services.AddTransient<DeactivateAccountHandler>();
        services.AddTransient<ReactivateAccountHandler>();
        services.AddTransient<ViewChartOfAccountsHandler>();
        services.AddTransient<ViewChartOfAccountsTemplatesHandler>();
        services.AddTransient<IRecognizedAccountingSubjectPort, RecognizedAccountingSubjectAdapter>();
        services.AddTransient<IChartOfAccountsAuthorityPort, ChartOfAccountsAuthorityAdapter>();
        services.AddSingleton<IChartOfAccountsTemplateCatalog, FileChartOfAccountsTemplateCatalog>();

        return services;
    }

    public static IServiceCollection AddChartOfAccountsMemoryPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IEventStore, InMemoryEventStore>();
        services.AddSingleton<IChartOfAccountsStore, InMemoryChartOfAccountsStore>();
        services.AddSingleton<IAccountStore, InMemoryAccountStore>();

        return services;
    }

    public static IServiceCollection AddChartOfAccountsPostgresPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IEventStore, PostgresEventStore>();
        services.AddSingleton<IChartOfAccountsStore, PostgresChartOfAccountsStore>();
        services.AddSingleton<IAccountStore, PostgresAccountStore>();

        return services;
    }

    public static IEndpointRouteBuilder MapChartOfAccounts(this IEndpointRouteBuilder endpoints)
    {
        var chartOfAccounts = endpoints.MapGroup("/chart-of-accounts").RequireAuthorization();
        chartOfAccounts.MapChartOfAccountsEndpoints();

        return endpoints;
    }
}
