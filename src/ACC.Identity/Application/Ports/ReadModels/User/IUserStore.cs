namespace ACC.Identity.Application.Ports.ReadModels.User;

public interface IUserStore
{
    UserView? Find(Guid userId);

    UserView? FindByEmail(string normalizedEmail);

    UserView? FindByEmailVerificationToken(string emailVerificationToken);

    void Save(UserView user);
}
