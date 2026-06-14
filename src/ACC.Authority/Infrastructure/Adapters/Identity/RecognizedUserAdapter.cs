using ACC.Authority.Application.Ports.Identity;
using ACC.Identity.Application.Ports.ReadModels.User;

namespace ACC.Authority.Infrastructure.Adapters.Identity;

public sealed class RecognizedUserAdapter : IRecognizedUserPort
{
    private readonly IUserStore users;

    public RecognizedUserAdapter(IUserStore users)
    {
        this.users = users;
    }

    public bool IsRecognizedUser(Guid userId) =>
        users.Find(userId) is not null;
}
