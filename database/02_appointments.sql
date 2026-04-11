CREATE SCHEMA IF NOT EXISTS appointments;

CREATE TABLE IF NOT EXISTS appointments.appointment (
  id uuid PRIMARY KEY,
  tenant_id uuid NOT NULL,
  patient_id uuid NOT NULL,
  professional_id uuid NOT NULL,
  starts_at_utc timestamptz NOT NULL,
  ends_at_utc timestamptz NOT NULL,
  status varchar(32) NOT NULL,
  notes text NULL
);

CREATE INDEX IF NOT EXISTS ix_appointment_tenant_starts_at
  ON appointments.appointment(tenant_id, starts_at_utc);

CREATE INDEX IF NOT EXISTS ix_appointment_tenant_professional_window
  ON appointments.appointment(tenant_id, professional_id, starts_at_utc);
