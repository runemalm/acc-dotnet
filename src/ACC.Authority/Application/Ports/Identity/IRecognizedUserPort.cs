namespace ACC.Authority.Application.Ports.Identity;

public interface IRecognizedUserPort
{
    bool IsRecognizedUser(Guid userId);
}
