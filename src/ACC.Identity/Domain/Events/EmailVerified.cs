namespace ACC.Identity.Domain.Events;

public sealed record EmailVerified(
    Guid UserId,
    DateTimeOffset VerifiedAt);
