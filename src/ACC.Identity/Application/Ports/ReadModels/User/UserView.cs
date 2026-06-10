namespace ACC.Identity.Application.Ports.ReadModels.User;

public sealed record UserView(
    Guid UserId,
    string Email,
    string NormalizedEmail,
    bool IsActive,
    string? EmailVerificationToken,
    DateTimeOffset? EmailVerifiedAt);
