namespace ACC.Identity.Application.UseCases.AuthenticateUser;

public sealed record AuthenticateUserResult(
    string AccessToken,
    DateTimeOffset ExpiresAt);
