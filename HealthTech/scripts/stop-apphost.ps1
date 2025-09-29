param(
    [switch]$Force
)

$pidFile = Join-Path $PSScriptRoot "apphost.pid"

if (-not (Test-Path $pidFile)) {
    Write-Host "Nenhum processo AppHost registrado. Nada para parar." -ForegroundColor Yellow
    exit 0
}

$pidText = (Get-Content $pidFile | Select-Object -First 1).Trim()

$pidValue = 0
if (-not [int]::TryParse($pidText, [ref]$pidValue)) {
    Write-Warning "PID inválido encontrado em $pidFile. Limpando arquivo."
    Remove-Item $pidFile -ErrorAction SilentlyContinue
    exit 1
}

$process = Get-Process -Id $pidValue -ErrorAction SilentlyContinue

if (-not $process) {
    Write-Host "Processo $pidValue não está em execução. Limpando arquivo." -ForegroundColor Yellow
    Remove-Item $pidFile -ErrorAction SilentlyContinue
    exit 0
}

try {
    if ($Force) {
        Stop-Process -Id $pidValue -Force -ErrorAction Stop
    } else {
        Stop-Process -Id $pidValue -ErrorAction Stop
    }
    Wait-Process -Id $pidValue -ErrorAction SilentlyContinue
    Write-Host "AppHost encerrado (PID $pidValue)." -ForegroundColor Green
}
catch {
    Write-Error "Falha ao encerrar o AppHost: $($_.Exception.Message)"
    exit 1
}
finally {
    Remove-Item $pidFile -ErrorAction SilentlyContinue
}
