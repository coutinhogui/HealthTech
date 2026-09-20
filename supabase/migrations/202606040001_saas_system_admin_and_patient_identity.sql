\set ON_ERROR_STOP on

create table if not exists core.system_admin_user (
  subject_id text primary key,
  email text not null unique,
  active boolean not null default true,
  created_at_utc timestamptz not null default now()
);

alter table core.tenant_user
  drop constraint if exists tenant_user_role_check;

alter table core.tenant_user
  drop constraint if exists ck_tenant_user_role;

alter table core.tenant_user
  add constraint ck_tenant_user_role
  check (role in ('admin', 'professional', 'reception', 'billing', 'patient'));

create table if not exists patients.patient_identity (
  tenant_id uuid not null,
  patient_id uuid not null,
  subject_id text not null,
  email text null,
  active boolean not null default true,
  linked_at_utc timestamptz not null default now(),
  primary key (tenant_id, subject_id),
  unique (tenant_id, patient_id),
  foreign key (tenant_id, patient_id) references patients.patient(tenant_id, id)
);

create index if not exists ix_patient_identity_tenant_patient
  on patients.patient_identity(tenant_id, patient_id);

alter table patients.patient_identity enable row level security;
alter table patients.patient_identity force row level security;

drop policy if exists tenant_isolation on patients.patient_identity;
create policy tenant_isolation on patients.patient_identity
  using (tenant_id::text = current_setting('app.tenant_id', true))
  with check (tenant_id::text = current_setting('app.tenant_id', true));

create or replace function core.is_system_admin(
  p_subject_id text,
  p_email text
)
returns boolean
language sql
security definer
set search_path = core, pg_temp
as $$
  select exists (
    select 1
    from core.system_admin_user sau
    where sau.active = true
      and (
        nullif(p_subject_id, '') is not null and sau.subject_id = p_subject_id
        or nullif(p_email, '') is not null and lower(sau.email) = lower(p_email)
      )
  );
$$;

create or replace function patients.resolve_patient_identity(
  p_tenant_id uuid,
  p_subject_id text,
  p_email text
)
returns uuid
language sql
security definer
set search_path = patients, pg_temp
as $$
  select pi.patient_id
  from patients.patient_identity pi
  where pi.tenant_id = p_tenant_id
    and pi.active = true
    and (
      nullif(p_subject_id, '') is not null and pi.subject_id = p_subject_id
      or nullif(p_email, '') is not null and lower(pi.email) = lower(p_email)
    )
  order by pi.linked_at_utc desc
  limit 1;
$$;

create or replace function core.admin_list_tenants(
  p_subject_id text,
  p_email text
)
returns table (
  id uuid,
  name text,
  active boolean
)
language plpgsql
security definer
set search_path = core, pg_temp
as $$
begin
  if not core.is_system_admin(p_subject_id, p_email) then
    raise exception 'system_admin_required' using errcode = '42501';
  end if;

  return query
    select t.id, t.name, t.active
    from core.tenant t
    order by t.name;
end;
$$;

create or replace function core.admin_create_tenant(
  p_subject_id text,
  p_email text,
  p_tenant_id uuid,
  p_tenant_name text,
  p_admin_subject_id text,
  p_admin_email text
)
returns table (
  id uuid,
  name text,
  active boolean
)
language plpgsql
security definer
set search_path = core, pg_temp
as $$
begin
  if not core.is_system_admin(p_subject_id, p_email) then
    raise exception 'system_admin_required' using errcode = '42501';
  end if;

  insert into core.tenant (id, name, active)
  values (p_tenant_id, trim(p_tenant_name), true)
  on conflict (id) do update
  set name = excluded.name,
      active = true;

  insert into core.tenant_user (tenant_id, subject_id, email, role, active, onboarding_completed_at_utc)
  values (p_tenant_id, trim(p_admin_subject_id), trim(p_admin_email), 'admin', true, now())
  on conflict (tenant_id, subject_id) do update
  set email = excluded.email,
      role = 'admin',
      active = true,
      onboarding_completed_at_utc = coalesce(core.tenant_user.onboarding_completed_at_utc, now());

  return query
    select t.id, t.name, t.active
    from core.tenant t
    where t.id = p_tenant_id;
end;
$$;

create or replace function core.admin_upsert_tenant_admin(
  p_subject_id text,
  p_email text,
  p_tenant_id uuid,
  p_admin_subject_id text,
  p_admin_email text,
  p_full_name text,
  p_phone text
)
returns boolean
language plpgsql
security definer
set search_path = core, pg_temp
as $$
begin
  if not core.is_system_admin(p_subject_id, p_email) then
    raise exception 'system_admin_required' using errcode = '42501';
  end if;

  if not exists (select 1 from core.tenant where id = p_tenant_id and active = true) then
    return false;
  end if;

  insert into core.tenant_user (
    tenant_id,
    subject_id,
    email,
    role,
    full_name,
    phone,
    active,
    onboarding_completed_at_utc
  )
  values (
    p_tenant_id,
    trim(p_admin_subject_id),
    trim(p_admin_email),
    'admin',
    nullif(trim(coalesce(p_full_name, '')), ''),
    nullif(trim(coalesce(p_phone, '')), ''),
    true,
    now()
  )
  on conflict (tenant_id, subject_id) do update
  set email = excluded.email,
      role = 'admin',
      full_name = excluded.full_name,
      phone = excluded.phone,
      active = true,
      onboarding_completed_at_utc = coalesce(core.tenant_user.onboarding_completed_at_utc, now());

  return true;
end;
$$;

create or replace function core.admin_list_system_admins(
  p_subject_id text,
  p_email text
)
returns table (
  subject_id text,
  email text,
  active boolean
)
language plpgsql
security definer
set search_path = core, pg_temp
as $$
begin
  if not core.is_system_admin(p_subject_id, p_email) then
    raise exception 'system_admin_required' using errcode = '42501';
  end if;

  return query
    select sau.subject_id, sau.email, sau.active
    from core.system_admin_user sau
    order by sau.email;
end;
$$;

create or replace function core.admin_upsert_system_admin(
  p_subject_id text,
  p_email text,
  p_target_subject_id text,
  p_target_email text,
  p_active boolean
)
returns boolean
language plpgsql
security definer
set search_path = core, pg_temp
as $$
begin
  if not core.is_system_admin(p_subject_id, p_email) then
    raise exception 'system_admin_required' using errcode = '42501';
  end if;

  insert into core.system_admin_user (subject_id, email, active)
  values (trim(p_target_subject_id), trim(p_target_email), p_active)
  on conflict (subject_id) do update
  set email = excluded.email,
      active = excluded.active;

  return true;
end;
$$;

select format(
  'grant select, insert, update, delete on core.system_admin_user, patients.patient_identity to %I',
  :'app_db_user')
\gexec

select format(
  'grant execute on function core.is_system_admin(text, text) to %I',
  :'app_db_user')
\gexec

select format(
  'grant execute on function patients.resolve_patient_identity(uuid, text, text) to %I',
  :'app_db_user')
\gexec

select format(
  'grant execute on function core.admin_list_tenants(text, text) to %I',
  :'app_db_user')
\gexec

select format(
  'grant execute on function core.admin_create_tenant(text, text, uuid, text, text, text) to %I',
  :'app_db_user')
\gexec

select format(
  'grant execute on function core.admin_upsert_tenant_admin(text, text, uuid, text, text, text, text) to %I',
  :'app_db_user')
\gexec

select format(
  'grant execute on function core.admin_list_system_admins(text, text) to %I',
  :'app_db_user')
\gexec

select format(
  'grant execute on function core.admin_upsert_system_admin(text, text, text, text, boolean) to %I',
  :'app_db_user')
\gexec
