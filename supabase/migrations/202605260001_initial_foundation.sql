create extension if not exists btree_gist;

create schema if not exists core;
create schema if not exists patients;
create schema if not exists scheduling;
create schema if not exists appointments;

create table if not exists core.tenant (
  id uuid primary key,
  name text not null,
  active boolean not null default true,
  created_at_utc timestamptz not null default now()
);

create table if not exists core.tenant_user (
  tenant_id uuid not null references core.tenant(id),
  subject_id text not null,
  email text not null,
  role text not null check (role in ('admin', 'professional', 'reception', 'billing')),
  active boolean not null default true,
  created_at_utc timestamptz not null default now(),
  primary key (tenant_id, subject_id),
  unique (tenant_id, email)
);

create table if not exists patients.patient (
  id uuid primary key,
  tenant_id uuid not null references core.tenant(id),
  full_name varchar(160) not null,
  document varchar(32) not null,
  birth_date date not null check (birth_date <= current_date),
  created_at_utc timestamptz not null default now(),
  unique (tenant_id, id),
  unique (tenant_id, document)
);

create table if not exists scheduling.specialty (
  id uuid primary key,
  tenant_id uuid not null references core.tenant(id),
  name varchar(120) not null,
  active boolean not null default true,
  unique (tenant_id, id),
  unique (tenant_id, name)
);

create table if not exists scheduling.location (
  id uuid primary key,
  tenant_id uuid not null references core.tenant(id),
  name varchar(120) not null,
  timezone varchar(64) not null default 'America/Sao_Paulo',
  active boolean not null default true,
  unique (tenant_id, id)
);

create table if not exists scheduling.professional (
  id uuid primary key,
  tenant_id uuid not null references core.tenant(id),
  full_name varchar(160) not null,
  specialty_id uuid not null,
  active boolean not null default true,
  unique (tenant_id, id),
  foreign key (tenant_id, specialty_id) references scheduling.specialty(tenant_id, id)
);

create table if not exists appointments.appointment (
  id uuid primary key,
  tenant_id uuid not null references core.tenant(id),
  patient_id uuid not null,
  professional_id uuid not null,
  location_id uuid null,
  starts_at_utc timestamptz not null,
  ends_at_utc timestamptz not null,
  status varchar(32) not null default 'scheduled',
  notes text null,
  created_at_utc timestamptz not null default now(),
  check (ends_at_utc > starts_at_utc),
  check (status in ('scheduled', 'confirmed', 'cancelled', 'completed', 'no_show')),
  foreign key (tenant_id, patient_id) references patients.patient(tenant_id, id),
  foreign key (tenant_id, professional_id) references scheduling.professional(tenant_id, id),
  foreign key (tenant_id, location_id) references scheduling.location(tenant_id, id)
);

alter table appointments.appointment
  drop constraint if exists no_overlapping_active_appointments;

alter table appointments.appointment
  add constraint no_overlapping_active_appointments
  exclude using gist (
    tenant_id with =,
    professional_id with =,
    tstzrange(starts_at_utc, ends_at_utc, '[)') with &&
  )
  where (status in ('scheduled', 'confirmed'));

create index if not exists ix_patient_tenant_full_name on patients.patient(tenant_id, full_name);
create index if not exists ix_appointment_tenant_starts_at on appointments.appointment(tenant_id, starts_at_utc);
create index if not exists ix_appointment_tenant_patient on appointments.appointment(tenant_id, patient_id, starts_at_utc desc);
create index if not exists ix_appointment_tenant_professional_window on appointments.appointment(tenant_id, professional_id, starts_at_utc, ends_at_utc);

alter table core.tenant enable row level security;
alter table core.tenant_user enable row level security;
alter table patients.patient enable row level security;
alter table scheduling.specialty enable row level security;
alter table scheduling.location enable row level security;
alter table scheduling.professional enable row level security;
alter table appointments.appointment enable row level security;

alter table core.tenant force row level security;
alter table core.tenant_user force row level security;
alter table patients.patient force row level security;
alter table scheduling.specialty force row level security;
alter table scheduling.location force row level security;
alter table scheduling.professional force row level security;
alter table appointments.appointment force row level security;

drop policy if exists tenant_isolation on core.tenant;
create policy tenant_isolation on core.tenant
  using (id::text = current_setting('app.tenant_id', true))
  with check (id::text = current_setting('app.tenant_id', true));

drop policy if exists tenant_isolation on core.tenant_user;
create policy tenant_isolation on core.tenant_user
  using (tenant_id::text = current_setting('app.tenant_id', true))
  with check (tenant_id::text = current_setting('app.tenant_id', true));

drop policy if exists tenant_isolation on patients.patient;
create policy tenant_isolation on patients.patient
  using (tenant_id::text = current_setting('app.tenant_id', true))
  with check (tenant_id::text = current_setting('app.tenant_id', true));

drop policy if exists tenant_isolation on scheduling.specialty;
create policy tenant_isolation on scheduling.specialty
  using (tenant_id::text = current_setting('app.tenant_id', true))
  with check (tenant_id::text = current_setting('app.tenant_id', true));

drop policy if exists tenant_isolation on scheduling.location;
create policy tenant_isolation on scheduling.location
  using (tenant_id::text = current_setting('app.tenant_id', true))
  with check (tenant_id::text = current_setting('app.tenant_id', true));

drop policy if exists tenant_isolation on scheduling.professional;
create policy tenant_isolation on scheduling.professional
  using (tenant_id::text = current_setting('app.tenant_id', true))
  with check (tenant_id::text = current_setting('app.tenant_id', true));

drop policy if exists tenant_isolation on appointments.appointment;
create policy tenant_isolation on appointments.appointment
  using (tenant_id::text = current_setting('app.tenant_id', true))
  with check (tenant_id::text = current_setting('app.tenant_id', true));
