\set ON_ERROR_STOP on

select format(
  'create role %I login password %L nosuperuser nocreatedb nocreaterole noreplication nobypassrls',
  :'app_db_user',
  :'app_db_password')
where not exists (
  select 1
  from pg_catalog.pg_roles
  where rolname = :'app_db_user'
)\gexec

select format(
  'alter role %I login password %L nosuperuser nocreatedb nocreaterole noreplication nobypassrls',
  :'app_db_user',
  :'app_db_password')
\gexec

select format('grant connect on database %I to %I', current_database(), :'app_db_user')\gexec
select format('grant usage on schema core, patients, scheduling, appointments to %I', :'app_db_user')\gexec
select format('revoke create on schema public, core, patients, scheduling, appointments from %I', :'app_db_user')\gexec
select format('grant select, insert, update, delete on all tables in schema core, patients, scheduling, appointments to %I', :'app_db_user')\gexec
select format('grant usage, select on all sequences in schema core, patients, scheduling, appointments to %I', :'app_db_user')\gexec
select format('grant execute on all functions in schema core, patients, scheduling, appointments to %I', :'app_db_user')\gexec

select format('revoke all on table core.system_admin_user from %I', :'app_db_user')\gexec
select format('revoke all on table patients.patient_identity from %I', :'app_db_user')\gexec
select format('grant select on table patients.patient_identity to %I', :'app_db_user')\gexec
