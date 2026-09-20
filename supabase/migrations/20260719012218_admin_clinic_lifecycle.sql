create or replace function core.is_active_tenant_membership(
  p_subject_id text,
  p_email text,
  p_tenant_id uuid
)
returns boolean
language sql
stable
security definer
set search_path = core, pg_temp
as $$
  select exists (
    select 1
    from core.tenant_user tu
    join core.tenant t on t.id = tu.tenant_id
    where tu.tenant_id = p_tenant_id
      and tu.active = true
      and t.active = true
      and (
        nullif(trim(p_subject_id), '') is not null and tu.subject_id = trim(p_subject_id)
        or nullif(trim(p_email), '') is not null and lower(tu.email) = lower(trim(p_email))
      )
  );
$$;

create or replace function core.admin_set_tenant_active(
  p_subject_id text,
  p_email text,
  p_tenant_id uuid,
  p_active boolean
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

  update core.tenant t
  set active = p_active
  where t.id = p_tenant_id;

  if not found then
    return;
  end if;

  return query
    select t.id, t.name, t.active
    from core.tenant t
    where t.id = p_tenant_id;
end;
$$;

create or replace function core.admin_list_tenant_admins(
  p_subject_id text,
  p_email text,
  p_tenant_id uuid
)
returns table (
  subject_id text,
  email text,
  full_name text,
  phone text,
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
    select tu.subject_id,
           tu.email,
           tu.full_name::text,
           tu.phone::text,
           tu.active
    from core.tenant_user tu
    where tu.tenant_id = p_tenant_id
      and tu.role = 'admin'
    order by tu.active desc, tu.email;
end;
$$;

create or replace function core.admin_update_tenant_admin(
  p_subject_id text,
  p_email text,
  p_tenant_id uuid,
  p_admin_subject_id text,
  p_admin_email text,
  p_full_name text,
  p_phone text,
  p_active boolean
)
returns boolean
language plpgsql
security definer
set search_path = core, pg_temp
as $$
declare
  v_tenant_active boolean;
  v_admin_exists boolean;
begin
  if not core.is_system_admin(p_subject_id, p_email) then
    raise exception 'system_admin_required' using errcode = '42501';
  end if;

  select t.active
  into v_tenant_active
  from core.tenant t
  where t.id = p_tenant_id;

  if not found then
    return false;
  end if;

  select exists (
    select 1
    from core.tenant_user tu
    where tu.tenant_id = p_tenant_id
      and tu.subject_id = trim(p_admin_subject_id)
      and tu.role = 'admin'
  )
  into v_admin_exists;

  if not v_tenant_active and (p_active or not v_admin_exists) then
    raise exception 'inactive_clinic_admin_activation_forbidden' using errcode = '55000';
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
    lower(trim(p_admin_email)),
    'admin',
    nullif(trim(coalesce(p_full_name, '')), ''),
    nullif(trim(coalesce(p_phone, '')), ''),
    p_active,
    case when p_active then now() else null end
  )
  on conflict (tenant_id, subject_id) do update
  set email = excluded.email,
      role = 'admin',
      full_name = excluded.full_name,
      phone = excluded.phone,
      active = excluded.active,
      onboarding_completed_at_utc = case
        when excluded.active then coalesce(core.tenant_user.onboarding_completed_at_utc, now())
        else core.tenant_user.onboarding_completed_at_utc
      end;

  return true;
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
language sql
security definer
set search_path = core, pg_temp
as $$
  select core.admin_update_tenant_admin(
    p_subject_id,
    p_email,
    p_tenant_id,
    p_admin_subject_id,
    p_admin_email,
    p_full_name,
    p_phone,
    true
  );
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
declare
  v_target_is_active boolean;
begin
  lock table core.system_admin_user in share row exclusive mode;

  if not core.is_system_admin(p_subject_id, p_email) then
    raise exception 'system_admin_required' using errcode = '42501';
  end if;

  select sau.active
  into v_target_is_active
  from core.system_admin_user sau
  where sau.subject_id = trim(p_target_subject_id);

  if coalesce(v_target_is_active, false)
     and not p_active
     and (select count(*) from core.system_admin_user where active = true) <= 1 then
    raise exception 'last_system_admin_cannot_be_disabled' using errcode = '23514';
  end if;

  insert into core.system_admin_user (subject_id, email, active)
  values (trim(p_target_subject_id), lower(trim(p_target_email)), p_active)
  on conflict (subject_id) do update
  set email = excluded.email,
      active = excluded.active;

  return true;
end;
$$;

revoke execute on all functions in schema core, patients, scheduling, appointments from public;

alter default privileges in schema core revoke execute on functions from public;
alter default privileges in schema patients revoke execute on functions from public;
alter default privileges in schema scheduling revoke execute on functions from public;
alter default privileges in schema appointments revoke execute on functions from public;
