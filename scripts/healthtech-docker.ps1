param(
    [ValidateSet("up", "start", "stop", "pause", "unpause", "restart", "down", "reset", "ps", "logs")]
    [string]$Action = "ps"
)

$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    switch ($Action) {
        "up" {
            docker compose up -d --build
        }
        "start" {
            docker compose start
        }
        "stop" {
            docker compose stop
        }
        "pause" {
            docker compose pause
        }
        "unpause" {
            docker compose unpause
        }
        "restart" {
            docker compose restart
        }
        "down" {
            docker compose down
        }
        "reset" {
            docker compose down -v
            docker compose up -d --build
        }
        "ps" {
            docker compose ps
        }
        "logs" {
            docker compose logs -f
        }
    }
}
finally {
    Pop-Location
}
