using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace ACC.Testing.Authentication;

public static class TestAuthenticationExtensions
{
    private const string Scheme = "Test";
    internal const string UserIdHeader = "X-Test-User-Id";

    public static IServiceCollection AddTestAuthentication(this IServiceCollection services)
    {
        services
            .AddAuthentication(Scheme)
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(Scheme, _ => { });
        services.AddAuthorization();

        return services;
    }

    public static HttpClient AuthenticateAs(this HttpClient client, Guid userId)
    {
        ArgumentNullException.ThrowIfNull(client);

        client.DefaultRequestHeaders.Remove(UserIdHeader);
        client.DefaultRequestHeaders.Add(UserIdHeader, userId.ToString());

        return client;
    }

    public static HttpClient SignOut(this HttpClient client)
    {
        ArgumentNullException.ThrowIfNull(client);

        client.DefaultRequestHeaders.Remove(UserIdHeader);

        return client;
    }
}
