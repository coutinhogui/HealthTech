using System.Security.Claims;

namespace HealthTech.BuildingBlocks.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IDbConnectionFactory
{
    System.Data.IDbConnection Create();
}

public static class HealthTechClaimTypes
{
    public const string TenantId = "tenant_id";
    public const string TenantRole = "tenant_role";
    public const string TenantName = "tenant_name";
}

public sealed record TenantMembership(Guid TenantId, string TenantName, string Role);

public sealed record ResolvedRequestContext(
    string SubjectId,
    string? Email,
    IReadOnlyCollection<TenantMembership> Memberships,
    TenantMembership? ActiveMembership)
{
    public bool HasActiveTenant => ActiveMembership is not null;

    public Guid GetRequiredTenantId()
        => ActiveMembership?.TenantId
           ?? throw new InvalidOperationException("No active tenant resolved for the current request.");

    public string GetRequiredTenantRole()
        => ActiveMembership?.Role
           ?? throw new InvalidOperationException("No active tenant role resolved for the current request.");
}

public interface IRequestContextAccessor
{
    ResolvedRequestContext Current { get; }
}

public interface ITenantAccessProvider
{
    Task<IReadOnlyCollection<TenantMembership>> GetMembershipsAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default);
}
