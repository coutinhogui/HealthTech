param(
    [switch]$NoBuild,
    [switch]$Detach
)

$repoRoot = Split-Path -Path $PSScriptRoot -Parent
$appHostDir = Join-Path $repoRoot "HealthTech.AppHost"
$projectPath = Join-Path $appHostDir "HealthTech.AppHost.csproj"
$pidFile = Join-Path $PSScriptRoot "apphost.pid"

if (-not (Test-Path $projectPath)) {
    Write-Error "AppHost project not found at $projectPath"
    exit 1
}

if (Test-Path $pidFile) {
    $existingPid = (Get-Content $pidFile | Select-Object -First 1).Trim()
    if ($existingPid) {
        $running = Get-Process -Id $existingPid -ErrorAction SilentlyContinue
        if ($running) {
            Write-Host "AppHost já está em execução (PID $existingPid)." -ForegroundColor Yellow
            exit 0
        }
    }
    Remove-Item $pidFile -ErrorAction SilentlyContinue
}

$arguments = @("run", "--project", $projectPath)
if ($NoBuild) {
    $arguments += "--no-build"
}

$startParams = @{
    FilePath = "dotnet"
    ArgumentList = $arguments
    WorkingDirectory = $repoRoot
    PassThru = $true
}

if ($Detach) {
    $startParams.WindowStyle = "Normal"
} else {
    $startParams.NoNewWindow = $true
}

try {
    $process = Start-Process @startParams
    $process.Id | Set-Content $pidFile

    if ($Detach) {
        Write-Host "AppHost iniciado em background (PID $($process.Id))." -ForegroundColor Green
        Write-Host "Uma nova janela foi aberta com a saída. Use stop-apphost.ps1/bat para encerrar." -ForegroundColor Gray
    } else {
        Write-Host "AppHost iniciado (PID $($process.Id)). Saída seguirá neste console." -ForegroundColor Green
        Write-Host "Procure mensagens 'Now listening on:' para identificar as portas." -ForegroundColor Gray
        Write-Host "Pressione Ctrl+C aqui para encerrar manualmente ou use stop-apphost.ps1/bat em outro terminal." -ForegroundColor Gray

        try {
            Wait-Process -Id $process.Id
        }
        finally {
            if (Test-Path $pidFile) {
                Remove-Item $pidFile -ErrorAction SilentlyContinue
            }
            Write-Host "AppHost foi encerrado." -ForegroundColor Yellow
        }
    }
}
catch {
    Write-Error "Falha ao iniciar o AppHost: $($_.Exception.Message)"
    exit 1
}
