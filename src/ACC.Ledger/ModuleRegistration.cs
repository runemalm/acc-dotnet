using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.EventSourcing.Memory;
using ACC.BuildingBlocks.EventSourcing.Postgres;
using ACC.Ledger.Application.Ports.ChartOfAccounts;
using ACC.Ledger.Application.Ports.Authority;
using ACC.Ledger.Application.UseCases.CloseFiscalPeriod;
using ACC.Ledger.Application.UseCases.OpenFiscalPeriod;
using ACC.Ledger.Application.UseCases.PostJournalEntry;
using ACC.Ledger.Application.UseCases.ViewJournalEntry;
using ACC.Ledger.Application.Ports.ReadModels.FiscalPeriod;
using ACC.Ledger.Application.Ports.ReadModels.JournalEntry;
using ACC.Ledger.Application.Ports.ReadModels.TrialBalance;
using ACC.Ledger.Infrastructure.Endpoints;
using ACC.Ledger.Infrastructure.Adapters.ChartOfAccounts;
using ACC.Ledger.Infrastructure.Adapters.Authority;
using ACC.Ledger.Infrastructure.ReadModels.FiscalPeriod;
using ACC.Ledger.Infrastructure.ReadModels.JournalEntry;
using ACC.Ledger.Domain.Aggregates;
using ACC.Ledger.Infrastructure.ReadModels.TrialBalance;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACC.Ledger;

public static class ModuleRegistration
{
    public static IServiceCollection AddLedger(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddLedgerApplication();

        var persistenceProvider = configuration["Persistence:Provider"] ?? "Memory";

        return persistenceProvider.Trim().ToUpperInvariant() switch
        {
            "POSTGRES" => services.AddLedgerPostgresPersistence(),
            "MEMORY" => services.AddLedgerMemoryPersistence(),
            _ => throw new InvalidOperationException(
                $"Unsupported persistence provider '{persistenceProvider}' for Ledger.")
        };
    }

    public static IServiceCollection AddLedgerApplication(this IServiceCollection services)
    {
        services.AddTransient<JournalEntryProjection>();
        services.AddTransient<FiscalPeriodProjection>();
        services.AddTransient<TrialBalanceProjection>();
        services.AddTransient(provider => new EventSourcedRepository<JournalEntry>(
            provider.GetRequiredService<IEventStore>(),
            JournalEntry.Rehydrate));
        services.AddTransient(provider => new EventSourcedRepository<FiscalPeriod>(
            provider.GetRequiredService<IEventStore>(),
            FiscalPeriod.Rehydrate));
        services.AddTransient<ViewJournalEntryHandler>();
        services.AddTransient<PostJournalEntryHandler>();
        services.AddTransient<OpenFiscalPeriodHandler>();
        services.AddTransient<CloseFiscalPeriodHandler>();
        services.AddTransient<IAccountAvailabilityPort, AccountAvailabilityAdapter>();
        services.AddTransient<ILedgerAuthorityPort, LedgerAuthorityAdapter>();

        return services;
    }

    public static IServiceCollection AddLedgerMemoryPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IEventStore, InMemoryEventStore>();
        services.AddSingleton<IFiscalPeriodStore, InMemoryFiscalPeriodStore>();
        services.AddSingleton<IJournalEntryStore, InMemoryJournalEntryStore>();
        services.AddSingleton<ITrialBalanceStore, InMemoryTrialBalanceStore>();

        return services;
    }

    public static IServiceCollection AddLedgerPostgresPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IEventStore, PostgresEventStore>();
        services.AddSingleton<IFiscalPeriodStore, PostgresFiscalPeriodStore>();
        services.AddSingleton<IJournalEntryStore, PostgresJournalEntryStore>();
        services.AddSingleton<ITrialBalanceStore, PostgresTrialBalanceStore>();

        return services;
    }

    public static IEndpointRouteBuilder MapLedger(this IEndpointRouteBuilder endpoints)
    {
        var ledger = endpoints.MapGroup("/ledger").RequireAuthorization();

        ledger.MapLedgerEndpoints();

        return endpoints;
    }
}
