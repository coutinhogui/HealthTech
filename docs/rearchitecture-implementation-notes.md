# HealthTech Rearquitetura - Primeira Onda

## Decisoes aplicadas

- Runtime alvo: .NET 10 LTS via `global.json` e `net10.0`.
- Autenticacao browser: BFF no gateway com cookie `HttpOnly`, `Secure` e `SameSite=Strict`.
- Supabase no browser: removido do `wwwroot/appsettings.json`; o cliente chama o BFF.
- OAuth: o Gateway possui fluxo server-side com PKCE, cookie temporario de correlacao e `/api/auth/providers` para habilitar provedores apenas quando configurados.
- Dados: migrations versionadas em `supabase/migrations` com RLS, FKs e constraint de conflito de agenda.
- CI: GitHub Actions executa restore, build, test, inventario de vulnerabilidades e secret scan.

## Segredos

A connection string Supabase que estava versionada foi removida de `src/apphost/appsettings.json`.
Rotacione a senha no Supabase antes de reutilizar o projeto em qualquer ambiente conectado.
Use um usuario de aplicacao com privilegios minimos para `PatientsDb`/`AppointmentsDb`; nao use `postgres`/superuser em runtime. As tabelas tenant-scoped usam `FORCE ROW LEVEL SECURITY`, mas superuser ainda consegue bypassar RLS no Postgres.

## Supabase

Nao registre URLs, publishable keys, secret keys, senhas ou connection strings reais neste arquivo.
Use placeholders nos exemplos e configure valores reais apenas via `user-secrets`, variaveis de ambiente ou secrets do CI.

```powershell
supabase login
supabase init
supabase link --project-ref <project-ref>
```

```env
DATABASE_URL="postgresql://<db-user>.<project-ref>:<password>@<pooler-host>:6543/postgres?pgbouncer=true"
DIRECT_URL="postgresql://<db-user>.<project-ref>:<password>@<pooler-host>:5432/postgres"
```

1. Add the Supabase MCP server to Codex
Run this command to add the server.
Code:
File: Code
```
codex mcp add supabase --url https://mcp.supabase.com/mcp?project_ref=<project-ref>
```

2. Authenticate
Run the authentication command.
Code:
File: Code
```
codex mcp login supabase
```

3. Verify authentication
Run /mcp inside Codex to verify.
Details:
Run /mcp inside Codex to verify authentication.

4. Install Agent Skills (Optional)
Agent Skills give AI coding tools ready-made instructions, scripts, and resources for working with Supabase more accurately and efficiently.
Details:
npx skills add supabase/agent-skills
Code:
File: Code
```
npx skills add supabase/agent-skills
```


Para desenvolvimento local, configure:

```powershell
dotnet user-secrets set "ConnectionStrings:PatientsDb" "<connection-string>" --project src/apphost/HealthTech.AppHost.csproj
dotnet user-secrets set "Bff:SupabaseUrl" "https://<project>.supabase.co" --project src/gateway/HealthTech.Gateway.csproj
dotnet user-secrets set "Bff:SupabaseAnonKey" "<anon-key>" --project src/gateway/HealthTech.Gateway.csproj
dotnet user-secrets set "Bff:PublicBaseUrl" "http://localhost:5026" --project src/gateway/HealthTech.Gateway.csproj
```

Para OAuth local com AppShell separado, mantenha `http://localhost:5191` em `Cors:AllowedOrigins`; o AppShell usa `/auth/callback` e o BFF troca o codigo com Supabase sem expor tokens ao browser.

## Validacao local

```powershell
dotnet restore HealthTech.sln
dotnet build HealthTech.sln -c Release --no-restore
dotnet test HealthTech.sln -c Release --no-build
```

## Validacao de banco real

Os testes em `tests/HealthTech.Database.Tests` executam contratos SQL sempre. A parte de integracao com Postgres real roda quando `HEALTHTECH_TEST_DATABASE` aponta para um banco descartavel com `test` no nome:

```powershell
docker run --name healthtech-postgres-test -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=healthtech_test -p 55432:5432 -d postgres:16
$env:HEALTHTECH_TEST_DATABASE='Host=localhost;Port=55432;Database=healthtech_test;Username=postgres;Password=postgres;Include Error Detail=true'
dotnet test tests/HealthTech.Database.Tests/HealthTech.Database.Tests.csproj -c Release
```

Essa suite aplica a migration inicial, cria uma role de aplicacao com privilegios minimos e valida RLS por tenant, bloqueio de insert cross-tenant e constraint de conflito de agenda.

## Onda 3 - Separacao operacional do banco

Separacao atual:

- **Migrations estruturais/admin**: `supabase/migrations/202605260001_initial_foundation.sql` e demais migrations de schema/constraints/RLS.
- **Role de aplicacao (least privilege)**: `supabase/migrations/202605310001_application_role.sql`.
- **Seed demo idempotente**: `supabase/seed.sql`.

Regras aplicadas na role de aplicacao:

- `NOBYPASSRLS`, `NOSUPERUSER`, `NOCREATEDB`, `NOCREATEROLE`, `NOREPLICATION`.
- Sem privilegio `CREATE` em `public`, `core`, `patients`, `scheduling`, `appointments`.
- Apenas `USAGE` em schemas de dominio + `SELECT/INSERT/UPDATE/DELETE` em tabelas + `USAGE/SELECT` em sequences.

Cobertura de testes de banco:

- app role nao bypassa RLS;
- app role nao consegue `CREATE`/`DROP`;
- agendamento ativo conflitante bloqueia;
- agendamento cancelado nao bloqueia novo no mesmo horario;
- seed demo continua idempotente (incluindo convenio demo).
