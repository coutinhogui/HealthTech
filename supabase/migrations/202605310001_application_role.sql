\set ON_ERROR_STOP on

SELECT format(
  'CREATE ROLE %I LOGIN PASSWORD %L NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS',
  :'app_db_user',
  :'app_db_password')
WHERE NOT EXISTS (
  SELECT 1
  FROM pg_catalog.pg_roles
  WHERE rolname = :'app_db_user'
)\gexec

SELECT format(
  'ALTER ROLE %I LOGIN PASSWORD %L NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS',
  :'app_db_user',
  :'app_db_password'
)\gexec

SELECT format(
  'GRANT CONNECT ON DATABASE %I TO %I',
  current_database(),
  :'app_db_user'
)\gexec

SELECT format(
  'GRANT USAGE ON SCHEMA core, patients, scheduling, appointments TO %I',
  :'app_db_user'
)\gexec

SELECT format(
  'REVOKE CREATE ON SCHEMA public FROM %I',
  :'app_db_user'
)\gexec

SELECT format(
  'REVOKE CREATE ON SCHEMA core, patients, scheduling, appointments FROM %I',
  :'app_db_user'
)\gexec

SELECT format(
  'GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA core, patients, scheduling, appointments TO %I',
  :'app_db_user'
)\gexec

SELECT format(
  'GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA core, patients, scheduling, appointments TO %I',
  :'app_db_user'
)\gexec

SELECT format(
  'ALTER DEFAULT PRIVILEGES IN SCHEMA core, patients, scheduling, appointments GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO %I',
  :'app_db_user'
)\gexec

SELECT format(
  'ALTER DEFAULT PRIVILEGES IN SCHEMA core, patients, scheduling, appointments GRANT USAGE, SELECT ON SEQUENCES TO %I',
  :'app_db_user'
)\gexec
