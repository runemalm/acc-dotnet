namespace ACC.Identity.Domain.Events;

public sealed record UserRegistered(
    Guid UserId,
    string Email,
    string PasswordHash,
    string EmailVerificationToken,
    DateTimeOffset EmailVerificationTokenExpiresAt,
    DateTimeOffset RegisteredAt);
