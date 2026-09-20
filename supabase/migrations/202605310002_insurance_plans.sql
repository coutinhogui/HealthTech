create table if not exists patients.insurance_plan (
  id uuid primary key,
  tenant_id uuid not null references core.tenant(id),
  payer_name varchar(120) not null,
  plan_name varchar(120) not null,
  active boolean not null default true,
  created_at_utc timestamptz not null default now(),
  unique (tenant_id, id),
  unique (tenant_id, payer_name, plan_name)
);

create table if not exists patients.patient_plan (
  tenant_id uuid not null,
  patient_id uuid not null,
  plan_id uuid not null,
  active boolean not null default true,
  linked_at_utc timestamptz not null default now(),
  primary key (tenant_id, patient_id, plan_id),
  foreign key (tenant_id, patient_id) references patients.patient(tenant_id, id),
  foreign key (tenant_id, plan_id) references patients.insurance_plan(tenant_id, id)
);

create index if not exists ix_insurance_plan_tenant_payer_name on patients.insurance_plan(tenant_id, payer_name, plan_name);
create index if not exists ix_patient_plan_tenant_patient on patients.patient_plan(tenant_id, patient_id);

alter table patients.insurance_plan enable row level security;
alter table patients.patient_plan enable row level security;

alter table patients.insurance_plan force row level security;
alter table patients.patient_plan force row level security;

drop policy if exists tenant_isolation on patients.insurance_plan;
create policy tenant_isolation on patients.insurance_plan
  using (tenant_id::text = current_setting('app.tenant_id', true))
  with check (tenant_id::text = current_setting('app.tenant_id', true));

drop policy if exists tenant_isolation on patients.patient_plan;
create policy tenant_isolation on patients.patient_plan
  using (tenant_id::text = current_setting('app.tenant_id', true))
  with check (tenant_id::text = current_setting('app.tenant_id', true));
