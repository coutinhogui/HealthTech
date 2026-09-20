CREATE SCHEMA IF NOT EXISTS patients;

CREATE TABLE IF NOT EXISTS patients.patient (
  id uuid PRIMARY KEY,
  tenant_id uuid NOT NULL,
  full_name varchar(160) NOT NULL,
  document varchar(32) NOT NULL,
  birth_date date NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_patient_tenant_full_name ON patients.patient(tenant_id, full_name);
CREATE UNIQUE INDEX IF NOT EXISTS ux_patient_tenant_document ON patients.patient(tenant_id, document);
