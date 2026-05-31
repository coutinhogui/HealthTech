# HealthTech Integration Checkpoint Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Transform the current rearquitecture work into an integration-ready checkpoint with a clean diff, reproducible Docker operation, verified security behavior, and clear handoff notes.

**Architecture:** No new product feature is introduced in this phase. The work is limited to operational hardening, cleanup, documentation, and verification around the existing BFF, Compose stack, Supabase migrations, Patients, Appointments, EHR, Billing, and Gateway multi-tenant enforcement.

**Tech Stack:** .NET 10, Blazor WebAssembly, ASP.NET Core Gateway/BFF, YARP, Docker Compose, PostgreSQL 16, Supabase-style SQL migrations, xUnit, GitHub Actions.

---

### Task 1: Worktree Inventory

**Files:**
- Read: `git status --short`
- Read: `git diff --stat`
- Read: `docs/rearchitecture-implementation-notes.md`
- Read: `README.md`

- [ ] **Step 1: Capture current branch and changed-file count**

Run:

```powershell
git branch --show-current
git status --short
git diff --stat
```

Expected: branch is not `main` or `master`; changed files are understood before cleanup.

- [ ] **Step 2: Classify changed files into buckets**

Use these buckets:

```text
Platform: global.json, Directory.Build.props, solution, package/project files
Docker/CI: compose.yml, docker/, .github/, .dockerignore, scripts/
Gateway/Auth: src/gateway/, src/building-blocks/Abstractions/
Database: supabase/
Backend: src/services/Patients/, src/services/Appointments/, src/services/Identity/
Frontend: src/frontends/
Tests: tests/
Docs: README.md, docs/
Removed scaffolding: Component1, ExampleJsInterop, project-dump artifacts
```

Expected: no unexplained generated or sensitive file remains in the integration set.

### Task 2: Local Artifact Cleanup

**Files:**
- Check: `artifacts/`
- Check: `.gitignore`
- Check: `.dockerignore`

- [ ] **Step 1: Confirm local QA artifacts are ignored**

Run:

```powershell
git check-ignore -v artifacts/qa/mobile-pacientes-fixed.png
```

Expected: ignored by `.gitignore` through `[Aa]rtifacts/`.

- [ ] **Step 2: Keep QA screenshots local only**

Run:

```powershell
git status --short artifacts
```

Expected: no tracked or untracked artifact files appear.

### Task 3: Documentation Consistency

**Files:**
- Modify if needed: `README.md`
- Modify if needed: `docs/rearchitecture-implementation-notes.md`
- Modify if needed: `.env.example`

- [ ] **Step 1: Verify Docker commands are documented**

Run:

```powershell
rg -n "docker compose|healthtech-docker|down -v|pause|stop|localhost:5191|localhost:5026" README.md docs .env.example
```

Expected: normal start, reset, stop/pause/down/down -v, AppShell URL, Gateway URL, and dev auth behavior are documented.

- [ ] **Step 2: Verify sensitive examples use placeholders**

Run:

```powershell
rg -n "<known-secret-patterns>|<known-project-ref>|<known-password-pattern>|<direct-postgres-url-pattern>" README.md docs .env.example src/apphost/appsettings.json
```

Expected: no real Supabase project ref, password, publishable key, secret key, or direct database URL. Placeholder URLs may remain only in docs.

### Task 4: Docker and Smoke Verification

**Files:**
- Read: `compose.yml`
- Read: `scripts/healthtech-docker.ps1`
- Read: `.github/workflows/ci.yml`

- [ ] **Step 1: Validate Compose syntax**

Run:

```powershell
docker compose config --quiet
```

Expected: exit code 0.

- [ ] **Step 2: Confirm services are grouped and healthy**

Run:

```powershell
docker compose ps
```

Expected: `app-shell`, `gateway`, `patients-api`, `appointments-api`, `identity-api`, and `postgres` are healthy; `db-migrate` exited 0.

- [ ] **Step 3: Re-run real Gateway smoke**

Run a smoke that verifies:

```text
GET /health -> 200
GET /api/patients anonymous -> 401
POST /api/auth/login -> 200 and HttpOnly cookie
GET /api/session with cookie -> authenticated true
GET /api/professionals -> at least one row
GET /api/locations -> at least one row
POST /api/patients -> creates real patient
POST /api/appointments -> creates real appointment
forged X-Tenant-Id read/write -> 403
```

Expected: all checks pass through `http://localhost:5026`.

### Task 5: Build, Test, and Audit

**Files:**
- Read: `HealthTech.sln`
- Read: test projects under `tests/`

- [ ] **Step 1: Run restore**

Run:

```powershell
dotnet restore HealthTech.sln
```

Expected: exit code 0.

- [ ] **Step 2: Run Release build**

Run:

```powershell
dotnet build HealthTech.sln -c Release --no-restore
```

Expected: exit code 0, no errors.

- [ ] **Step 3: Run tests**

Run:

```powershell
dotnet test HealthTech.sln -c Release --no-build -v minimal
```

Expected: all tests pass.

- [ ] **Step 4: Run NuGet vulnerability audit**

Run:

```powershell
dotnet list HealthTech.sln package --vulnerable --include-transitive
```

Expected: no vulnerable packages.

### Task 6: Integration Handoff

**Files:**
- Read: `git status --short`
- Read: `git diff --stat`

- [ ] **Step 1: Produce final status**

Run:

```powershell
git status --short
git diff --stat
```

Expected: final response lists what changed by subsystem and what remains outside local verification.

- [ ] **Step 2: Do not commit without explicit approval**

Expected: no `git add`, `git commit`, `git push`, or PR creation unless the user explicitly asks for it.
