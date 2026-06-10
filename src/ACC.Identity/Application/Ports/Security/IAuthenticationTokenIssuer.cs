namespace ACC.Identity.Application.Ports.Security;

public interface IAuthenticationTokenIssuer
{
    AuthenticationToken Issue(Guid userId, string email, DateTimeOffset issuedAt);
}

public sealed record AuthenticationToken(
    string AccessToken,
    DateTimeOffset ExpiresAt);
