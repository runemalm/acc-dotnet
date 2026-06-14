using ACC.Authority.Domain.Aggregates;

namespace ACC.Authority.Application.Ports.ReadModels.RoleAssignment;

public interface IRoleAssignmentStore
{
    RoleAssignmentView? Find(Guid roleAssignmentId);

    IReadOnlyCollection<RoleAssignmentView> FindActiveByUserId(Guid userId);

    bool ActiveAssignmentExists(
        Guid userId,
        Guid accountingSubjectId,
        Role role);

    void Save(RoleAssignmentView roleAssignment);
}
