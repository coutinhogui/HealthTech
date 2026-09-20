# Operacao Supabase, OAuth e administracao

## Escopo e invariantes

O Supabase remoto fornece Postgres e Auth. O browser nunca recebe tokens do
Supabase: o Gateway/BFF executa o fluxo OAuth com PKCE, guarda tokens no servidor
e emite apenas o cookie `HealthTech.Bff` com `HttpOnly`, `Secure` e
`SameSite=Strict`.

Regras operacionais:

- Nunca executar `supabase/seed.sql` no projeto remoto.
- Nunca aplicar `202605310001_application_role.sql` no Supabase gerenciado.
- Migrations estruturais novas devem ser SQL PostgreSQL puro.
- A role local `healthtech_app` e seus grants sao criados por
  `supabase/runtime-role.sql`, depois das migrations, somente no Compose.
- Funcoes privilegiadas revogam `EXECUTE` de `PUBLIC`; o bootstrap local concede
  execucao apenas a role de runtime.
- Clinicas, memberships e administradores sao desativados logicamente. Nao ha
  exclusao fisica no fluxo administrativo.

## CLI fixada e migrations

Use a versao oficial fixada para evitar diferencas entre maquinas:

```powershell
npx supabase@2.109.1 --version
npx supabase@2.109.1 migration new nome_da_migration
```

No Compose, `db-migrate` aplica migrations estruturais como `postgres`, executa
`supabase/runtime-role.sql`, aplica o seed local idempotente e termina com codigo
zero. Verifique com:

```powershell
docker compose up -d --build
docker compose ps -a
docker compose logs db-migrate
```

O estado esperado e `healthtech-db-migrate` como `exited (0)` e todos os servicos
long-running como `healthy`.

## Reconciliacao de um projeto remoto

Antes de qualquer DDL, registre:

- estado e versao do projeto;
- historico de migrations;
- schemas, tabelas, funcoes, roles e contagens;
- advisors de seguranca e performance.

Escolha uma das rotas:

1. Banco vazio: aplique todas as migrations estruturais em ordem, sem seed.
2. Historico alinhado: aplique apenas migrations ainda ausentes.
3. Schema sem historico: prove as pos-condicoes de cada versao, registre somente
   baselines comprovados e crie objetos ausentes por migration incremental.

Depois, repita inventario, contagens e advisors. Todas as tabelas de aplicacao
devem usar RLS. `core.system_admin_user` possui RLS sem policy deliberadamente:
acesso direto e negado, e as operacoes autorizadas passam por funcoes
`SECURITY DEFINER`. O aviso informativo `rls_enabled_no_policy` e esperado.
Indices recem-criados podem aparecer como nao usados enquanto o banco estiver
vazio; nao os remova antes de observar carga real.

## Google OAuth local

No Google Cloud, o OAuth client usado pelo Supabase deve autorizar o callback
estavel do projeto:

```text
https://xzsmjroigbxaexstaufg.supabase.co/auth/v1/callback
```

No Supabase Auth, ative Google com esse client e configure:

- Site URL local: `http://localhost:5191`.
- Redirect URL permitida: `http://localhost:5191/auth/callback`.

No `.env` local ignorado pelo Git, configure sem registrar valores secretos:

```text
HEALTHTECH_SUPABASE_URL=https://xzsmjroigbxaexstaufg.supabase.co
HEALTHTECH_SUPABASE_ANON_KEY=<publishable-key-ativa>
HEALTHTECH_SUPABASE_AUTHORITY=https://xzsmjroigbxaexstaufg.supabase.co/auth/v1
HEALTHTECH_SUPABASE_ISSUER=https://xzsmjroigbxaexstaufg.supabase.co/auth/v1
HEALTHTECH_PUBLIC_BASE_URL=http://localhost:5026
HEALTHTECH_BFF_ENABLE_DEV_AUTH=false
```

O frontend inicia o login pelo Gateway, pede ao Supabase que retorne para
`http://localhost:5191/auth/callback` e essa pagina conclui a troca de codigo no
endpoint server-side `/api/auth/callback`. Nao use URL temporaria de tunnel no
ambiente local. Se o navegador realmente recusar cookies `Secure` em localhost,
use HTTPS local temporario em vez de enfraquecer os atributos do cookie.

## Bootstrap do primeiro administrador

Depois do primeiro login Google, localize o usuario em `auth.users` pelo e-mail
esperado e insira exatamente esse `id` e e-mail em `core.system_admin_user`.
Esse bootstrap e dado operacional controlado, nao seed demo. Depois que existir
um administrador ativo, use a tela `/admin/sistema/usuarios` ou a API
`GET/PUT /api/admin/system-users`.

A funcao de upsert bloqueia concorrentemente a desativacao do ultimo
`system_admin` ativo. A administracao de uma clinica inativa permite listar e
desativar admins existentes, mas impede criar ou reativar admins ate a clinica
ser reativada.

## Validacao de acesso e revogacao

Checklist minimo:

- Login Google para `accessArea=environment` entra em `/admin` somente se o
  usuario estiver ativo em `core.system_admin_user`.
- Login Google para `accessArea=clinic` exige membership ativo em
  `core.tenant_user` e clinica ativa.
- `GET /api/session` reconstruira memberships pelo banco e removera tenant ativo
  que tenha sido revogado.
- Requisicoes tenant-scoped revalidam usuario, membership e `core.tenant.active`
  e retornam `403 clinic_inactive_or_membership_revoked` quando necessario.
- Headers `X-Tenant-Id` enviados pelo browser sao rejeitados com `403`.
- Logout remove o cookie BFF; `localStorage` e `sessionStorage` nao contem tokens.

Execute tambem:

```powershell
dotnet restore HealthTech.sln
dotnet build HealthTech.sln --no-restore
dotnet test HealthTech.sln --no-build
```

Para a integracao de banco, valide migrations em PostgreSQL 16 (Compose/CI) e
PostgreSQL 17 (mesma major do Supabase remoto).
