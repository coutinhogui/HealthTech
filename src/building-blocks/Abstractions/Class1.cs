namespace HealthTech.BuildingBlocks.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IDbConnectionFactory
{
    System.Data.IDbConnection Create();
}
