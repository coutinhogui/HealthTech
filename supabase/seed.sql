insert into core.tenant (id, name)
values ('11111111-1111-1111-1111-111111111111', 'Demo Clinic')
on conflict (id) do update set name = excluded.name;

insert into core.tenant (id, name)
values
  ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 'Cardio Prime'),
  ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 'Derma Center Paulista')
on conflict (id) do update
set name = excluded.name,
    active = true;

insert into core.tenant_user (tenant_id, subject_id, email, role)
values (
  '11111111-1111-1111-1111-111111111111',
  'bootstrap-admin',
  'admin@healthtech.local',
  'admin'
)
on conflict (tenant_id, subject_id) do update
set email = excluded.email,
    role = excluded.role,
    active = true;

insert into core.system_admin_user (subject_id, email, active)
select
  coalesce(nullif(:'system_admin_subject_id', ''), :'system_admin_email'),
  :'system_admin_email',
  true
where nullif(:'system_admin_email', '') is not null
on conflict (subject_id) do update
set email = excluded.email,
    active = true;

insert into scheduling.specialty (id, tenant_id, name)
values (
  '22222222-2222-2222-2222-222222222222',
  '11111111-1111-1111-1111-111111111111',
  'Clinica geral'
)
on conflict (tenant_id, name) do update set active = true;

insert into scheduling.specialty (id, tenant_id, name)
values
  ('22222222-2222-2222-2222-222222222223', '11111111-1111-1111-1111-111111111111', 'Cardiologia'),
  ('22222222-2222-2222-2222-222222222224', '11111111-1111-1111-1111-111111111111', 'Dermatologia')
on conflict (tenant_id, name) do update set active = true;

insert into scheduling.specialty (id, tenant_id, name)
values
  ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb101', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 'Cardiologia'),
  ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb102', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 'Clinica geral'),
  ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb201', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 'Dermatologia'),
  ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb202', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 'Nutrologia')
on conflict (tenant_id, name) do update set active = true;

insert into scheduling.location (id, tenant_id, name)
values (
  '33333333-3333-3333-3333-333333333333',
  '11111111-1111-1111-1111-111111111111',
  'Unidade principal'
)
on conflict (tenant_id, id) do update set active = true;

insert into scheduling.location (
  id,
  tenant_id,
  name,
  timezone,
  address_line,
  neighborhood,
  city,
  state,
  postal_code,
  latitude,
  longitude,
  public_region
)
values
  (
    '33333333-3333-3333-3333-333333333333',
    '11111111-1111-1111-1111-111111111111',
    'Unidade Centro',
    'America/Sao_Paulo',
    'Rua das Clinicas, 100',
    'Centro',
    'Sao Paulo',
    'SP',
    '01000-000',
    -23.550520,
    -46.633308,
    'Centro'
  ),
  (
    '33333333-3333-3333-3333-333333333334',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1',
    'Cardio Prime Jardins',
    'America/Sao_Paulo',
    'Alameda Saude, 450',
    'Jardins',
    'Sao Paulo',
    'SP',
    '01400-000',
    -23.567300,
    -46.658700,
    'Jardins'
  ),
  (
    '33333333-3333-3333-3333-333333333335',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2',
    'Derma Center Barra',
    'America/Sao_Paulo',
    'Avenida das Americas, 3200',
    'Barra da Tijuca',
    'Rio de Janeiro',
    'RJ',
    '22640-102',
    -23.000700,
    -43.365900,
    'Barra da Tijuca'
  )
on conflict (tenant_id, id) do update
set name = excluded.name,
    timezone = excluded.timezone,
    address_line = excluded.address_line,
    neighborhood = excluded.neighborhood,
    city = excluded.city,
    state = excluded.state,
    postal_code = excluded.postal_code,
    latitude = excluded.latitude,
    longitude = excluded.longitude,
    public_region = excluded.public_region,
    active = true;

insert into scheduling.professional (id, tenant_id, full_name, specialty_id)
values (
  'cccccccc-cccc-cccc-cccc-cccccccccccc',
  '11111111-1111-1111-1111-111111111111',
  'Dra. Ana Costa',
  '22222222-2222-2222-2222-222222222222'
)
on conflict (tenant_id, id) do update
set full_name = excluded.full_name,
    specialty_id = excluded.specialty_id,
    active = true;

insert into scheduling.professional (id, tenant_id, full_name, specialty_id)
values
  ('cccccccc-cccc-cccc-cccc-ccccccccccd1', '11111111-1111-1111-1111-111111111111', 'Dr. Bruno Lima', '22222222-2222-2222-2222-222222222223'),
  ('cccccccc-cccc-cccc-cccc-ccccccccccd2', '11111111-1111-1111-1111-111111111111', 'Dra. Marina Rocha', '22222222-2222-2222-2222-222222222224')
on conflict (tenant_id, id) do update
set full_name = excluded.full_name,
    specialty_id = excluded.specialty_id,
    active = true;

insert into scheduling.professional (id, tenant_id, full_name, specialty_id)
values
  ('dddddddd-dddd-dddd-dddd-dddddddddd11', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 'Dra. Laura Martins', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb101'),
  ('dddddddd-dddd-dddd-dddd-dddddddddd12', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 'Dr. Renato Alves', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb102'),
  ('dddddddd-dddd-dddd-dddd-dddddddddd21', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 'Dra. Beatriz Nogueira', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb201'),
  ('dddddddd-dddd-dddd-dddd-dddddddddd22', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 'Dr. Caio Ferraz', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb202')
on conflict (tenant_id, id) do update
set full_name = excluded.full_name,
    specialty_id = excluded.specialty_id,
    active = true;

insert into patients.insurance_plan (id, tenant_id, payer_name, plan_name)
values (
  '44444444-4444-4444-4444-444444444444',
  '11111111-1111-1111-1111-111111111111',
  'Unimed',
  'Unimed Basico'
)
on conflict (tenant_id, payer_name, plan_name) do update
set active = true;
