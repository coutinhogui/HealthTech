alter table core.tenant_user
  add column if not exists professional_id uuid null;

alter table core.tenant_user
  drop constraint if exists fk_tenant_user_professional;

alter table core.tenant_user
  add constraint fk_tenant_user_professional
  foreign key (tenant_id, professional_id) references scheduling.professional(tenant_id, id);

create index if not exists ix_tenant_user_tenant_professional
  on core.tenant_user(tenant_id, professional_id)
  where professional_id is not null;

drop function if exists core.resolve_tenant_memberships(text, text);

create or replace function core.resolve_tenant_memberships(
  p_subject_id text,
  p_email text
)
returns table (
  tenant_id uuid,
  tenant_name text,
  role text,
  professional_id uuid
)
language sql
security definer
set search_path = core, pg_temp
as $$
  select tu.tenant_id,
         t.name as tenant_name,
         tu.role,
         tu.professional_id
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
