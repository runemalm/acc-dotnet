using ACC.BuildingBlocks.EventSourcing;
using ACC.BuildingBlocks.EventSourcing.Memory;
using ACC.Identity.Application.Ports.Communication;
using ACC.Identity.Application.Ports.ReadModels.User;
using ACC.Identity.Application.Ports.Security;
using ACC.Identity.Application.UseCases.AuthenticateUser;
using ACC.Identity.Application.UseCases.RegisterUser;
using ACC.Identity.Application.UseCases.ResendVerification;
using ACC.Identity.Application.UseCases.VerifyEmail;
using ACC.Identity.Domain.Aggregates;
using ACC.Identity.Infrastructure.ReadModels.User;

namespace ACC.Identity.Tests.TestKit;

internal sealed class IdentityUseCaseTestContext
{
    private readonly InMemoryUserStore userStore = new();
    private readonly EventSourcedRepository<User> users;

    public IdentityUseCaseTestContext()
    {
        var eventStore = new InMemoryEventStore();
        users = new EventSourcedRepository<User>(
            eventStore,
            User.Rehydrate);
        var tokenGenerator = new TestEmailVerificationTokenGenerator();

        RegisterUser = new RegisterUserHandler(
            users,
            userStore,
            new UserProjection(userStore),
            new TestPasswordHasher(),
            tokenGenerator,
            EmailSender,
            TokenIssuer);

        AuthenticateUser = new AuthenticateUserHandler(
            users,
            userStore,
            new TestPasswordHasher(),
            TokenIssuer);

        ResendVerification = new ResendVerificationHandler(
            users,
            userStore,
            tokenGenerator,
            EmailSender,
            new UserProjection(userStore));

        VerifyEmail = new VerifyEmailHandler(
            users,
            userStore,
            new UserProjection(userStore));
    }

    public RegisterUserHandler RegisterUser { get; }

    public AuthenticateUserHandler AuthenticateUser { get; }

    public ResendVerificationHandler ResendVerification { get; }

    public VerifyEmailHandler VerifyEmail { get; }

    public RecordingIdentityEmailSender EmailSender { get; } = new();

    public RecordingAuthenticationTokenIssuer TokenIssuer { get; } = new();

    public UserView? FindUser(Guid userId) =>
        userStore.Find(userId);

    public UserView? FindUserByEmail(string email) =>
        userStore.FindByEmail(email.Trim().ToUpperInvariant());

    public User LoadUser(Guid userId) =>
        users.Load(StreamId.For("user", userId));

    private sealed class TestPasswordHasher : IPasswordHasher
    {
        public string Hash(string password) =>
            $"hashed:{password}";

        public bool Verify(string password, string passwordHash) =>
            passwordHash == Hash(password);
    }

    private sealed class TestEmailVerificationTokenGenerator : IEmailVerificationTokenGenerator
    {
        private int nextToken = 1;

        public string Generate() =>
            $"verification-token-{nextToken++}";
    }

    internal sealed class RecordingIdentityEmailSender : IIdentityEmailSender
    {
        private readonly List<VerificationEmail> sentVerificationEmails = [];

        public IReadOnlyCollection<VerificationEmail> SentVerificationEmails =>
            sentVerificationEmails.ToArray();

        public void SendVerificationEmail(
            string email,
            string verificationToken,
            DateTimeOffset expiresAt)
        {
            sentVerificationEmails.Add(new VerificationEmail(
                email,
                verificationToken,
                expiresAt));
        }
    }

    internal sealed record VerificationEmail(
        string Email,
        string VerificationToken,
        DateTimeOffset ExpiresAt);

    internal sealed class RecordingAuthenticationTokenIssuer : IAuthenticationTokenIssuer
    {
        public AuthenticationToken Issue(Guid userId, string email, DateTimeOffset issuedAt)
        {
            LastIssuedFor = new IssuedTokenRequest(userId, email, issuedAt);

            return new AuthenticationToken(
                "access-token",
                issuedAt.AddMinutes(60));
        }

        public IssuedTokenRequest? LastIssuedFor { get; private set; }
    }

    internal sealed record IssuedTokenRequest(
        Guid UserId,
        string Email,
        DateTimeOffset IssuedAt);
}
