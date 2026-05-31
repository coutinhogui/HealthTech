create table if not exists patients.patient_evolution (
  id uuid primary key,
  tenant_id uuid not null,
  patient_id uuid not null,
  encountered_at_utc timestamptz not null,
  subjective text not null,
  objective text not null,
  assessment text not null,
  plan text not null,
  created_by_subject varchar(120) null,
  created_at_utc timestamptz not null default now(),
  unique (tenant_id, id),
  foreign key (tenant_id, patient_id) references patients.patient(tenant_id, id)
);

create index if not exists ix_patient_evolution_tenant_patient_encountered
  on patients.patient_evolution(tenant_id, patient_id, encountered_at_utc desc);

alter table patients.patient_evolution enable row level security;
alter table patients.patient_evolution force row level security;

drop policy if exists tenant_isolation on patients.patient_evolution;
create policy tenant_isolation on patients.patient_evolution
  using (tenant_id::text = current_setting('app.tenant_id', true))
  with check (tenant_id::text = current_setting('app.tenant_id', true));
