namespace HealthTech.Database.Tests;

using Xunit;

public sealed class MigrationContractTests
{
    [Fact]
    public void Initial_foundation_migration_enables_rls_and_core_constraints()
    {
        var path = Path.Combine(GetRepoRoot(), "supabase", "migrations", "202605260001_initial_foundation.sql");

        var sql = File.ReadAllText(path);

        Assert.Contains("enable row level security", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("create policy", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no_overlapping_active_appointments", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("foreign key (tenant_id, patient_id)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("check (ends_at_utc > starts_at_utc)", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Initial_foundation_migration_forces_rls_on_tenant_scoped_tables()
    {
        var path = Path.Combine(GetRepoRoot(), "supabase", "migrations", "202605260001_initial_foundation.sql");

        var sql = File.ReadAllText(path);

        string[] tenantScopedTables =
        [
            "core.tenant",
            "core.tenant_user",
            "patients.patient",
            "scheduling.specialty",
            "scheduling.location",
            "scheduling.professional",
            "appointments.appointment"
        ];

        foreach (var table in tenantScopedTables)
        {
            Assert.Contains($"alter table {table} force row level security;", sql, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Application_role_migration_defines_least_privilege_and_nobypassrls()
    {
        var path = Path.Combine(GetRepoRoot(), "supabase", "migrations", "202605310001_application_role.sql");

        var sql = File.ReadAllText(path);

        Assert.Contains("create role", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("nobypassrls", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("nosuperuser", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("nocreatedb", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("nocreaterole", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("grant usage on schema core, patients, scheduling, appointments", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("revoke create on schema public", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("revoke create on schema core, patients, scheduling, appointments", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("grant select, insert, update, delete on all tables", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("grant usage, select on all sequences", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void WhatsApp_manual_migration_creates_tenant_scoped_table_with_rls()
    {
        var path = Path.Combine(GetRepoRoot(), "supabase", "migrations", "202605310003_whatsapp_manual.sql");

        var sql = File.ReadAllText(path);

        Assert.Contains("create table if not exists appointments.whatsapp_manual_message", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("foreign key (appointment_id) references appointments.appointment(id)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("status in ('intent', 'sent')", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("enable row level security", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("force row level security", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("create policy tenant_isolation", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Patient_evolution_migration_creates_tenant_scoped_table_with_rls()
    {
        var path = Path.Combine(GetRepoRoot(), "supabase", "migrations", "202605310004_patient_evolution.sql");

        var sql = File.ReadAllText(path);

        Assert.Contains("create table if not exists patients.patient_evolution", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("foreign key (tenant_id, patient_id) references patients.patient(tenant_id, id)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("encountered_at_utc", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("enable row level security", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("force row level security", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("create policy tenant_isolation", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Finance_basics_migration_creates_charge_and_batch_with_rls_and_tenant_fks()
    {
        var path = Path.Combine(GetRepoRoot(), "supabase", "migrations", "202605310005_finance_basics.sql");

        var sql = File.ReadAllText(path);

        Assert.Contains("create table if not exists appointments.billing_batch", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("create table if not exists appointments.charge", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("payer_type in ('insurance', 'private')", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("status in ('open', 'paid', 'batched', 'overdue')", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("foreign key (tenant_id, patient_id) references patients.patient(tenant_id, id)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("foreign key (tenant_id, batch_id) references appointments.billing_batch(tenant_id, id)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("alter table appointments.billing_batch force row level security;", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("alter table appointments.charge force row level security;", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("create policy tenant_isolation on appointments.billing_batch", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("create policy tenant_isolation on appointments.charge", sql, StringComparison.OrdinalIgnoreCase);
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
