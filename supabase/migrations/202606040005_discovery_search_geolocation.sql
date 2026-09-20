\set ON_ERROR_STOP on

create or replace function scheduling.discovery_search(
  p_mode text default 'professional',
  p_query text default null,
  p_clinic text default null,
  p_specialty text default null,
  p_region text default null,
  p_take integer default 20,
  p_latitude numeric default null,
  p_longitude numeric default null
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
      case when lower(coalesce(p_mode, 'professional')) = 'clinic' then 'clinic' else 'professional' end as mode,
      p_latitude::float8 as latitude,
      p_longitude::float8 as longitude
  ),
  candidates as (
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
      ) as region_label,
      case
        when n.latitude is not null
          and n.longitude is not null
          and l.latitude is not null
          and l.longitude is not null
        then
          power((l.latitude::float8 - n.latitude), 2)
          + power((l.longitude::float8 - n.longitude) * cos(radians(n.latitude)), 2)
        else null
      end as distance_score,
      n.mode,
      n.take_count
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
  )
  select
    c.tenant_id,
    c.professional_id,
    c.location_id,
    c.professional_name,
    c.specialty_name,
    c.clinic_name,
    c.location_name,
    c.city,
    c.state,
    c.region_label
  from candidates c
  order by
    c.distance_score nulls last,
    case when c.mode = 'clinic' then lower(c.clinic_name) else lower(c.professional_name) end,
    lower(c.specialty_name),
    lower(coalesce(c.city, '')),
    lower(coalesce(c.location_name, ''))
  limit (select take_count from normalized);
$$;

select format(
  'grant execute on function scheduling.discovery_search(text, text, text, text, text, integer, numeric, numeric) to %I',
  :'app_db_user')
\gexec
