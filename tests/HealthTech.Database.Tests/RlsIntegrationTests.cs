using Npgsql;
using Xunit;

namespace HealthTech.Database.Tests;

public sealed class RlsIntegrationTests(PostgresIntegrationFixture database) : IClassFixture<PostgresIntegrationFixture>
{
    [Fact]
    public async Task Application_role_is_configured_without_bypassrls()
    {
        if (!database.Enabled)
        {
            return;
        }

        await using var admin = new NpgsqlConnection(database.AdminConnectionString);
        await admin.OpenAsync();

        var bypassRls = await PostgresIntegrationFixture.ScalarAsync<bool>(
            admin,
            $"select rolbypassrls from pg_roles where rolname = '{PostgresIntegrationFixture.AppRole}';");

        Assert.False(bypassRls);
    }

    [Fact]
    public async Task Application_role_cannot_create_or_drop_structures()
    {
        if (!database.Enabled)
        {
            return;
        }

        await using var app = new NpgsqlConnection(database.AppConnectionString);
        await app.OpenAsync();

        var createException = await Assert.ThrowsAsync<PostgresException>(() =>
            PostgresIntegrationFixture.ExecuteAsync(app, "create table public.healthtech_forbidden_create(id int);"));
        Assert.Equal("42501", createException.SqlState);

        var createInDomainSchemaException = await Assert.ThrowsAsync<PostgresException>(() =>
            PostgresIntegrationFixture.ExecuteAsync(app, "create table patients.healthtech_forbidden_create(id int);"));
        Assert.Equal("42501", createInDomainSchemaException.SqlState);

        var dropException = await Assert.ThrowsAsync<PostgresException>(() =>
            PostgresIntegrationFixture.ExecuteAsync(app, "drop table core.tenant;"));
        Assert.Equal("42501", dropException.SqlState);
    }

    [Fact]
    public async Task Tenant_scoped_queries_only_return_current_tenant_rows()
    {
        if (!database.Enabled)
        {
            return;
        }

        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var patientA = Guid.NewGuid();
        var patientB = Guid.NewGuid();

        await using var admin = new NpgsqlConnection(database.AdminConnectionString);
        await admin.OpenAsync();
        await SeedTenantAsync(admin, tenantA, "Tenant A");
        await SeedTenantAsync(admin, tenantB, "Tenant B");
        await SeedPatientAsync(admin, tenantA, patientA, "Alice Tenant A", "DOC-A");
        await SeedPatientAsync(admin, tenantB, patientB, "Bob Tenant B", "DOC-B");

        await using var app = new NpgsqlConnection(database.AppConnectionString);
        await app.OpenAsync();
        await SetTenantAsync(app, tenantA);

        var visibleCount = await PostgresIntegrationFixture.ScalarAsync<long>(
            app,
            "select count(*) from patients.patient;");

        Assert.Equal(1L, visibleCount);
    }

    [Fact]
    public async Task Appointment_overlap_constraint_ignores_cancelled_records_for_same_window()
    {
        if (!database.Enabled)
        {
            return;
        }

        var tenantId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var specialtyId = Guid.NewGuid();
        var startsAt = DateTimeOffset.UtcNow.AddDays(1).AddMinutes(30);
        var endsAt = startsAt.AddMinutes(30);
        var firstAppointmentId = Guid.NewGuid();
        var secondAppointmentId = Guid.NewGuid();

        await using var admin = new NpgsqlConnection(database.AdminConnectionString);
        await admin.OpenAsync();
        await SeedTenantAsync(admin, tenantId, "Tenant");
        await SeedPatientAsync(admin, tenantId, patientId, "Alice", "DOC-CANCEL-WINDOW");
        await SeedProfessionalAsync(admin, tenantId, specialtyId, professionalId);

        await using var app = new NpgsqlConnection(database.AppConnectionString);
        await app.OpenAsync();
        await SetTenantAsync(app, tenantId);
        await SeedAppointmentAsync(app, tenantId, firstAppointmentId, patientId, professionalId, startsAt, endsAt);

        await PostgresIntegrationFixture.ExecuteAsync(
            app,
            $"""
            update appointments.appointment
            set status = 'cancelled'
            where id = '{firstAppointmentId:D}';
            """);

        await SeedAppointmentAsync(app, tenantId, secondAppointmentId, patientId, professionalId, startsAt, endsAt);

        var total = await PostgresIntegrationFixture.ScalarAsync<long>(
            app,
            "select count(*) from appointments.appointment;");

        Assert.Equal(2L, total);
    }

    [Fact]
    public async Task Seed_script_is_idempotent()
    {
        if (!database.Enabled)
        {
            return;
        }

        var seedPath = Path.Combine(GetRepoRoot(), "supabase", "seed.sql");
        var seedSql = PostgresIntegrationFixture.PrepareSqlForNpgsql(
            await File.ReadAllTextAsync(seedPath),
            new Dictionary<string, string>
            {
                ["system_admin_subject_id"] = string.Empty,
                ["system_admin_email"] = string.Empty
            });

        await using var admin = new NpgsqlConnection(database.AdminConnectionString);
        await admin.OpenAsync();

        await PostgresIntegrationFixture.ExecuteAsync(admin, seedSql);
        await PostgresIntegrationFixture.ExecuteAsync(admin, seedSql);

        var tenants = await PostgresIntegrationFixture.ScalarAsync<long>(
            admin,
            "select count(*) from core.tenant where id = '11111111-1111-1111-1111-111111111111';");
        var users = await PostgresIntegrationFixture.ScalarAsync<long>(
            admin,
            "select count(*) from core.tenant_user where tenant_id = '11111111-1111-1111-1111-111111111111' and subject_id = 'bootstrap-admin';");
        var plans = await PostgresIntegrationFixture.ScalarAsync<long>(
            admin,
            "select count(*) from patients.insurance_plan where tenant_id = '11111111-1111-1111-1111-111111111111' and payer_name = 'Unimed' and plan_name = 'Unimed Basico';");

        Assert.Equal(1L, tenants);
        Assert.Equal(1L, users);
        Assert.Equal(1L, plans);
    }

    [Fact]
    public async Task Rls_rejects_cross_tenant_insert_even_when_sql_sends_other_tenant_id()
    {
        if (!database.Enabled)
        {
            return;
        }

        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await using var admin = new NpgsqlConnection(database.AdminConnectionString);
        await admin.OpenAsync();
        await SeedTenantAsync(admin, tenantA, "Tenant A");
        await SeedTenantAsync(admin, tenantB, "Tenant B");

        await using var app = new NpgsqlConnection(database.AppConnectionString);
        await app.OpenAsync();
        await SetTenantAsync(app, tenantA);

        var exception = await Assert.ThrowsAsync<PostgresException>(() => SeedPatientAsync(
            app,
            tenantB,
            Guid.NewGuid(),
            "Mallory",
            "DOC-CROSS"));

        Assert.Equal("42501", exception.SqlState);
    }

    [Fact]
    public async Task Rls_prevents_cross_tenant_update_even_when_sql_filters_other_tenant_id()
    {
        if (!database.Enabled)
        {
            return;
        }

        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var patientB = Guid.NewGuid();

        await using var admin = new NpgsqlConnection(database.AdminConnectionString);
        await admin.OpenAsync();
        await SeedTenantAsync(admin, tenantA, "Tenant A");
        await SeedTenantAsync(admin, tenantB, "Tenant B");
        await SeedPatientAsync(admin, tenantB, patientB, "Bob Tenant B", "DOC-B-UPDATE");

        await using var app = new NpgsqlConnection(database.AppConnectionString);
        await app.OpenAsync();
        await SetTenantAsync(app, tenantA);

        var affected = await PostgresIntegrationFixture.ExecuteNonQueryAsync(
            app,
            $"""
            update patients.patient
            set full_name = 'Forged Update'
            where tenant_id = '{tenantB:D}' and id = '{patientB:D}';
            """);

        Assert.Equal(0, affected);

        var unchangedName = await PostgresIntegrationFixture.ScalarAsync<string>(
            admin,
            $"select full_name from patients.patient where tenant_id = '{tenantB:D}' and id = '{patientB:D}';");

        Assert.Equal("Bob Tenant B", unchangedName);
    }

    [Fact]
    public async Task Appointment_overlap_constraint_rejects_conflicting_professional_window()
    {
        if (!database.Enabled)
        {
            return;
        }

        var tenantId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var specialtyId = Guid.NewGuid();
        var startsAt = DateTimeOffset.UtcNow.AddDays(1).AddMinutes(10);
        var endsAt = startsAt.AddMinutes(30);

        await using var admin = new NpgsqlConnection(database.AdminConnectionString);
        await admin.OpenAsync();
        await SeedTenantAsync(admin, tenantId, "Tenant");
        await SeedPatientAsync(admin, tenantId, patientId, "Alice", "DOC-APPT");
        await SeedProfessionalAsync(admin, tenantId, specialtyId, professionalId);

        await using var app = new NpgsqlConnection(database.AppConnectionString);
        await app.OpenAsync();
        await SetTenantAsync(app, tenantId);
        await SeedAppointmentAsync(app, tenantId, Guid.NewGuid(), patientId, professionalId, startsAt, endsAt);

        var exception = await Assert.ThrowsAsync<PostgresException>(() => SeedAppointmentAsync(
            app,
            tenantId,
            Guid.NewGuid(),
            patientId,
            professionalId,
            startsAt.AddMinutes(10),
            endsAt.AddMinutes(10)));

        Assert.Equal("23P01", exception.SqlState);
    }

    [Fact]
    public async Task Appointment_overlap_constraint_allows_same_window_for_different_tenants()
    {
        if (!database.Enabled)
        {
            return;
        }

        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var patientA = Guid.NewGuid();
        var patientB = Guid.NewGuid();
        var professionalA = Guid.NewGuid();
        var professionalB = Guid.NewGuid();
        var specialtyA = Guid.NewGuid();
        var specialtyB = Guid.NewGuid();
        var startsAt = DateTimeOffset.UtcNow.AddDays(1).AddMinutes(20);
        var endsAt = startsAt.AddMinutes(30);

        await using var admin = new NpgsqlConnection(database.AdminConnectionString);
        await admin.OpenAsync();
        await SeedTenantAsync(admin, tenantA, "Tenant A");
        await SeedTenantAsync(admin, tenantB, "Tenant B");
        await SeedPatientAsync(admin, tenantA, patientA, "Alice", "DOC-APPT-A");
        await SeedPatientAsync(admin, tenantB, patientB, "Bob", "DOC-APPT-B");
        await SeedProfessionalAsync(admin, tenantA, specialtyA, professionalA);
        await SeedProfessionalAsync(admin, tenantB, specialtyB, professionalB);

        await using var app = new NpgsqlConnection(database.AppConnectionString);
        await app.OpenAsync();
        await SetTenantAsync(app, tenantA);
        await SeedAppointmentAsync(app, tenantA, Guid.NewGuid(), patientA, professionalA, startsAt, endsAt);

        await SetTenantAsync(app, tenantB);
        await SeedAppointmentAsync(app, tenantB, Guid.NewGuid(), patientB, professionalB, startsAt, endsAt);

        var appointmentCount = await PostgresIntegrationFixture.ScalarAsync<long>(
            admin,
            $"""
            select count(*)
            from appointments.appointment
            where tenant_id in ('{tenantA:D}', '{tenantB:D}');
            """);

        Assert.Equal(2L, appointmentCount);
    }

    [Fact]
    public async Task Appointment_rejects_location_from_another_tenant()
    {
        if (!database.Enabled)
        {
            return;
        }

        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var patientA = Guid.NewGuid();
        var professionalA = Guid.NewGuid();
        var specialtyA = Guid.NewGuid();
        var locationB = Guid.NewGuid();
        var startsAt = DateTimeOffset.UtcNow.AddDays(1).AddMinutes(40);

        await using var admin = new NpgsqlConnection(database.AdminConnectionString);
        await admin.OpenAsync();
        await SeedTenantAsync(admin, tenantA, "Tenant A");
        await SeedTenantAsync(admin, tenantB, "Tenant B");
        await SeedPatientAsync(admin, tenantA, patientA, "Alice", "DOC-APPT-LOC");
        await SeedProfessionalAsync(admin, tenantA, specialtyA, professionalA);
        await SeedLocationAsync(admin, tenantB, locationB);

        await using var app = new NpgsqlConnection(database.AppConnectionString);
        await app.OpenAsync();
        await SetTenantAsync(app, tenantA);

        var exception = await Assert.ThrowsAsync<PostgresException>(() => SeedAppointmentAsync(
            app,
            tenantA,
            Guid.NewGuid(),
            patientA,
            professionalA,
            startsAt,
            startsAt.AddMinutes(30),
            locationB));

        Assert.Equal("23503", exception.SqlState);
    }

    [Fact]
    public async Task WhatsApp_manual_message_is_isolated_by_tenant()
    {
        if (!database.Enabled)
        {
            return;
        }

        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var patientA = Guid.NewGuid();
        var professionalA = Guid.NewGuid();
        var specialtyA = Guid.NewGuid();
        var appointmentA = Guid.NewGuid();
        var messageA = Guid.NewGuid();
        var startsAt = DateTimeOffset.UtcNow.AddDays(2).AddHours(1);

        await using var admin = new NpgsqlConnection(database.AdminConnectionString);
        await admin.OpenAsync();
        await SeedTenantAsync(admin, tenantA, "Tenant A");
        await SeedTenantAsync(admin, tenantB, "Tenant B");
        await SeedPatientAsync(admin, tenantA, patientA, "Alice", "DOC-WPP-A");
        await SeedProfessionalAsync(admin, tenantA, specialtyA, professionalA);
        await SeedAppointmentAsync(admin, tenantA, appointmentA, patientA, professionalA, startsAt, startsAt.AddMinutes(30));

        await using var app = new NpgsqlConnection(database.AppConnectionString);
        await app.OpenAsync();
        await SetTenantAsync(app, tenantA);

        await PostgresIntegrationFixture.ExecuteAsync(app, $"""
            insert into appointments.whatsapp_manual_message (
              id, tenant_id, appointment_id, recipient_phone, message_text, status, created_by_subject
            ) values (
              '{messageA:D}', '{tenantA:D}', '{appointmentA:D}', '+5511999999999', 'Confirmacao', 'intent', 'user-a'
            );
            """);

        var forbidden = await Assert.ThrowsAsync<PostgresException>(() => PostgresIntegrationFixture.ExecuteAsync(app, $"""
            insert into appointments.whatsapp_manual_message (
              id, tenant_id, appointment_id, recipient_phone, message_text, status, created_by_subject
            ) values (
              '{Guid.NewGuid():D}', '{tenantB:D}', '{appointmentA:D}', '+5511888888888', 'Tentativa indevida', 'intent', 'user-a'
            );
            """));

        Assert.Equal("42501", forbidden.SqlState);
    }

    [Fact]
    public async Task Patient_evolution_is_isolated_by_tenant()
    {
        if (!database.Enabled)
        {
            return;
        }

        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var patientA = Guid.NewGuid();

        await using var admin = new NpgsqlConnection(database.AdminConnectionString);
        await admin.OpenAsync();
        await SeedTenantAsync(admin, tenantA, "Tenant A");
        await SeedTenantAsync(admin, tenantB, "Tenant B");
        await SeedPatientAsync(admin, tenantA, patientA, "Alice", "DOC-EHR-A");

        await using var app = new NpgsqlConnection(database.AppConnectionString);
        await app.OpenAsync();
        await SetTenantAsync(app, tenantA);

        await PostgresIntegrationFixture.ExecuteAsync(app, $"""
            insert into patients.patient_evolution (
              id, tenant_id, patient_id, encountered_at_utc, subjective, objective, assessment, plan, created_by_subject
            ) values (
              '{Guid.NewGuid():D}', '{tenantA:D}', '{patientA:D}', now(), 'subj', 'obj', 'assess', 'plan', 'user-a'
            );
            """);

        var forbidden = await Assert.ThrowsAsync<PostgresException>(() => PostgresIntegrationFixture.ExecuteAsync(app, $"""
            insert into patients.patient_evolution (
              id, tenant_id, patient_id, encountered_at_utc, subjective, objective, assessment, plan, created_by_subject
            ) values (
              '{Guid.NewGuid():D}', '{tenantB:D}', '{patientA:D}', now(), 'subj', 'obj', 'assess', 'plan', 'user-a'
            );
            """));

        Assert.Equal("42501", forbidden.SqlState);
    }

    [Fact]
    public async Task Patient_identity_is_isolated_by_tenant()
    {
        if (!database.Enabled)
        {
            return;
        }

        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var patientA = Guid.NewGuid();
        var patientB = Guid.NewGuid();

        await using var admin = new NpgsqlConnection(database.AdminConnectionString);
        await admin.OpenAsync();
        await SeedTenantAsync(admin, tenantA, "Tenant A");
        await SeedTenantAsync(admin, tenantB, "Tenant B");
        await SeedPatientAsync(admin, tenantA, patientA, "Alice", "DOC-IDENTITY-A");
        await SeedPatientAsync(admin, tenantB, patientB, "Bob", "DOC-IDENTITY-B");

        await using var app = new NpgsqlConnection(database.AppConnectionString);
        await app.OpenAsync();
        await SetTenantAsync(app, tenantA);

        await PostgresIntegrationFixture.ExecuteAsync(app, $"""
            insert into patients.patient_identity (
              tenant_id, patient_id, subject_id, email
            ) values (
              '{tenantA:D}', '{patientA:D}', 'patient-subject-a', 'patient-a@example.local'
            );
            """);

        var forbidden = await Assert.ThrowsAsync<PostgresException>(() => PostgresIntegrationFixture.ExecuteAsync(app, $"""
            insert into patients.patient_identity (
              tenant_id, patient_id, subject_id, email
            ) values (
              '{tenantB:D}', '{patientB:D}', 'patient-subject-b', 'patient-b@example.local'
            );
            """));

        Assert.Equal("42501", forbidden.SqlState);
    }

    [Fact]
    public async Task Billing_charge_and_batch_are_isolated_by_tenant()
    {
        if (!database.Enabled)
        {
            return;
        }

        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var patientA = Guid.NewGuid();
        var patientB = Guid.NewGuid();
        var professionalA = Guid.NewGuid();
        var professionalB = Guid.NewGuid();
        var specialtyA = Guid.NewGuid();
        var specialtyB = Guid.NewGuid();
        var appointmentA = Guid.NewGuid();
        var appointmentB = Guid.NewGuid();
        var batchA = Guid.NewGuid();
        var startsAt = DateTimeOffset.UtcNow.AddDays(3).AddHours(1);

        await using var admin = new NpgsqlConnection(database.AdminConnectionString);
        await admin.OpenAsync();
        await SeedTenantAsync(admin, tenantA, "Tenant A");
        await SeedTenantAsync(admin, tenantB, "Tenant B");
        await SeedPatientAsync(admin, tenantA, patientA, "Alice", "DOC-BILL-A");
        await SeedPatientAsync(admin, tenantB, patientB, "Bob", "DOC-BILL-B");
        await SeedProfessionalAsync(admin, tenantA, specialtyA, professionalA);
        await SeedProfessionalAsync(admin, tenantB, specialtyB, professionalB);
        await SeedAppointmentAsync(admin, tenantA, appointmentA, patientA, professionalA, startsAt, startsAt.AddMinutes(30));
        await SeedAppointmentAsync(admin, tenantB, appointmentB, patientB, professionalB, startsAt, startsAt.AddMinutes(30));

        await using var app = new NpgsqlConnection(database.AppConnectionString);
        await app.OpenAsync();
        await SetTenantAsync(app, tenantA);

        await PostgresIntegrationFixture.ExecuteAsync(app, $"""
            insert into appointments.billing_batch (
              id, tenant_id, competence_month, total_amount, status, created_by_subject
            ) values (
              '{batchA:D}', '{tenantA:D}', date '2026-06-01', 250.00, 'closed', 'user-a'
            );
            """);

        await PostgresIntegrationFixture.ExecuteAsync(app, $"""
            insert into appointments.charge (
              id, tenant_id, appointment_id, patient_id, payer_type, description, amount, due_date, status, batch_id, created_by_subject
            ) values (
              '{Guid.NewGuid():D}', '{tenantA:D}', '{appointmentA:D}', '{patientA:D}', 'private', 'Consulta tenant A', 250.00, date '2026-06-15', 'batched', '{batchA:D}', 'user-a'
            );
            """);

        var forbiddenBatchInsert = await Assert.ThrowsAsync<PostgresException>(() => PostgresIntegrationFixture.ExecuteAsync(app, $"""
            insert into appointments.billing_batch (
              id, tenant_id, competence_month, total_amount, status, created_by_subject
            ) values (
              '{Guid.NewGuid():D}', '{tenantB:D}', date '2026-06-01', 300.00, 'closed', 'user-a'
            );
            """));
        Assert.Equal("42501", forbiddenBatchInsert.SqlState);

        var forbiddenChargeInsert = await Assert.ThrowsAsync<PostgresException>(() => PostgresIntegrationFixture.ExecuteAsync(app, $"""
            insert into appointments.charge (
              id, tenant_id, appointment_id, patient_id, payer_type, description, amount, due_date, status, created_by_subject
            ) values (
              '{Guid.NewGuid():D}', '{tenantB:D}', '{appointmentB:D}', '{patientB:D}', 'insurance', 'Tentativa tenant B', 300.00, date '2026-06-16', 'open', 'user-a'
            );
            """));
        Assert.Equal("42501", forbiddenChargeInsert.SqlState);
    }

    [Fact]
    public async Task Active_membership_requires_both_user_and_clinic_to_remain_active()
    {
        if (!database.Enabled)
        {
            return;
        }

        var tenantId = Guid.NewGuid();
        const string subjectId = "live-membership-user";
        const string email = "live-membership@healthtech.local";

        await using var admin = new NpgsqlConnection(database.AdminConnectionString);
        await admin.OpenAsync();
        await PostgresIntegrationFixture.ExecuteAsync(admin, $"""
            insert into core.tenant (id, name, active) values ('{tenantId:D}', 'Live membership clinic', true);
            insert into core.tenant_user (tenant_id, subject_id, email, role, active)
            values ('{tenantId:D}', '{subjectId}', '{email}', 'admin', true);
            """);

        await using var app = new NpgsqlConnection(database.AppConnectionString);
        await app.OpenAsync();

        Assert.True(await PostgresIntegrationFixture.ScalarAsync<bool>(app, $"""
            select core.is_active_tenant_membership('{subjectId}', '{email}', '{tenantId:D}');
            """));

        await PostgresIntegrationFixture.ExecuteAsync(admin, $"""
            update core.tenant_user set active = false where tenant_id = '{tenantId:D}' and subject_id = '{subjectId}';
            """);
        Assert.False(await PostgresIntegrationFixture.ScalarAsync<bool>(app, $"""
            select core.is_active_tenant_membership('{subjectId}', '{email}', '{tenantId:D}');
            """));

        await PostgresIntegrationFixture.ExecuteAsync(admin, $"""
            update core.tenant_user set active = true where tenant_id = '{tenantId:D}' and subject_id = '{subjectId}';
            update core.tenant set active = false where id = '{tenantId:D}';
            """);
        Assert.False(await PostgresIntegrationFixture.ScalarAsync<bool>(app, $"""
            select core.is_active_tenant_membership('{subjectId}', '{email}', '{tenantId:D}');
            """));
    }

    [Fact]
    public async Task Inactive_clinic_allows_listing_and_deactivation_but_rejects_admin_activation()
    {
        if (!database.Enabled)
        {
            return;
        }

        var tenantId = Guid.NewGuid();
        const string actorSubject = "system-admin-lifecycle";
        const string actorEmail = "system-admin-lifecycle@healthtech.local";
        const string clinicAdminSubject = "clinic-admin-lifecycle";

        await using var admin = new NpgsqlConnection(database.AdminConnectionString);
        await admin.OpenAsync();
        await PostgresIntegrationFixture.ExecuteAsync(admin, $"""
            insert into core.system_admin_user (subject_id, email, active)
            values ('{actorSubject}', '{actorEmail}', true);
            insert into core.tenant (id, name, active)
            values ('{tenantId:D}', 'Lifecycle clinic', true);
            """);

        await using var app = new NpgsqlConnection(database.AppConnectionString);
        await app.OpenAsync();
        Assert.True(await PostgresIntegrationFixture.ScalarAsync<bool>(app, $"""
            select core.admin_update_tenant_admin(
              '{actorSubject}', '{actorEmail}', '{tenantId:D}', '{clinicAdminSubject}',
              'clinic-admin-lifecycle@healthtech.local', 'Clinic Admin', null, true);
            """));

        await PostgresIntegrationFixture.ExecuteAsync(app, $"""
            select * from core.admin_set_tenant_active('{actorSubject}', '{actorEmail}', '{tenantId:D}', false);
            """);

        Assert.Equal(1L, await PostgresIntegrationFixture.ScalarAsync<long>(app, $"""
            select count(*) from core.admin_list_tenant_admins('{actorSubject}', '{actorEmail}', '{tenantId:D}');
            """));

        Assert.True(await PostgresIntegrationFixture.ScalarAsync<bool>(app, $"""
            select core.admin_update_tenant_admin(
              '{actorSubject}', '{actorEmail}', '{tenantId:D}', '{clinicAdminSubject}',
              'clinic-admin-lifecycle@healthtech.local', 'Clinic Admin', null, false);
            """));

        var forbidden = await Assert.ThrowsAsync<PostgresException>(() => PostgresIntegrationFixture.ExecuteAsync(app, $"""
            select core.admin_update_tenant_admin(
              '{actorSubject}', '{actorEmail}', '{tenantId:D}', '{clinicAdminSubject}',
              'clinic-admin-lifecycle@healthtech.local', 'Clinic Admin', null, true);
            """));
        Assert.Equal("55000", forbidden.SqlState);
        Assert.Equal("inactive_clinic_admin_activation_forbidden", forbidden.MessageText);
    }

    [Fact]
    public async Task Concurrent_updates_cannot_disable_all_system_admins()
    {
        if (!database.Enabled)
        {
            return;
        }

        const string firstSubject = "system-admin-concurrent-a";
        const string firstEmail = "system-admin-concurrent-a@healthtech.local";
        const string secondSubject = "system-admin-concurrent-b";
        const string secondEmail = "system-admin-concurrent-b@healthtech.local";

        await using var admin = new NpgsqlConnection(database.AdminConnectionString);
        await admin.OpenAsync();
        await PostgresIntegrationFixture.ExecuteAsync(admin, $"""
            delete from core.system_admin_user;
            insert into core.system_admin_user (subject_id, email, active) values
              ('{firstSubject}', '{firstEmail}', true),
              ('{secondSubject}', '{secondEmail}', true);
            """);

        async Task<PostgresException?> TryDisableAsync(string subject, string email)
        {
            await using var connection = new NpgsqlConnection(database.AppConnectionString);
            await connection.OpenAsync();
            try
            {
                await PostgresIntegrationFixture.ExecuteAsync(connection, $"""
                    select core.admin_upsert_system_admin('{subject}', '{email}', '{subject}', '{email}', false);
                    """);
                return null;
            }
            catch (PostgresException exception)
            {
                return exception;
            }
        }

        var outcomes = await Task.WhenAll(
            TryDisableAsync(firstSubject, firstEmail),
            TryDisableAsync(secondSubject, secondEmail));

        Assert.Single(outcomes, outcome => outcome is null);
        var rejected = Assert.Single(outcomes, outcome => outcome is not null);
        Assert.Equal("23514", rejected!.SqlState);
        Assert.Equal("last_system_admin_cannot_be_disabled", rejected.MessageText);
        Assert.Equal(1L, await PostgresIntegrationFixture.ScalarAsync<long>(admin,
            "select count(*) from core.system_admin_user where active = true;"));
    }

    [Fact]
    public async Task Privileged_functions_do_not_grant_execute_to_public()
    {
        if (!database.Enabled)
        {
            return;
        }

        await using var admin = new NpgsqlConnection(database.AdminConnectionString);
        await admin.OpenAsync();
        var publicExecuteGrants = await PostgresIntegrationFixture.ScalarAsync<long>(admin, """
            select count(*)
            from pg_proc p
            join pg_namespace n on n.oid = p.pronamespace
            cross join lateral aclexplode(coalesce(p.proacl, acldefault('f', p.proowner))) privilege
            where n.nspname in ('core', 'patients', 'scheduling', 'appointments')
              and privilege.grantee = 0
              and privilege.privilege_type = 'EXECUTE';
            """);

        Assert.Equal(0L, publicExecuteGrants);
    }

    private static Task SetTenantAsync(NpgsqlConnection connection, Guid tenantId)
        => PostgresIntegrationFixture.ExecuteAsync(connection, $"select set_config('app.tenant_id', '{tenantId:D}', false);");

    private static Task SeedTenantAsync(NpgsqlConnection connection, Guid tenantId, string name)
        => PostgresIntegrationFixture.ExecuteAsync(connection, $"""
            insert into core.tenant (id, name)
            values ('{tenantId:D}', '{name}');
            """);

    private static Task SeedPatientAsync(NpgsqlConnection connection, Guid tenantId, Guid patientId, string fullName, string document)
        => PostgresIntegrationFixture.ExecuteAsync(connection, $"""
            insert into patients.patient (id, tenant_id, full_name, document, birth_date)
            values ('{patientId:D}', '{tenantId:D}', '{fullName}', '{document}', date '1990-01-01');
            """);

    private static Task SeedProfessionalAsync(NpgsqlConnection connection, Guid tenantId, Guid specialtyId, Guid professionalId)
        => PostgresIntegrationFixture.ExecuteAsync(connection, $"""
            insert into scheduling.specialty (id, tenant_id, name)
            values ('{specialtyId:D}', '{tenantId:D}', 'Clinica Geral');

            insert into scheduling.professional (id, tenant_id, full_name, specialty_id)
            values ('{professionalId:D}', '{tenantId:D}', 'Dra. Teste', '{specialtyId:D}');
            """);

    private static Task SeedLocationAsync(NpgsqlConnection connection, Guid tenantId, Guid locationId)
        => PostgresIntegrationFixture.ExecuteAsync(connection, $"""
            insert into scheduling.location (id, tenant_id, name)
            values ('{locationId:D}', '{tenantId:D}', 'Unidade Teste');
            """);

    private static Task SeedAppointmentAsync(
        NpgsqlConnection connection,
        Guid tenantId,
        Guid appointmentId,
        Guid patientId,
        Guid professionalId,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        Guid? locationId = null)
        => PostgresIntegrationFixture.ExecuteAsync(connection, $"""
            insert into appointments.appointment (id, tenant_id, patient_id, professional_id, location_id, starts_at_utc, ends_at_utc, status)
            values ('{appointmentId:D}', '{tenantId:D}', '{patientId:D}', '{professionalId:D}', {FormatNullableUuid(locationId)}, timestamptz '{startsAt:O}', timestamptz '{endsAt:O}', 'scheduled');
            """);

    private static string FormatNullableUuid(Guid? value)
        => value is null ? "null" : $"'{value.Value:D}'";

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
