namespace ACC.AccountingSubject.Application.Ports.Identity;

public interface IRecognizedUserPort
{
    bool IsRecognizedUser(Guid userId);
}
