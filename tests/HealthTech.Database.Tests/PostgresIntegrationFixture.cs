using Npgsql;
using Xunit;

namespace HealthTech.Database.Tests;

public sealed class PostgresIntegrationFixture : IAsyncLifetime
{
    public const string AppRole = "healthtech_app_test";
    public const string AppPassword = "healthtech-app-test";

    public string? AdminConnectionString { get; private set; }
    public string? AppConnectionString { get; private set; }
    public bool Enabled => !string.IsNullOrWhiteSpace(AdminConnectionString);

    public async Task InitializeAsync()
    {
        AdminConnectionString = Environment.GetEnvironmentVariable("HEALTHTECH_TEST_DATABASE");
        if (string.IsNullOrWhiteSpace(AdminConnectionString))
        {
            return;
        }

        var builder = new NpgsqlConnectionStringBuilder(AdminConnectionString);
        if (string.IsNullOrWhiteSpace(builder.Database) ||
            !builder.Database.Contains("test", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("HEALTHTECH_TEST_DATABASE must point to a disposable test database.");
        }

        await using var admin = new NpgsqlConnection(AdminConnectionString);
        await admin.OpenAsync();

        await ExecuteAsync(admin, """
            drop schema if exists appointments cascade;
            drop schema if exists scheduling cascade;
            drop schema if exists patients cascade;
            drop schema if exists core cascade;
            """);

        var migrationsPath = Path.Combine(GetRepoRoot(), "supabase", "migrations");
        // Role migration uses psql meta-commands (\set/\gexec), so the fixture applies
        // schema/data migrations here and configures the app role programmatically below.
        var migrationFiles = Directory.GetFiles(migrationsPath, "*.sql")
            .Where(path => !path.EndsWith("202605310001_application_role.sql", StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var migrationFile in migrationFiles)
        {
            var sql = await File.ReadAllTextAsync(migrationFile);
            await ExecuteAsync(admin, sql);
        }

        await ExecuteAsync(admin, $$"""
            do $$
            begin
              if not exists (select 1 from pg_roles where rolname = '{{AppRole}}') then
                create role {{AppRole}} login password '{{AppPassword}}' nosuperuser nocreatedb nocreaterole noreplication nobypassrls;
              end if;
            end
            $$;

            alter role {{AppRole}} login password '{{AppPassword}}' nosuperuser nocreatedb nocreaterole noreplication nobypassrls;
            grant usage on schema core, patients, scheduling, appointments to {{AppRole}};
            revoke create on schema public from {{AppRole}};
            revoke create on schema core, patients, scheduling, appointments from {{AppRole}};
            grant select, insert, update, delete on all tables in schema core, patients, scheduling, appointments to {{AppRole}};
            grant usage, select on all sequences in schema core, patients, scheduling, appointments to {{AppRole}};
            """);

        builder.Username = AppRole;
        builder.Password = AppPassword;
        AppConnectionString = builder.ConnectionString;
    }

    public async Task DisposeAsync()
    {
        if (!Enabled)
        {
            return;
        }

        await using var admin = new NpgsqlConnection(AdminConnectionString);
        await admin.OpenAsync();
        await ExecuteAsync(admin, """
            drop schema if exists appointments cascade;
            drop schema if exists scheduling cascade;
            drop schema if exists patients cascade;
            drop schema if exists core cascade;
            """);
    }

    public static async Task ExecuteAsync(NpgsqlConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    public static async Task<int> ExecuteNonQueryAsync(NpgsqlConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return await command.ExecuteNonQueryAsync();
    }

    public static async Task<T?> ScalarAsync<T>(NpgsqlConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        var value = await command.ExecuteScalarAsync();
        return value is null or DBNull ? default : (T)value;
    }

    private static string GetRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "HealthTech.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not locate repository root.");
    }
}
