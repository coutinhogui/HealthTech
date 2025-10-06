namespace HealthTech.Patients.Application;
using MediatR;
using HealthTech.BuildingBlocks.SharedKernel;
using HealthTech.Patients.Domain;

public record RegisterPatientCommand(string FullName, string Document, DateOnly BirthDate) : IRequest<Result<Guid>>;

public interface IPatientRepository
{
    Task AddAsync(Patient entity, CancellationToken ct);
    Task<Patient?> GetAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<Patient>> ListAsync(int skip, int take, CancellationToken ct);
}

public sealed class RegisterPatientHandler(IPatientRepository repo, HealthTech.BuildingBlocks.Abstractions.IUnitOfWork uow) : IRequestHandler<RegisterPatientCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterPatientCommand request, CancellationToken ct)
    {
        var entity = new Patient(PatientId.New(), request.FullName, request.Document, request.BirthDate);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return Result<Guid>.Ok(entity.Id.Value);
    }
}
