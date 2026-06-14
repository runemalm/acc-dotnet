using ACC.Authority.Domain.Powers;

namespace ACC.Authority.Application.Policies;

public interface IAuthorityPolicy
{
    bool HasPower(
        Guid actorUserId,
        Guid accountingSubjectId,
        Power power);
}
