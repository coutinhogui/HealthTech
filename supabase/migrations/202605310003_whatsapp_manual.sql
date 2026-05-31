create table if not exists appointments.whatsapp_manual_message (
  id uuid primary key,
  tenant_id uuid not null references core.tenant(id),
  appointment_id uuid not null,
  recipient_phone varchar(32) not null,
  message_text varchar(600) not null,
  status varchar(24) not null check (status in ('intent', 'sent')),
  provider varchar(80) null,
  provider_message_id varchar(160) null,
  created_by_subject varchar(120) null,
  created_at_utc timestamptz not null default now(),
  sent_at_utc timestamptz null,
  unique (tenant_id, id),
  foreign key (appointment_id) references appointments.appointment(id)
);

create index if not exists ix_whatsapp_manual_tenant_appointment_created
  on appointments.whatsapp_manual_message(tenant_id, appointment_id, created_at_utc desc);

alter table appointments.whatsapp_manual_message enable row level security;
alter table appointments.whatsapp_manual_message force row level security;

drop policy if exists tenant_isolation on appointments.whatsapp_manual_message;
create policy tenant_isolation on appointments.whatsapp_manual_message
  using (tenant_id::text = current_setting('app.tenant_id', true))
  with check (tenant_id::text = current_setting('app.tenant_id', true));
