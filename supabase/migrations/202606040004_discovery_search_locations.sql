\set ON_ERROR_STOP on

alter table scheduling.location
  add column if not exists address_line varchar(180),
  add column if not exists neighborhood varchar(120),
  add column if not exists city varchar(120),
  add column if not exists state varchar(32),
  add column if not exists postal_code varchar(24),
  add column if not exists latitude numeric(9,6),
  add column if not exists longitude numeric(9,6),
  add column if not exists public_region varchar(160);

create index if not exists ix_location_public_region
  on scheduling.location (tenant_id, city, state, public_region)
  where active = true;

create or replace function scheduling.discovery_search(
  p_mode text default 'professional',
  p_query text default null,
  p_clinic text default null,
  p_specialty text default null,
  p_region text default null,
  p_take integer default 20
)
returns table (
  tenant_id uuid,
  professional_id uuid,
  location_id uuid,
  professional_name text,
  specialty_name text,
  clinic_name text,
  location_name text,
  city text,
  state text,
  region_label text
)
language sql
security definer
set search_path = scheduling, core, pg_temp
as $$
  with normalized as (
    select
      lower(nullif(trim(coalesce(p_query, '')), '')) as q,
      lower(nullif(trim(coalesce(p_clinic, '')), '')) as clinic,
      lower(nullif(trim(coalesce(p_specialty, '')), '')) as specialty,
      lower(nullif(trim(coalesce(p_region, '')), '')) as region,
      least(greatest(coalesce(p_take, 20), 1), 50) as take_count,
      case when lower(coalesce(p_mode, 'professional')) = 'clinic' then 'clinic' else 'professional' end as mode
  )
  select
    t.id as tenant_id,
    p.id as professional_id,
    l.id as location_id,
    p.full_name as professional_name,
    s.name as specialty_name,
    t.name as clinic_name,
    l.name as location_name,
    l.city,
    l.state,
    concat_ws(' - ',
      nullif(concat_ws(', ', nullif(l.public_region, ''), nullif(l.city, '')), ''),
      nullif(l.state, '')
    ) as region_label
  from normalized n
  cross join core.tenant t
  inner join scheduling.professional p on p.tenant_id = t.id
  inner join scheduling.specialty s on s.tenant_id = p.tenant_id and s.id = p.specialty_id
  left join scheduling.location l on l.tenant_id = t.id and l.active = true
  where t.active = true
    and p.active = true
    and s.active = true
    and (
      n.q is null
      or lower(p.full_name) like '%' || n.q || '%'
      or lower(s.name) like '%' || n.q || '%'
      or lower(t.name) like '%' || n.q || '%'
    )
    and (
      n.clinic is null
      or lower(t.name) like '%' || n.clinic || '%'
    )
    and (
      n.specialty is null
      or lower(s.name) like '%' || n.specialty || '%'
    )
    and (
      n.region is null
      or lower(coalesce(l.public_region, '')) like '%' || n.region || '%'
      or lower(coalesce(l.neighborhood, '')) like '%' || n.region || '%'
      or lower(coalesce(l.city, '')) like '%' || n.region || '%'
      or lower(coalesce(l.state, '')) like '%' || n.region || '%'
      or lower(coalesce(l.postal_code, '')) like '%' || n.region || '%'
    )
  order by
    case when n.mode = 'clinic' then lower(t.name) else lower(p.full_name) end,
    lower(s.name),
    lower(coalesce(l.city, '')),
    lower(coalesce(l.name, ''))
  limit (select take_count from normalized);
$$;

create or replace function appointments.discovery_list_busy_windows(
  p_tenant_id uuid,
  p_professional_id uuid,
  p_location_id uuid,
  p_from_utc timestamptz,
  p_to_utc timestamptz
)
returns table (
  starts_at_utc timestamptz,
  ends_at_utc timestamptz
)
language sql
security definer
set search_path = appointments, scheduling, core, pg_temp
as $$
  select a.starts_at_utc,
         a.ends_at_utc
  from appointments.appointment a
  inner join scheduling.professional p
    on p.tenant_id = a.tenant_id and p.id = a.professional_id
  inner join core.tenant t on t.id = a.tenant_id
  where a.tenant_id = p_tenant_id
    and a.professional_id = p_professional_id
    and a.status in ('scheduled', 'confirmed')
    and a.starts_at_utc < p_to_utc
    and a.ends_at_utc > p_from_utc
    and p.active = true
    and t.active = true
    and (
      p_location_id is null
      or exists (
        select 1
        from scheduling.location l
        where l.tenant_id = p_tenant_id
          and l.id = p_location_id
          and l.active = true
      )
    )
  order by a.starts_at_utc;
$$;

create or replace function appointments.discovery_book_appointment(
  p_tenant_id uuid,
  p_professional_id uuid,
  p_location_id uuid,
  p_starts_at_utc timestamptz,
  p_ends_at_utc timestamptz,
  p_patient_name text,
  p_patient_document text,
  p_patient_birth_date date,
  p_patient_email text,
  p_patient_phone text,
  p_notes text default null
)
returns uuid
language plpgsql
security definer
set search_path = appointments, patients, scheduling, core, pg_temp
as $$
declare
  v_patient_id uuid;
  v_appointment_id uuid := gen_random_uuid();
  v_contact_note text;
begin
  if p_tenant_id is null or p_professional_id is null then
    raise exception 'discovery_booking_invalid' using errcode = '22023';
  end if;

  if p_ends_at_utc <= p_starts_at_utc then
    raise exception 'discovery_booking_invalid' using errcode = '22023';
  end if;

  if p_starts_at_utc < now() then
    raise exception 'discovery_booking_invalid' using errcode = '22023';
  end if;

  if not exists (
    select 1
    from core.tenant t
    where t.id = p_tenant_id and t.active = true
  ) then
    raise exception 'clinic_not_found' using errcode = '02000';
  end if;

  if not exists (
    select 1
    from scheduling.professional p
    where p.tenant_id = p_tenant_id
      and p.id = p_professional_id
      and p.active = true
  ) then
    raise exception 'professional_not_found' using errcode = '02000';
  end if;

  if p_location_id is not null and not exists (
    select 1
    from scheduling.location l
    where l.tenant_id = p_tenant_id
      and l.id = p_location_id
      and l.active = true
  ) then
    raise exception 'location_not_found' using errcode = '02000';
  end if;

  insert into patients.patient (id, tenant_id, full_name, document, birth_date)
  values (
    gen_random_uuid(),
    p_tenant_id,
    trim(p_patient_name),
    trim(p_patient_document),
    p_patient_birth_date
  )
  on conflict (tenant_id, document) do update
  set full_name = excluded.full_name,
      birth_date = excluded.birth_date
  returning id into v_patient_id;

  v_contact_note := concat_ws(
    E'\n',
    'Agendamento solicitado pelo Discovery.',
    nullif('E-mail: ' || trim(coalesce(p_patient_email, '')), 'E-mail: '),
    nullif('Telefone: ' || trim(coalesce(p_patient_phone, '')), 'Telefone: '),
    nullif('Observacoes: ' || trim(coalesce(p_notes, '')), 'Observacoes: ')
  );

  insert into appointments.appointment (
    id,
    tenant_id,
    patient_id,
    professional_id,
    location_id,
    starts_at_utc,
    ends_at_utc,
    status,
    notes
  )
  values (
    v_appointment_id,
    p_tenant_id,
    v_patient_id,
    p_professional_id,
    p_location_id,
    p_starts_at_utc,
    p_ends_at_utc,
    'scheduled',
    v_contact_note
  );

  return v_appointment_id;
exception
  when exclusion_violation then
    raise exception 'appointment_conflict' using errcode = '23P01';
end;
$$;

select format(
  'grant execute on function scheduling.discovery_search(text, text, text, text, text, integer) to %I',
  :'app_db_user')
\gexec

select format(
  'grant execute on function appointments.discovery_list_busy_windows(uuid, uuid, uuid, timestamptz, timestamptz) to %I',
  :'app_db_user')
\gexec

select format(
  'grant execute on function appointments.discovery_book_appointment(uuid, uuid, uuid, timestamptz, timestamptz, text, text, date, text, text, text) to %I',
  :'app_db_user')
\gexec
