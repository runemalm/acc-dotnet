using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace ACC.Host;

public static class AuthenticationRegistration
{
    public static IServiceCollection AddHostAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var issuer = RequiredSetting(configuration, "Identity:Jwt:Issuer");
        var audience = RequiredSetting(configuration, "Identity:Jwt:Audience");
        var signingKey = RequiredSetting(configuration, "Identity:Jwt:SigningKey");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = "sub"
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var subject = context.Principal?.FindFirst("sub")?.Value;
                        if (!Guid.TryParse(subject, out _))
                        {
                            context.Fail("The token subject must identify a user.");
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static string RequiredSetting(IConfiguration configuration, string key) =>
        string.IsNullOrWhiteSpace(configuration[key])
            ? throw new InvalidOperationException($"Configuration setting '{key}' is required.")
            : configuration[key]!;
}
