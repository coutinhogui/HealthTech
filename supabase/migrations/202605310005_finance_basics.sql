create table if not exists appointments.billing_batch (
  id uuid primary key,
  tenant_id uuid not null references core.tenant(id),
  competence_month date not null,
  total_amount numeric(12,2) not null check (total_amount >= 0),
  status varchar(24) not null check (status in ('closed')),
  created_by_subject varchar(120) null,
  created_at_utc timestamptz not null default now(),
  unique (tenant_id, id),
  unique (tenant_id, competence_month)
);

create table if not exists appointments.charge (
  id uuid primary key,
  tenant_id uuid not null references core.tenant(id),
  appointment_id uuid not null references appointments.appointment(id),
  patient_id uuid not null,
  payer_type varchar(24) not null check (payer_type in ('insurance', 'private')),
  description varchar(200) not null,
  amount numeric(12,2) not null check (amount > 0),
  due_date date not null,
  status varchar(24) not null check (status in ('open', 'paid', 'batched', 'overdue')),
  paid_at_utc timestamptz null,
  batch_id uuid null,
  created_by_subject varchar(120) null,
  created_at_utc timestamptz not null default now(),
  unique (tenant_id, id),
  foreign key (tenant_id, patient_id) references patients.patient(tenant_id, id),
  foreign key (tenant_id, batch_id) references appointments.billing_batch(tenant_id, id)
);

create index if not exists ix_charge_tenant_due_status
  on appointments.charge(tenant_id, due_date, status);
create index if not exists ix_charge_tenant_patient
  on appointments.charge(tenant_id, patient_id, created_at_utc desc);
create index if not exists ix_charge_tenant_batch
  on appointments.charge(tenant_id, batch_id);

alter table appointments.billing_batch enable row level security;
alter table appointments.charge enable row level security;
alter table appointments.billing_batch force row level security;
alter table appointments.charge force row level security;

drop policy if exists tenant_isolation on appointments.billing_batch;
create policy tenant_isolation on appointments.billing_batch
  using (tenant_id::text = current_setting('app.tenant_id', true))
  with check (tenant_id::text = current_setting('app.tenant_id', true));

drop policy if exists tenant_isolation on appointments.charge;
create policy tenant_isolation on appointments.charge
  using (tenant_id::text = current_setting('app.tenant_id', true))
  with check (tenant_id::text = current_setting('app.tenant_id', true));
