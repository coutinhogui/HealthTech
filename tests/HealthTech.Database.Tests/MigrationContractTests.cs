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

    [Fact]
    public void Tenant_user_professional_link_migration_preserves_tenant_boundary()
    {
        var path = Path.Combine(GetRepoRoot(), "supabase", "migrations", "202606010001_clinic_roles_rbac.sql");

        var sql = File.ReadAllText(path);

        Assert.Contains("alter table core.tenant_user", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("professional_id uuid null", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("foreign key (tenant_id, professional_id) references scheduling.professional(tenant_id, id)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("drop function if exists core.resolve_tenant_memberships(text, text)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("create or replace function core.resolve_tenant_memberships", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("professional_id", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Saas_admin_migration_creates_global_admins_and_patient_identity()
    {
        var path = Path.Combine(GetRepoRoot(), "supabase", "migrations", "202606040001_saas_system_admin_and_patient_identity.sql");

        var sql = File.ReadAllText(path);

        Assert.Contains("create table if not exists core.system_admin_user", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("create table if not exists patients.patient_identity", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("role in ('admin', 'professional', 'reception', 'billing', 'patient')", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("foreign key (tenant_id, patient_id) references patients.patient(tenant_id, id)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("alter table patients.patient_identity force row level security;", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("create or replace function core.is_system_admin", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("create or replace function patients.resolve_patient_identity", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Admin_lifecycle_migration_is_portable_and_protects_privileged_functions()
    {
        var path = Path.Combine(GetRepoRoot(), "supabase", "migrations", "20260719012218_admin_clinic_lifecycle.sql");

        var sql = File.ReadAllText(path);

        Assert.DoesNotContain("\\gexec", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(":'app_db_user'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("create or replace function core.admin_set_tenant_active", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("create or replace function core.is_active_tenant_membership", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("create or replace function core.admin_list_tenant_admins", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("create or replace function core.admin_update_tenant_admin", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("last_system_admin_cannot_be_disabled", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("revoke execute on all functions", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Supabase_hardening_migration_secures_global_admins_and_covers_advisor_findings()
    {
        var path = Path.Combine(GetRepoRoot(), "supabase", "migrations", "20260719023000_supabase_security_performance.sql");

        var sql = File.ReadAllText(path);

        Assert.Contains("alter table core.system_admin_user enable row level security", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("alter table core.system_admin_user force row level security", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("alter extension btree_gist set schema extensions", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("(select current_setting('app.tenant_id', true))", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ix_appointment_tenant_location", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ix_charge_appointment", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ix_whatsapp_manual_appointment", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ix_patient_plan_tenant_plan", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ix_professional_tenant_specialty", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Seed_bootstraps_first_system_admin_from_compose_variable()
    {
        var path = Path.Combine(GetRepoRoot(), "supabase", "seed.sql");

        var sql = File.ReadAllText(path);

        Assert.Contains("insert into core.system_admin_user", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(":'system_admin_email'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("on conflict (subject_id) do update", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Discovery_search_migration_adds_public_location_metadata_and_safe_functions()
    {
        var path = Path.Combine(GetRepoRoot(), "supabase", "migrations", "202606040004_discovery_search_locations.sql");

        var sql = File.ReadAllText(path);

        Assert.Contains("add column if not exists city", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("add column if not exists public_region", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("create or replace function scheduling.discovery_search", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("security definer", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("t.active = true", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("p.active = true", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("s.active = true", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("p_location_id is not null and not exists", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("raise exception 'location_not_found'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("grant execute on function scheduling.discovery_search", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Discovery_geolocation_migration_extends_search_without_exposing_private_data()
    {
        var path = Path.Combine(GetRepoRoot(), "supabase", "migrations", "202606040005_discovery_search_geolocation.sql");

        var sql = File.ReadAllText(path);

        Assert.Contains("p_latitude numeric default null", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("p_longitude numeric default null", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("distance_score", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("t.active = true", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("p.active = true", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("s.active = true", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("grant execute on function scheduling.discovery_search", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Seed_contains_multiple_public_clinics_regions_specialties_and_professionals()
    {
        var path = Path.Combine(GetRepoRoot(), "supabase", "seed.sql");

        var sql = File.ReadAllText(path);

        Assert.Contains("Cardio Prime", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Derma Center Paulista", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Jardins", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Barra da Tijuca", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Dra. Laura Martins", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Dra. Beatriz Nogueira", sql, StringComparison.OrdinalIgnoreCase);
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
