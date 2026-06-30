using ACC.AccountingSubject.Application.Ports.Identity;
using ACC.AccountingSubject.Application.Ports.ReadModels.AccountingSubject;
using ACC.AccountingSubject.Application.UseCases.EstablishAccountingSubject;
using ACC.AccountingSubject.Infrastructure.Adapters.Identity;
using ACC.AccountingSubject.Infrastructure.ReadModels.AccountingSubject;
using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.EventSourcing.Memory;
using ACC.BuildingBlocks.EventSourcing.Postgres;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACC.AccountingSubject;

public static class ModuleRegistration
{
    public static IServiceCollection AddAccountingSubject(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAccountingSubjectApplication();

        var persistenceProvider = configuration["Persistence:Provider"] ?? "Memory";

        return persistenceProvider.Trim().ToUpperInvariant() switch
        {
            "POSTGRES" => services.AddAccountingSubjectPostgresPersistence(),
            "MEMORY" => services.AddAccountingSubjectMemoryPersistence(),
            _ => throw new InvalidOperationException(
                $"Unsupported persistence provider '{persistenceProvider}' for AccountingSubject.")
        };
    }

    public static IServiceCollection AddAccountingSubjectApplication(this IServiceCollection services)
    {
        services.AddTransient(provider => new EventSourcedRepository<Domain.Aggregates.AccountingSubject>(
            provider.GetRequiredService<IEventStore>(),
            Domain.Aggregates.AccountingSubject.Rehydrate));
        services.AddTransient<AccountingSubjectProjection>();
        services.AddTransient<EstablishAccountingSubjectHandler>();
        services.AddTransient<IRecognizedUserPort, RecognizedUserAdapter>();

        return services;
    }

    public static IServiceCollection AddAccountingSubjectMemoryPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IEventStore, InMemoryEventStore>();
        services.AddSingleton<IAccountingSubjectStore, InMemoryAccountingSubjectStore>();

        return services;
    }

    public static IServiceCollection AddAccountingSubjectPostgresPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IEventStore, PostgresEventStore>();
        services.AddSingleton<IAccountingSubjectStore, PostgresAccountingSubjectStore>();

        return services;
    }
}
