CREATE SCHEMA IF NOT EXISTS patients;
CREATE TABLE IF NOT EXISTS patients.patient (
  id uuid PRIMARY KEY,
  full_name varchar(160) NOT NULL,
  document varchar(32) NOT NULL,
  birth_date date NOT NULL
);
CREATE INDEX IF NOT EXISTS ix_patient_full_name ON patients.patient(full_name);
