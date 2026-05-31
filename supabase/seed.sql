insert into core.tenant (id, name)
values ('11111111-1111-1111-1111-111111111111', 'Demo Clinic')
on conflict (id) do update set name = excluded.name;

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

insert into scheduling.specialty (id, tenant_id, name)
values (
  '22222222-2222-2222-2222-222222222222',
  '11111111-1111-1111-1111-111111111111',
  'Clinica geral'
)
on conflict (tenant_id, name) do update set active = true;

insert into scheduling.location (id, tenant_id, name)
values (
  '33333333-3333-3333-3333-333333333333',
  '11111111-1111-1111-1111-111111111111',
  'Unidade principal'
)
on conflict (tenant_id, id) do update set active = true;

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

insert into patients.insurance_plan (id, tenant_id, payer_name, plan_name)
values (
  '44444444-4444-4444-4444-444444444444',
  '11111111-1111-1111-1111-111111111111',
  'Unimed',
  'Unimed Basico'
)
on conflict (tenant_id, payer_name, plan_name) do update
set active = true;
