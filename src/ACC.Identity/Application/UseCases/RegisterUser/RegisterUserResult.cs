namespace ACC.Identity.Application.UseCases.RegisterUser;

public sealed record RegisterUserResult(
    Guid UserId,
    string AccessToken,
    DateTimeOffset ExpiresAt);
