\set ON_ERROR_STOP on

create or replace function core.discovery_list_clinics()
returns table (
  id uuid,
  name text,
  active boolean
)
language sql
security definer
set search_path = core, pg_temp
as $$
  select t.id, t.name, t.active
  from core.tenant t
  where t.active = true
  order by t.name;
$$;

create or replace function scheduling.discovery_list_specialties(
  p_tenant_id uuid
)
returns table (
  id uuid,
  name text,
  active boolean
)
language sql
security definer
set search_path = scheduling, core, pg_temp
as $$
  select s.id, s.name, s.active
  from scheduling.specialty s
  inner join core.tenant t on t.id = s.tenant_id
  where s.tenant_id = p_tenant_id
    and s.active = true
    and t.active = true
  order by s.name;
$$;

create or replace function scheduling.discovery_list_professionals(
  p_tenant_id uuid,
  p_specialty_id uuid default null
)
returns table (
  id uuid,
  full_name text,
  specialty_id uuid,
  specialty_name text,
  active boolean
)
language sql
security definer
set search_path = scheduling, core, pg_temp
as $$
  select p.id,
         p.full_name,
         p.specialty_id,
         s.name as specialty_name,
         p.active
  from scheduling.professional p
  inner join scheduling.specialty s
    on s.tenant_id = p.tenant_id and s.id = p.specialty_id
  inner join core.tenant t on t.id = p.tenant_id
  where p.tenant_id = p_tenant_id
    and p.active = true
    and s.active = true
    and t.active = true
    and (p_specialty_id is null or p.specialty_id = p_specialty_id)
  order by p.full_name;
$$;

create or replace function appointments.discovery_list_busy_windows(
  p_tenant_id uuid,
  p_professional_id uuid,
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
  order by a.starts_at_utc;
$$;

create or replace function appointments.discovery_book_appointment(
  p_tenant_id uuid,
  p_professional_id uuid,
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
    null,
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
  'grant execute on function core.discovery_list_clinics() to %I',
  :'app_db_user')
\gexec

select format(
  'grant execute on function scheduling.discovery_list_specialties(uuid) to %I',
  :'app_db_user')
\gexec

select format(
  'grant execute on function scheduling.discovery_list_professionals(uuid, uuid) to %I',
  :'app_db_user')
\gexec

select format(
  'grant execute on function appointments.discovery_list_busy_windows(uuid, uuid, timestamptz, timestamptz) to %I',
  :'app_db_user')
\gexec

select format(
  'grant execute on function appointments.discovery_book_appointment(uuid, uuid, timestamptz, timestamptz, text, text, date, text, text, text) to %I',
  :'app_db_user')
\gexec
