-- Keep privileged global administrator data outside direct PostgREST access.
-- Application access is mediated by SECURITY DEFINER functions whose EXECUTE
-- privilege is granted only by the local runtime-role bootstrap.
alter table core.system_admin_user enable row level security;
alter table core.system_admin_user force row level security;

-- Supabase installs extensions in a dedicated schema. Relocate the portable
-- btree_gist extension when upgrading databases created by the initial script.
create schema if not exists extensions;

do $$
begin
  if exists (
    select 1
    from pg_extension e
    join pg_namespace n on n.oid = e.extnamespace
    where e.extname = 'btree_gist'
      and n.nspname <> 'extensions'
  ) then
    alter extension btree_gist set schema extensions;
  end if;
end;
$$;

-- Evaluate the tenant context once per statement instead of once per row.
alter policy tenant_isolation on core.tenant
  using (id::text = (select current_setting('app.tenant_id', true)))
  with check (id::text = (select current_setting('app.tenant_id', true)));

alter policy tenant_isolation on core.tenant_user
  using (tenant_id::text = (select current_setting('app.tenant_id', true)))
  with check (tenant_id::text = (select current_setting('app.tenant_id', true)));

alter policy tenant_isolation on patients.patient
  using (tenant_id::text = (select current_setting('app.tenant_id', true)))
  with check (tenant_id::text = (select current_setting('app.tenant_id', true)));

alter policy tenant_isolation on patients.insurance_plan
  using (tenant_id::text = (select current_setting('app.tenant_id', true)))
  with check (tenant_id::text = (select current_setting('app.tenant_id', true)));

alter policy tenant_isolation on patients.patient_plan
  using (tenant_id::text = (select current_setting('app.tenant_id', true)))
  with check (tenant_id::text = (select current_setting('app.tenant_id', true)));

alter policy tenant_isolation on patients.patient_evolution
  using (tenant_id::text = (select current_setting('app.tenant_id', true)))
  with check (tenant_id::text = (select current_setting('app.tenant_id', true)));

alter policy tenant_isolation on patients.patient_identity
  using (tenant_id::text = (select current_setting('app.tenant_id', true)))
  with check (tenant_id::text = (select current_setting('app.tenant_id', true)));

alter policy tenant_isolation on scheduling.specialty
  using (tenant_id::text = (select current_setting('app.tenant_id', true)))
  with check (tenant_id::text = (select current_setting('app.tenant_id', true)));

alter policy tenant_isolation on scheduling.location
  using (tenant_id::text = (select current_setting('app.tenant_id', true)))
  with check (tenant_id::text = (select current_setting('app.tenant_id', true)));

alter policy tenant_isolation on scheduling.professional
  using (tenant_id::text = (select current_setting('app.tenant_id', true)))
  with check (tenant_id::text = (select current_setting('app.tenant_id', true)));

alter policy tenant_isolation on appointments.appointment
  using (tenant_id::text = (select current_setting('app.tenant_id', true)))
  with check (tenant_id::text = (select current_setting('app.tenant_id', true)));

alter policy tenant_isolation on appointments.whatsapp_manual_message
  using (tenant_id::text = (select current_setting('app.tenant_id', true)))
  with check (tenant_id::text = (select current_setting('app.tenant_id', true)));

alter policy tenant_isolation on appointments.billing_batch
  using (tenant_id::text = (select current_setting('app.tenant_id', true)))
  with check (tenant_id::text = (select current_setting('app.tenant_id', true)));

alter policy tenant_isolation on appointments.charge
  using (tenant_id::text = (select current_setting('app.tenant_id', true)))
  with check (tenant_id::text = (select current_setting('app.tenant_id', true)));

-- Cover every foreign key reported by the Supabase performance advisor.
create index if not exists ix_appointment_tenant_location
  on appointments.appointment(tenant_id, location_id);

create index if not exists ix_charge_appointment
  on appointments.charge(appointment_id);

create index if not exists ix_whatsapp_manual_appointment
  on appointments.whatsapp_manual_message(appointment_id);

create index if not exists ix_patient_plan_tenant_plan
  on patients.patient_plan(tenant_id, plan_id);

create index if not exists ix_professional_tenant_specialty
  on scheduling.professional(tenant_id, specialty_id);
