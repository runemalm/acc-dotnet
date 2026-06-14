using ACC.AccountingSubject.Application.Ports.ReadModels.AccountingSubject;
using ACC.AccountingSubject.Application.UseCases.CreateAccountingSubject;
using ACC.AccountingSubject.Infrastructure.ReadModels.AccountingSubject;
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
        services.AddTransient<CreateAccountingSubjectHandler>();

        return services;
    }

    public static IServiceCollection AddAccountingSubjectMemoryPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IAccountingSubjectStore, InMemoryAccountingSubjectStore>();

        return services;
    }

    public static IServiceCollection AddAccountingSubjectPostgresPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IAccountingSubjectStore, PostgresAccountingSubjectStore>();

        return services;
    }
}
