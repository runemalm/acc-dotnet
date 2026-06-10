namespace ACC.Identity.Domain.Events;

public sealed record EmailVerificationResent(
    Guid UserId,
    string EmailVerificationToken,
    DateTimeOffset EmailVerificationTokenExpiresAt,
    DateTimeOffset ResentAt);
