using System.Collections.Concurrent;
using ACC.Authority.Application.Ports.ReadModels.RoleAssignment;
using ACC.Authority.Domain.Aggregates;

namespace ACC.Authority.Infrastructure.ReadModels.RoleAssignment;

public sealed class InMemoryRoleAssignmentStore : IRoleAssignmentStore
{
    private readonly ConcurrentDictionary<Guid, RoleAssignmentView> roleAssignments = new();

    public RoleAssignmentView? Find(Guid roleAssignmentId) =>
        roleAssignments.GetValueOrDefault(roleAssignmentId);

    public IReadOnlyCollection<RoleAssignmentView> FindActiveByUserId(Guid userId) =>
        roleAssignments.Values
            .Where(roleAssignment =>
                roleAssignment.UserId == userId &&
                roleAssignment.IsActive)
            .OrderBy(roleAssignment => roleAssignment.AccountingSubjectId)
            .ThenBy(roleAssignment => roleAssignment.Role)
            .ToArray();

    public bool ActiveAssignmentExists(
        Guid userId,
        Guid accountingSubjectId,
        Role role) =>
        roleAssignments.Values.Any(roleAssignment =>
            roleAssignment.UserId == userId &&
            roleAssignment.AccountingSubjectId == accountingSubjectId &&
            roleAssignment.Role == role &&
            roleAssignment.IsActive);

    public void Save(RoleAssignmentView roleAssignment)
    {
        ArgumentNullException.ThrowIfNull(roleAssignment);

        roleAssignments[roleAssignment.RoleAssignmentId] = roleAssignment;
    }
}
