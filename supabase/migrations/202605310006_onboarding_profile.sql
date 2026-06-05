alter table core.tenant_user
  add column if not exists full_name varchar(160) null,
  add column if not exists phone varchar(32) null,
  add column if not exists onboarding_completed_at_utc timestamptz null;

update core.tenant_user
set full_name = coalesce(full_name, 'Administrador Demo'),
    phone = coalesce(phone, '+55 11 99999-0000'),
    onboarding_completed_at_utc = coalesce(onboarding_completed_at_utc, now())
where subject_id = 'bootstrap-admin'
  and email = 'admin@healthtech.local';

drop function if exists core.resolve_tenant_memberships(text, text) cascade;

create function core.resolve_tenant_memberships(
  p_subject_id text,
  p_email text
)
returns table (
  tenant_id uuid,
  tenant_name text,
  role text
)
language sql
security definer
set search_path = core, pg_temp
as $$
  select tu.tenant_id,
         t.name as tenant_name,
         tu.role
  from core.tenant_user tu
  join core.tenant t on t.id = tu.tenant_id
  where tu.active = true
    and t.active = true
    and (
      nullif(p_subject_id, '') is not null and tu.subject_id = p_subject_id
      or nullif(p_email, '') is not null and lower(tu.email) = lower(p_email)
    );
$$;

select format(
  'grant execute on function core.resolve_tenant_memberships(text, text) to %I',
  :'app_db_user')
\gexec
