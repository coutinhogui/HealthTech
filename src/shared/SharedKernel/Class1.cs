using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace HealthTech.BuildingBlocks.SharedKernel;

public abstract class Entity<TId>
{
    public TId Id { get; protected set; }
    protected Entity(TId id) => Id = id;
}

public sealed record ResultError(string Code, string Message, IReadOnlyDictionary<string, string[]> Details)
{
    public static ResultError Create(string code, string message)
        => new(code, message, new Dictionary<string, string[]>());

    public static ResultError Validation(IReadOnlyDictionary<string, string[]> details)
        => new("validation_error", "One or more validation errors occurred.", details);
}

public readonly record struct Result<T>(bool Success, T? Value, ResultError? Error)
{
    public static Result<T> Ok(T value) => new(true, value, null);
    public static Result<T> Fail(string error) => new(false, default, ResultError.Create("error", error));
    public static Result<T> Fail(string code, string message) => new(false, default, ResultError.Create(code, message));
    public static Result<T> Validation(IReadOnlyDictionary<string, string[]> details) => new(false, default, ResultError.Validation(details));
}

public static class HealthTechTelemetry
{
    public const string ActivitySourceName = "HealthTech";
    public const string MeterName = "HealthTech";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    public static readonly Meter Meter = new(MeterName);
}
