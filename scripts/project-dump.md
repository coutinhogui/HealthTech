# Project Dump (2025-10-05 16:32:20)

<details><summary><strong>📁 Índice</strong></summary>

- [1. .\Directory.Build.props](#001---Directory-Build-props.ToLower())
- [2. .\dumpScript.ps1](#002---dumpScript-ps1.ToLower())
- [3. .\healthtech_bootstrap.ps1](#003---healthtech_bootstrap-ps1.ToLower())
- [4. .\HealthTech.sln](#004---HealthTech-sln.ToLower())
- [5. .\src\apphost\appsettings.json](#005---src-apphost-appsettings-json.ToLower())
- [6. .\src\apphost\HealthTech.AppHost.csproj](#006---src-apphost-HealthTech-AppHost-csproj.ToLower())
- [7. .\src\apphost\Program.cs](#007---src-apphost-Program-cs.ToLower())
- [8. .\src\apphost\Properties\launchSettings.json](#008---src-apphost-Properties-launchSettings-json.ToLower())
- [9. .\src\building-blocks\Abstractions\Class1.cs](#009---src-building-blocks-Abstractions-Class1-cs.ToLower())
- [10. .\src\building-blocks\Abstractions\HealthTech.BuildingBlocks.Abstractions.csproj](#010---src-building-blocks-Abstractions-HealthTech-BuildingBlocks-Abstractions-csproj.ToLower())
- [11. .\src\building-blocks\SharedKernel\Class1.cs](#011---src-building-blocks-SharedKernel-Class1-cs.ToLower())
- [12. .\src\building-blocks\SharedKernel\HealthTech.BuildingBlocks.SharedKernel.csproj](#012---src-building-blocks-SharedKernel-HealthTech-BuildingBlocks-SharedKernel-csproj.ToLower())
- [13. .\src\frontends\app-shell\_Imports.razor](#013---src-frontends-app-shell-_Imports-razor.ToLower())
- [14. .\src\frontends\app-shell\App.razor](#014---src-frontends-app-shell-App-razor.ToLower())
- [15. .\src\frontends\app-shell\appsettings.json](#015---src-frontends-app-shell-appsettings-json.ToLower())
- [16. .\src\frontends\app-shell\HealthTech.AppShell.csproj](#016---src-frontends-app-shell-HealthTech-AppShell-csproj.ToLower())
- [17. .\src\frontends\app-shell\Layout\MainLayout.razor](#017---src-frontends-app-shell-Layout-MainLayout-razor.ToLower())
- [18. .\src\frontends\app-shell\Layout\NavMenu.razor](#018---src-frontends-app-shell-Layout-NavMenu-razor.ToLower())
- [19. .\src\frontends\app-shell\Pages\Counter.razor](#019---src-frontends-app-shell-Pages-Counter-razor.ToLower())
- [20. .\src\frontends\app-shell\Pages\Home.razor](#020---src-frontends-app-shell-Pages-Home-razor.ToLower())
- [21. .\src\frontends\app-shell\Pages\Login.razor](#021---src-frontends-app-shell-Pages-Login-razor.ToLower())
- [22. .\src\frontends\app-shell\Pages\Weather.razor](#022---src-frontends-app-shell-Pages-Weather-razor.ToLower())
- [23. .\src\frontends\app-shell\Program.cs](#023---src-frontends-app-shell-Program-cs.ToLower())
- [24. .\src\frontends\app-shell\Properties\launchSettings.json](#024---src-frontends-app-shell-Properties-launchSettings-json.ToLower())
- [25. .\src\frontends\app-shell\Services\BearerAuthorizationMessageHandler.cs](#025---src-frontends-app-shell-Services-BearerAuthorizationMessageHandler-cs.ToLower())
- [26. .\src\frontends\app-shell\Services\SupabaseAuthService.cs](#026---src-frontends-app-shell-Services-SupabaseAuthService-cs.ToLower())
- [27. .\src\frontends\app-shell\wwwroot\css\app.css](#027---src-frontends-app-shell-wwwroot-css-app-css.ToLower())
- [28. .\src\frontends\app-shell\wwwroot\sample-data\weather.json](#028---src-frontends-app-shell-wwwroot-sample-data-weather-json.ToLower())
- [29. .\src\frontends\mf-appointment\_Imports.razor](#029---src-frontends-mf-appointment-_Imports-razor.ToLower())
- [30. .\src\frontends\mf-appointment\Component1.razor](#030---src-frontends-mf-appointment-Component1-razor.ToLower())
- [31. .\src\frontends\mf-appointment\Component1.razor.css](#031---src-frontends-mf-appointment-Component1-razor-css.ToLower())
- [32. .\src\frontends\mf-appointment\ExampleJsInterop.cs](#032---src-frontends-mf-appointment-ExampleJsInterop-cs.ToLower())
- [33. .\src\frontends\mf-appointment\HealthTech.mf.appointment.csproj](#033---src-frontends-mf-appointment-HealthTech-mf-appointment-csproj.ToLower())
- [34. .\src\frontends\mf-appointment\wwwroot\exampleJsInterop.js](#034---src-frontends-mf-appointment-wwwroot-exampleJsInterop-js.ToLower())
- [35. .\src\frontends\mf-billing\_Imports.razor](#035---src-frontends-mf-billing-_Imports-razor.ToLower())
- [36. .\src\frontends\mf-billing\Component1.razor](#036---src-frontends-mf-billing-Component1-razor.ToLower())
- [37. .\src\frontends\mf-billing\Component1.razor.css](#037---src-frontends-mf-billing-Component1-razor-css.ToLower())
- [38. .\src\frontends\mf-billing\ExampleJsInterop.cs](#038---src-frontends-mf-billing-ExampleJsInterop-cs.ToLower())
- [39. .\src\frontends\mf-billing\HealthTech.mf.billing.csproj](#039---src-frontends-mf-billing-HealthTech-mf-billing-csproj.ToLower())
- [40. .\src\frontends\mf-billing\wwwroot\exampleJsInterop.js](#040---src-frontends-mf-billing-wwwroot-exampleJsInterop-js.ToLower())
- [41. .\src\frontends\mf-ehr\_Imports.razor](#041---src-frontends-mf-ehr-_Imports-razor.ToLower())
- [42. .\src\frontends\mf-ehr\Component1.razor](#042---src-frontends-mf-ehr-Component1-razor.ToLower())
- [43. .\src\frontends\mf-ehr\Component1.razor.css](#043---src-frontends-mf-ehr-Component1-razor-css.ToLower())
- [44. .\src\frontends\mf-ehr\ExampleJsInterop.cs](#044---src-frontends-mf-ehr-ExampleJsInterop-cs.ToLower())
- [45. .\src\frontends\mf-ehr\HealthTech.mf.ehr.csproj](#045---src-frontends-mf-ehr-HealthTech-mf-ehr-csproj.ToLower())
- [46. .\src\frontends\mf-ehr\wwwroot\exampleJsInterop.js](#046---src-frontends-mf-ehr-wwwroot-exampleJsInterop-js.ToLower())
- [47. .\src\frontends\mf-patient\_Imports.razor](#047---src-frontends-mf-patient-_Imports-razor.ToLower())
- [48. .\src\frontends\mf-patient\Component1.razor](#048---src-frontends-mf-patient-Component1-razor.ToLower())
- [49. .\src\frontends\mf-patient\Component1.razor.css](#049---src-frontends-mf-patient-Component1-razor-css.ToLower())
- [50. .\src\frontends\mf-patient\ExampleJsInterop.cs](#050---src-frontends-mf-patient-ExampleJsInterop-cs.ToLower())
- [51. .\src\frontends\mf-patient\HealthTech.mf.patient.csproj](#051---src-frontends-mf-patient-HealthTech-mf-patient-csproj.ToLower())
- [52. .\src\frontends\mf-patient\wwwroot\exampleJsInterop.js](#052---src-frontends-mf-patient-wwwroot-exampleJsInterop-js.ToLower())
- [53. .\src\frontends\ui-kit\_Imports.razor](#053---src-frontends-ui-kit-_Imports-razor.ToLower())
- [54. .\src\frontends\ui-kit\Component1.razor](#054---src-frontends-ui-kit-Component1-razor.ToLower())
- [55. .\src\frontends\ui-kit\Component1.razor.css](#055---src-frontends-ui-kit-Component1-razor-css.ToLower())
- [56. .\src\frontends\ui-kit\ExampleJsInterop.cs](#056---src-frontends-ui-kit-ExampleJsInterop-cs.ToLower())
- [57. .\src\frontends\ui-kit\HealthTech.ui.kit.csproj](#057---src-frontends-ui-kit-HealthTech-ui-kit-csproj.ToLower())
- [58. .\src\frontends\ui-kit\wwwroot\exampleJsInterop.js](#058---src-frontends-ui-kit-wwwroot-exampleJsInterop-js.ToLower())
- [59. .\src\gateway\appsettings.Development.json](#059---src-gateway-appsettings-Development-json.ToLower())
- [60. .\src\gateway\appsettings.json](#060---src-gateway-appsettings-json.ToLower())
- [61. .\src\gateway\HealthTech.Gateway.csproj](#061---src-gateway-HealthTech-Gateway-csproj.ToLower())
- [62. .\src\gateway\Program.cs](#062---src-gateway-Program-cs.ToLower())
- [63. .\src\gateway\Properties\launchSettings.json](#063---src-gateway-Properties-launchSettings-json.ToLower())
- [64. .\src\services\Appointments\Appointments.Api\appsettings.Development.json](#064---src-services-Appointments-Appointments-Api-appsettings-Development-json.ToLower())
- [65. .\src\services\Appointments\Appointments.Api\appsettings.json](#065---src-services-Appointments-Appointments-Api-appsettings-json.ToLower())
- [66. .\src\services\Appointments\Appointments.Api\HealthTech.Appointments.Api.csproj](#066---src-services-Appointments-Appointments-Api-HealthTech-Appointments-Api-csproj.ToLower())
- [67. .\src\services\Appointments\Appointments.Api\Program.cs](#067---src-services-Appointments-Appointments-Api-Program-cs.ToLower())
- [68. .\src\services\Appointments\Appointments.Api\Properties\launchSettings.json](#068---src-services-Appointments-Appointments-Api-Properties-launchSettings-json.ToLower())
- [69. .\src\services\Appointments\Appointments.Application\Class1.cs](#069---src-services-Appointments-Appointments-Application-Class1-cs.ToLower())
- [70. .\src\services\Appointments\Appointments.Application\HealthTech.Appointments.Application.csproj](#070---src-services-Appointments-Appointments-Application-HealthTech-Appointments-Application-csproj.ToLower())
- [71. .\src\services\Appointments\Appointments.Domain\Class1.cs](#071---src-services-Appointments-Appointments-Domain-Class1-cs.ToLower())
- [72. .\src\services\Appointments\Appointments.Domain\HealthTech.Appointments.Domain.csproj](#072---src-services-Appointments-Appointments-Domain-HealthTech-Appointments-Domain-csproj.ToLower())
- [73. .\src\services\Appointments\Appointments.Infrastructure\Class1.cs](#073---src-services-Appointments-Appointments-Infrastructure-Class1-cs.ToLower())
- [74. .\src\services\Appointments\Appointments.Infrastructure\HealthTech.Appointments.Infrastructure.csproj](#074---src-services-Appointments-Appointments-Infrastructure-HealthTech-Appointments-Infrastructure-csproj.ToLower())
- [75. .\src\services\Identity\Identity.Api\appsettings.Development.json](#075---src-services-Identity-Identity-Api-appsettings-Development-json.ToLower())
- [76. .\src\services\Identity\Identity.Api\appsettings.json](#076---src-services-Identity-Identity-Api-appsettings-json.ToLower())
- [77. .\src\services\Identity\Identity.Api\Controllers\WhoAmIController.cs](#077---src-services-Identity-Identity-Api-Controllers-WhoAmIController-cs.ToLower())
- [78. .\src\services\Identity\Identity.Api\HealthTech.Identity.Api.csproj](#078---src-services-Identity-Identity-Api-HealthTech-Identity-Api-csproj.ToLower())
- [79. .\src\services\Identity\Identity.Api\Program.cs](#079---src-services-Identity-Identity-Api-Program-cs.ToLower())
- [80. .\src\services\Identity\Identity.Api\Properties\launchSettings.json](#080---src-services-Identity-Identity-Api-Properties-launchSettings-json.ToLower())
- [81. .\src\services\Identity\Identity.Application\Class1.cs](#081---src-services-Identity-Identity-Application-Class1-cs.ToLower())
- [82. .\src\services\Identity\Identity.Application\HealthTech.Identity.Application.csproj](#082---src-services-Identity-Identity-Application-HealthTech-Identity-Application-csproj.ToLower())
- [83. .\src\services\Identity\Identity.Domain\Class1.cs](#083---src-services-Identity-Identity-Domain-Class1-cs.ToLower())
- [84. .\src\services\Identity\Identity.Domain\HealthTech.Identity.Domain.csproj](#084---src-services-Identity-Identity-Domain-HealthTech-Identity-Domain-csproj.ToLower())
- [85. .\src\services\Identity\Identity.Infrastructure\Class1.cs](#085---src-services-Identity-Identity-Infrastructure-Class1-cs.ToLower())
- [86. .\src\services\Identity\Identity.Infrastructure\HealthTech.Identity.Infrastructure.csproj](#086---src-services-Identity-Identity-Infrastructure-HealthTech-Identity-Infrastructure-csproj.ToLower())
- [87. .\src\services\Patients\Patients.Api\appsettings.Development.json](#087---src-services-Patients-Patients-Api-appsettings-Development-json.ToLower())
- [88. .\src\services\Patients\Patients.Api\appsettings.json](#088---src-services-Patients-Patients-Api-appsettings-json.ToLower())
- [89. .\src\services\Patients\Patients.Api\HealthTech.Patients.Api.csproj](#089---src-services-Patients-Patients-Api-HealthTech-Patients-Api-csproj.ToLower())
- [90. .\src\services\Patients\Patients.Api\Program.cs](#090---src-services-Patients-Patients-Api-Program-cs.ToLower())
- [91. .\src\services\Patients\Patients.Api\Properties\launchSettings.json](#091---src-services-Patients-Patients-Api-Properties-launchSettings-json.ToLower())
- [92. .\src\services\Patients\Patients.Application\Application.cs](#092---src-services-Patients-Patients-Application-Application-cs.ToLower())
- [93. .\src\services\Patients\Patients.Application\HealthTech.Patients.Application.csproj](#093---src-services-Patients-Patients-Application-HealthTech-Patients-Application-csproj.ToLower())
- [94. .\src\services\Patients\Patients.Domain\HealthTech.Patients.Domain.csproj](#094---src-services-Patients-Patients-Domain-HealthTech-Patients-Domain-csproj.ToLower())
- [95. .\src\services\Patients\Patients.Domain\Patient.cs](#095---src-services-Patients-Patients-Domain-Patient-cs.ToLower())
- [96. .\src\services\Patients\Patients.Infrastructure\HealthTech.Patients.Infrastructure.csproj](#096---src-services-Patients-Patients-Infrastructure-HealthTech-Patients-Infrastructure-csproj.ToLower())
- [97. .\src\services\Patients\Patients.Infrastructure\Infrastructure.cs](#097---src-services-Patients-Patients-Infrastructure-Infrastructure-cs.ToLower())
</details>

## 1. .\Directory.Build.props
<a id="001---Directory-Build-props.ToLower()"></a>

```xml
<Project>
  <PropertyGroup>
    <Company>Acme</Company>
    <Authors>Acme</Authors>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>preview</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
    <Deterministic>true</Deterministic>
  </PropertyGroup>
</Project>

```

## 2. .\dumpScript.ps1
<a id="002---dumpScript-ps1.ToLower()"></a>

```powershell
param(
  [string]$Root = ".",
  [string]$OutMd = "project-dump.md",
  [string]$OutZip = "project-dump.zip",
  [int]$MaxBytesPerFile = 200000,   # ~200 KB por arquivo no MD
  [int]$MaxFiles = 500,             # limite de arquivos listados
  [switch]$NoZip                    # opcional: não gerar zip
)

$ErrorActionPreference = "Stop"

Write-Host "[i] Root: $Root"
Set-Location $Root

# 1) Padrões de inclusão e exclusão
$include = @("*.sln","*.csproj","*.props","*.targets","*.cs","*.razor","*.razor.css","*.json","*.yml","*.yaml","*.config","*.ps1","*.ts","*.tsx","*.js","*.css")
$excludeDirs = @("\.git", "\.vs", "bin", "obj", "node_modules", "dist", "artifacts", "coverage")
$excludeFiles = @("appsettings.*.json.local","secrets.json")

function Should-ExcludeDir([string]$path) {
  foreach($d in $excludeDirs){
    if($path -match [regex]::Escape([IO.Path]::DirectorySeparatorChar) + $d + "($|[\\/])"){ return $true }
  }
  return $false
}

Write-Host "[i] Coletando arquivos..."
$files = Get-ChildItem -Path . -Recurse -File -Include $include |
  Where-Object { -not (Should-ExcludeDir $_.DirectoryName) } |
  Where-Object { $excludeFiles -notcontains $_.Name } |
  Sort-Object FullName

if($files.Count -eq 0){ Write-Error "Nenhum arquivo relevante encontrado."; exit 1 }

if($files.Count -gt $MaxFiles){
  Write-Warning "Muitos arquivos ($($files.Count)). Limitando aos primeiros $MaxFiles."
  $files = $files | Select-Object -First $MaxFiles
}

Write-Host "[i] Escrevendo $OutMd..."
"# Project Dump ($(Get-Date -Format 'yyyy-MM-dd HH:mm:ss'))" | Out-File $OutMd -Encoding UTF8
"" | Out-File $OutMd -Append -Encoding UTF8

# ToC colapsável
"<details><summary><strong>📁 Índice</strong></summary>" | Out-File $OutMd -Append -Encoding UTF8
"" | Out-File $OutMd -Append -Encoding UTF8

$idx = 1
foreach($f in $files){
  $rel = Resolve-Path -Relative $f.FullName
  $anchor = "$($idx.ToString().PadLeft(3,'0'))-$(($rel -replace '[^a-zA-Z0-9\-_.\\/ ]','' ) -replace '[\\/. ]','-').ToLower()"
  "- [$idx. $rel](#$anchor)" | Out-File $OutMd -Append -Encoding UTF8
  $idx++
}

"</details>" | Out-File $OutMd -Append -Encoding UTF8
"" | Out-File $OutMd -Append -Encoding UTF8

# Conteúdo dos arquivos
$idx = 1
foreach($f in $files){
  $rel = Resolve-Path -Relative $f.FullName
  $anchor = "$($idx.ToString().PadLeft(3,'0'))-$(($rel -replace '[^a-zA-Z0-9\-_.\\/ ]','' ) -replace '[\\/. ]','-').ToLower()"

  "## $idx. $rel" | Out-File $OutMd -Append -Encoding UTF8
  "<a id=""$anchor""></a>" | Out-File $OutMd -Append -Encoding UTF8
  ""                       | Out-File $OutMd -Append -Encoding UTF8

  $lang = switch($f.Extension.ToLower()){
    ".cs"     { "csharp" }
    ".razor"  { "razor" }
    ".json"   { "json" }
    ".yml"    { "yaml" }
    ".yaml"   { "yaml" }
    ".ps1"    { "powershell" }
    ".ts"     { "typescript" }
    ".tsx"    { "tsx" }
    ".js"     { "javascript" }
    ".css"    { "css" }
    ".csproj" { "xml" }
    ".props"  { "xml" }
    ".targets"{ "xml" }
    default   { "" }
  }

  $content = Get-Content -Raw -Encoding UTF8 $f.FullName
  $bytes = [System.Text.Encoding]::UTF8.GetByteCount($content)

  $truncated = $false
  if($bytes -gt $MaxBytesPerFile){
    $truncated = $true
    $lines = Get-Content -Encoding UTF8 $f.FullName
    $acc = New-Object System.Collections.Generic.List[string]
    $total = 0
    foreach($ln in $lines){
      $b = [System.Text.Encoding]::UTF8.GetByteCount($ln + "`n")
      if(($total + $b) -gt $MaxBytesPerFile){ break }
      $acc.Add($ln)
      $total += $b
    }
    $content = ($acc -join "`n")
  }

  '```' + $lang | Out-File $OutMd -Append -Encoding UTF8
  $content      | Out-File $OutMd -Append -Encoding UTF8
  '```'         | Out-File $OutMd -Append -Encoding UTF8

  if ($truncated) {
    "" | Out-File $OutMd -Append -Encoding UTF8
    $kb = [math]::Round($MaxBytesPerFile / 1024)
    "[truncate] Exibindo apenas ~${kb} KB deste arquivo. Conteúdo completo está no ZIP." | Out-File $OutMd -Append -Encoding UTF8
  }

  "" | Out-File $OutMd -Append -Encoding UTF8
  $idx++
}

# 6) Gerar ZIP se desejado e caminho válido
if (-not $NoZip -and $OutZip -and $OutZip.Trim().Length -gt 0) {
  Write-Host ""
  Write-Host ('[i] Gerando ZIP: {0}' -f $OutZip)

  if (Test-Path $OutZip) { Remove-Item $OutZip -Force }

  $tmpFolder = Join-Path $env:TEMP $("dump_" + [guid]::NewGuid().ToString())
  New-Item -ItemType Directory -Path $tmpFolder | Out-Null

  foreach ($f in $files) {
    $rel = Resolve-Path -Relative $f.FullName
    $dest = Join-Path $tmpFolder $rel
    $destDir = Split-Path -Parent $dest
    if (-not (Test-Path $destDir)) { New-Item -ItemType Directory -Path $destDir | Out-Null }
    Copy-Item $f.FullName $dest
  }

  Compress-Archive -Path (Join-Path $tmpFolder '*') -DestinationPath $OutZip
  Remove-Item $tmpFolder -Recurse -Force
}

# Final
Write-Host "`n[OK] Dump gerado:"
Write-Host (" - Arquivos incluídos: {0}" -f $files.Count)
Write-Host (" - Markdown: {0}" -f (Resolve-Path $OutMd).Path)
if (-not $NoZip -and $OutZip) {
  Write-Host (" - ZIP:      {0}" -f (Resolve-Path $OutZip).Path)
}

```

## 3. .\healthtech_bootstrap.ps1
<a id="003---healthtech_bootstrap-ps1.ToLower()"></a>

```powershell
param(
  [string]$SolutionName = "HealthTech",
  [string]$Company = "Acme",
  [string]$Domain = "healthtech.local",
  [string]$DotnetVersion = "9.0"
)

$ErrorActionPreference = "Stop"

function Ensure-Dir($path) {
  if (!(Test-Path $path)) { New-Item -ItemType Directory -Path $path | Out-Null }
}

function Write-File($path, [string]$content) {
  $dir = Split-Path $path -Parent
  Ensure-Dir $dir
  Set-Content -Path $path -Value $content -Encoding UTF8
}

function Append-File($path, [string]$content) {
  Add-Content -Path $path -Value $content -Encoding UTF8
}

function Dotnet-New($template, $output, $name) {
  Ensure-Dir $output
  dotnet new $template -n $name -o $output | Out-Null
}

# 1) Raiz e solução
$root = (Resolve-Path ".").Path
$src = Join-Path $root "src"
$services = Join-Path $src "services"
$building = Join-Path $src "building-blocks"
$fronts = Join-Path $src "frontends"
$apphost = Join-Path $src "apphost"
$gateway = Join-Path $src "gateway"

Ensure-Dir $src
Ensure-Dir $services
Ensure-Dir $building
Ensure-Dir $fronts
Ensure-Dir $apphost
Ensure-Dir $gateway

if (!(Test-Path (Join-Path $root "$SolutionName.sln"))) {
  dotnet new sln -n $SolutionName | Out-Null
}

# 2) Directory.Build.props
$props = @"
<Project>
  <PropertyGroup>
    <Company>$Company</Company>
    <Authors>$Company</Authors>
    <TargetFramework>net$DotnetVersion</TargetFramework>
    <LangVersion>preview</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
    <Deterministic>true</Deterministic>
  </PropertyGroup>
</Project>
"@
Write-File (Join-Path $root "Directory.Build.props") $props

# 3) Building Blocks
$sharedKernel = Join-Path $building "SharedKernel"
$abstractions = Join-Path $building "Abstractions"
Dotnet-New classlib $sharedKernel "$SolutionName.BuildingBlocks.SharedKernel"
Dotnet-New classlib $abstractions "$SolutionName.BuildingBlocks.Abstractions"

dotnet sln add "$sharedKernel/$SolutionName.BuildingBlocks.SharedKernel.csproj" | Out-Null

dotnet sln add "$abstractions/$SolutionName.BuildingBlocks.Abstractions.csproj" | Out-Null

# 4) Pacotes comuns
function Add-Common-Packages($proj) {
  dotnet add $proj package MediatR | Out-Null
  dotnet add $proj package FluentValidation | Out-Null
}

# 5) Serviços (exemplo: Patients, Appointments, Identity)
$svcNames = @("Patients","Appointments","Identity")
foreach ($svc in $svcNames) {
  $svcRoot = Join-Path $services $svc
  $projApi = Join-Path $svcRoot "$svc.Api"
  $projApp = Join-Path $svcRoot "$svc.Application"
  $projDom = Join-Path $svcRoot "$svc.Domain"
  $projInfra = Join-Path $svcRoot "$svc.Infrastructure"

  Dotnet-New webapi $projApi "$SolutionName.$svc.Api"
  Dotnet-New classlib $projApp "$SolutionName.$svc.Application"
  Dotnet-New classlib $projDom "$SolutionName.$svc.Domain"
  Dotnet-New classlib $projInfra "$SolutionName.$svc.Infrastructure"

  # Limpar WeatherForecast padrão, se existir
  Remove-Item -Force -Recurse "$projApi/WeatherForecast.cs" -ErrorAction SilentlyContinue
  Remove-Item -Force -Recurse "$projApi/Controllers" -ErrorAction SilentlyContinue

  # Referências (PowerShell usa crase ` para quebra de linha)
  dotnet add "$projApi/$SolutionName.$svc.Api.csproj" reference `
    "$projApp/$SolutionName.$svc.Application.csproj" `
    "$projInfra/$SolutionName.$svc.Infrastructure.csproj" `
    "$projDom/$SolutionName.$svc.Domain.csproj" | Out-Null

  dotnet add "$projApp/$SolutionName.$svc.Application.csproj" reference `
    "$sharedKernel/$SolutionName.BuildingBlocks.SharedKernel.csproj" `
    "$abstractions/$SolutionName.BuildingBlocks.Abstractions.csproj" `
    "$projDom/$SolutionName.$svc.Domain.csproj" | Out-Null

  dotnet add "$projInfra/$SolutionName.$svc.Infrastructure.csproj" reference `
    "$projDom/$SolutionName.$svc.Domain.csproj" `
    "$abstractions/$SolutionName.BuildingBlocks.Abstractions.csproj" | Out-Null

  dotnet add "$projDom/$SolutionName.$svc.Domain.csproj" reference `
    "$sharedKernel/$SolutionName.BuildingBlocks.SharedKernel.csproj" | Out-Null

  # Pacotes
  Add-Common-Packages "$projApp/$SolutionName.$svc.Application.csproj"

  dotnet add "$projInfra/$SolutionName.$svc.Infrastructure.csproj" package Dapper | Out-Null
  dotnet add "$projInfra/$SolutionName.$svc.Infrastructure.csproj" package Npgsql | Out-Null
  dotnet add "$projInfra/$SolutionName.$svc.Infrastructure.csproj" package Microsoft.Extensions.Configuration.Abstractions | Out-Null
  dotnet add "$projInfra/$SolutionName.$svc.Infrastructure.csproj" package Microsoft.Extensions.Options.ConfigurationExtensions | Out-Null

  dotnet add "$projApi/$SolutionName.$svc.Api.csproj" package MediatR | Out-Null
  dotnet add "$projApi/$SolutionName.$svc.Api.csproj" package FluentValidation | Out-Null
  dotnet add "$projApi/$SolutionName.$svc.Api.csproj" package Swashbuckle.AspNetCore | Out-Null
  dotnet add "$projApi/$SolutionName.$svc.Api.csproj" package OpenTelemetry.Exporter.Console | Out-Null
  dotnet add "$projApi/$SolutionName.$svc.Api.csproj" package OpenTelemetry.Exporter.Otlp | Out-Null
  dotnet add "$projApi/$SolutionName.$svc.Api.csproj" package OpenTelemetry.Extensions.Hosting | Out-Null
  dotnet add "$projApi/$SolutionName.$svc.Api.csproj" package Microsoft.AspNetCore.Authentication.JwtBearer | Out-Null

  # Adicionar à solução
  dotnet sln add `
    "$projApi/$SolutionName.$svc.Api.csproj" `
    "$projApp/$SolutionName.$svc.Application.csproj" `
    "$projDom/$SolutionName.$svc.Domain.csproj" `
    "$projInfra/$SolutionName.$svc.Infrastructure.csproj" | Out-Null
}

# 6) Gateway YARP
Dotnet-New webapi $gateway "$SolutionName.Gateway"
dotnet add "$gateway/$SolutionName.Gateway.csproj" package Yarp.ReverseProxy | Out-Null

dotnet sln add "$gateway/$SolutionName.Gateway.csproj" | Out-Null

# 7) Aspire AppHost (console com bootstrap)
Dotnet-New console $apphost "$SolutionName.AppHost"
dotnet sln add "$apphost/$SolutionName.AppHost.csproj" | Out-Null

# 8) Frontends (Blazor WASM + MudBlazor + microfronts como RCL)
$appShell = Join-Path $fronts "app-shell"
Dotnet-New blazorwasm $appShell "$SolutionName.AppShell"
dotnet add "$appShell/$SolutionName.AppShell.csproj" package MudBlazor | Out-Null

dotnet sln add "$appShell/$SolutionName.AppShell.csproj" | Out-Null

$microfronts = @("mf-patient","mf-appointment","mf-ehr","mf-billing","ui-kit")
foreach ($mf in $microfronts) {
  $mfPath = Join-Path $fronts $mf
  Dotnet-New razorclasslib $mfPath "$SolutionName.$($mf -replace '-', '.')"
  dotnet sln add "$mfPath/$SolutionName.$($mf -replace '-', '.').csproj" | Out-Null
  dotnet add "$appShell/$SolutionName.AppShell.csproj" reference "$mfPath/$SolutionName.$($mf -replace '-', '.').csproj" | Out-Null
}

# 9) Arquivos base do SharedKernel/Abstractions
$sharedKernelCode = @"
namespace $Company.$SolutionName.BuildingBlocks.SharedKernel;

public abstract class Entity<TId>
{
    public TId Id { get; protected set; }
    protected Entity(TId id) => Id = id;
}

public readonly record struct Result<T>(bool Success, T? Value, string? Error)
{
    public static Result<T> Ok(T value) => new(true, value, null);
    public static Result<T> Fail(string error) => new(false, default, error);
}
"@
Write-File (Join-Path $sharedKernel "Class1.cs") $sharedKernelCode

$abstractionsCode = @"
namespace $Company.$SolutionName.BuildingBlocks.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IDbConnectionFactory
{
    System.Data.IDbConnection Create();
}
"@
Write-File (Join-Path $abstractions "Class1.cs") $abstractionsCode

# 10) Implementação exemplo: Patients (Domain, Application, Infrastructure, Api)
$patientsRoot = Join-Path $services "Patients"
$patientsDomain = Join-Path $patientsRoot "$SolutionName.Patients.Domain"
$patientsApp = Join-Path $patientsRoot "$SolutionName.Patients.Application"
$patientsInfra = Join-Path $patientsRoot "$SolutionName.Patients.Infrastructure"
$patientsApi = Join-Path $patientsRoot "$SolutionName.Patients.Api"

$patientDomainCode = @"
namespace $Company.$SolutionName.Patients.Domain;
using $Company.$SolutionName.BuildingBlocks.SharedKernel;

public readonly record struct PatientId(Guid Value)
{
    public static PatientId New() => new(Guid.NewGuid());
}

public sealed class Patient : Entity<PatientId>
{
    public string FullName { get; private set; }
    public string Document { get; private set; }
    public DateOnly BirthDate { get; private set; }

    private Patient() : base(new PatientId(Guid.Empty)) { FullName = Document = string.Empty; BirthDate = default; }

    public Patient(PatientId id, string fullName, string document, DateOnly birthDate) : base(id)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("FullName obrigatório");
        if (string.IsNullOrWhiteSpace(document)) throw new ArgumentException("Document obrigatório");
        FullName = fullName; Document = document; BirthDate = birthdate;
    }
}
"@
Write-File (Join-Path $patientsDomain "Patient.cs") $patientDomainCode

$patientsAppCode = @"
namespace $Company.$SolutionName.Patients.Application;
using MediatR;
using $Company.$SolutionName.BuildingBlocks.SharedKernel;
using $Company.$SolutionName.Patients.Domain;

public record RegisterPatientCommand(string FullName, string Document, DateOnly BirthDate) : IRequest<Result<Guid>>;

public interface IPatientRepository
{
    Task AddAsync(Patient entity, CancellationToken ct);
    Task<Patient?> GetAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<Patient>> ListAsync(int skip, int take, CancellationToken ct);
}

public sealed class RegisterPatientHandler(IPatientRepository repo, $Company.$SolutionName.BuildingBlocks.Abstractions.IUnitOfWork uow) : IRequestHandler<RegisterPatientCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterPatientCommand request, CancellationToken ct)
    {
        var entity = new Patient(PatientId.New(), request.FullName, request.Document, request.BirthDate);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return Result<Guid>.Ok(entity.Id.Value);
    }
}
"@
Write-File (Join-Path $patientsApp "Application.cs") $patientsAppCode

$patientsInfraCode = @"
namespace $Company.$SolutionName.Patients.Infrastructure;
using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using $Company.$SolutionName.BuildingBlocks.Abstractions;
using $Company.$SolutionName.Patients.Application;
using $Company.$SolutionName.Patients.Domain;

public sealed class NpgsqlConnectionFactory(IConfiguration cfg) : IDbConnectionFactory
{
    public IDbConnection Create()
    {
        var cs = cfg.GetConnectionString("PatientsDb") ?? throw new InvalidOperationException("Missing PatientsDb connection string");
        return new Npgsql.NpgsqlConnection(cs);
    }
}

public sealed class UnitOfWork(IDbConnectionFactory f) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(0); // Dapper: transacione por operação conforme necessidade.
}

public sealed class PatientRepository(IDbConnectionFactory f) : IPatientRepository
{
    public async Task AddAsync(Patient entity, CancellationToken ct)
    {
        const string sql = "INSERT INTO patients.patient(id, full_name, document, birth_date) VALUES (@Id, @FullName, @Document, @BirthDate);";
        using var con = f.Create();
        await con.ExecuteAsync(new CommandDefinition(sql, new { Id = entity.Id.Value, entity.FullName, entity.Document, BirthDate = entity.BirthDate }, cancellationToken: ct));
    }

    public async Task<Patient?> GetAsync(Guid id, CancellationToken ct)
    {
        const string sql = "SELECT id, full_name, document, birth_date FROM patients.patient WHERE id = @Id";
        using var con = f.Create();
        var row = await con.QuerySingleOrDefaultAsync(sql, new { Id = id });
        if (row is null) return null;
        return new Patient(new PatientId((Guid)row.id), (string)row.full_name, (string)row.document, DateOnly.FromDateTime((DateTime)row.birth_date));
    }

    public async Task<IEnumerable<Patient>> ListAsync(int skip, int take, CancellationToken ct)
    {
        const string sql = "SELECT id, full_name, document, birth_date FROM patients.patient ORDER BY full_name OFFSET @Skip LIMIT @Take";
        using var con = f.Create();
        var rows = await con.QueryAsync(sql, new { Skip = skip, Take = take });
        return rows.Select(r => new Patient(new PatientId((Guid)r.id), (string)r.full_name, (string)r.document, DateOnly.FromDateTime((DateTime)r.birth_date)));
    }
}

public static class PatientsInfraRegistration
{
    public static IServiceCollection AddPatientsInfra(this IServiceCollection services)
    {
        services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        return services;
    }
}
"@
Write-File (Join-Path $patientsInfra "Infrastructure.cs") $patientsInfraCode

$patientsApiProgram = @"
using MediatR;
using $Company.$SolutionName.Patients.Application;
using $Company.$SolutionName.Patients.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<RegisterPatientHandler>());

builder.Services.AddPatientsInfra();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.Authority = builder.Configuration["Auth:Authority"]; // Supabase JWKS/Issuer
        opt.TokenValidationParameters.ValidateAudience = false;
        opt.TokenValidationParameters.ValidIssuer = builder.Configuration["Auth:Issuer"]; 
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Patients API", Version = "v1" }));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/patients", async (RegisterPatientCommand cmd, ISender sender) =>
{
    var result = await sender.Send(cmd);
    return result.Success ? Results.Created($"/patients/{result.Value}", new { id = result.Value }) : Results.BadRequest(result.Error);
}).RequireAuthorization();

app.Run();
"@
Write-File (Join-Path $patientsApi "Program.cs") $patientsApiProgram

$appsettingsPatients = @"
{
  ""ConnectionStrings"": {
    ""PatientsDb"": ""Host=localhost;Port=5432;Database=patientsdb;Username=postgres;Password=postgres""
  },
  ""Auth"": {
    ""Authority"": ""https://YOUR-PROJECT.supabase.co/auth/v1"",
    ""Issuer"": ""https://YOUR-PROJECT.supabase.co/auth/v1""
  }
}
"@
Write-File (Join-Path $patientsApi "appsettings.json") $appsettingsPatients

# 11) Gateway Program.cs e config YARP (in-memory)
$gatewayProgram = @"
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy().LoadFromMemory(new[]
{
    new Yarp.ReverseProxy.Configuration.RouteConfig()
    {
        RouteId = "patients",
        ClusterId = "patients",
        Match = new(){ Path = "/patients/{**catch-all}" }
    }
}, new[]
{
    new Yarp.ReverseProxy.Configuration.ClusterConfig()
    {
        ClusterId = "patients",
        Destinations = new Dictionary<string, Yarp.ReverseProxy.Configuration.DestinationConfig>
        {
            ["d1"] = new() { Address = "http://localhost:5280" }
        }
    }
});

var app = builder.Build();
app.MapReverseProxy();
app.Run();
"@
Write-File (Join-Path $gateway "Program.cs") $gatewayProgram

# 12) AppHost (mensagem de bootstrap)
$appHostProgram = @"
Console.WriteLine("Starting $SolutionName AppHost (dev bootstrap)...");
Console.WriteLine("- Ensure Postgres and Redis are running.");
Console.WriteLine("- Start services and gateway with 'dotnet run' in each folder or attach your orchestrator of choice.");
"@
Write-File (Join-Path $apphost "Program.cs") $appHostProgram

# 13) Frontend AppShell bootstrap (MudBlazor + layout mínimo)
$appShellProgram = @"
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using $Company.$SolutionName.AppShell;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddMudServices();
await builder.Build().RunAsync();
"@
Write-File (Join-Path $appShell "Program.cs") $appShellProgram

$appShellAppRazor = @"
<App>
  <Router AppAssembly="@typeof(App).Assembly">
    <Found Context="routeData">
      <RouteView RouteData="routeData" />
    </Found>
    <NotFound>
      <p>Rota não encontrada.</p>
    </NotFound>
  </Router>
</App>
"@
Write-File (Join-Path $appShell "App.razor") $appShellAppRazor

# 14) Scripts SQL de bootstrap (schema patients)
$sqlDir = Join-Path $root "database"
Ensure-Dir $sqlDir
$patientsSql = @"
CREATE SCHEMA IF NOT EXISTS patients;
CREATE TABLE IF NOT EXISTS patients.patient (
  id uuid PRIMARY KEY,
  full_name varchar(160) NOT NULL,
  document varchar(32) NOT NULL,
  birth_date date NOT NULL
);
CREATE INDEX IF NOT EXISTS ix_patient_full_name ON patients.patient(full_name);
"@
Write-File (Join-Path $sqlDir "01_patients.sql") $patientsSql

# 15) README
$readme = @"
# $SolutionName (Blueprint Clean Architecture + Dapper + .NET 9 + Blazor/MudBlazor + YARP + Supabase)

## Como usar
1. Execute o script: `pwsh ./healthtech-bootstrap.ps1 -SolutionName $SolutionName`
2. Suba o Postgres e rode os scripts em `database/`.
3. Ajuste `appsettings.json` com as strings do Supabase e Auth JWKS/Issuer.
4. Rode as APIs e o Gateway:
   - `dotnet run --project src/services/Patients/$SolutionName.Patients.Api`
   - `dotnet run --project src/gateway/$SolutionName.Gateway`
5. Rode o AppShell (Blazor WASM):
   - `dotnet run --project src/frontends/app-shell/$SolutionName.AppShell`

## Próximos passos
- Criar endpoints adicionais, testes e observabilidade.
- Adicionar Redis, OTEL Collector e orquestração real com .NET Aspire.
- Incluir microfronts com rotas.
"@
Write-File (Join-Path $root "README.md") $readme

Write-Host "`n✅ Blueprint criado com sucesso! Abra a solução '$SolutionName.sln' e siga o README." -ForegroundColor Green

```

## 4. .\HealthTech.sln
<a id="004---HealthTech-sln.ToLower()"></a>

```

Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "src", "src", "{827E0CD3-B72D-47B6-A68D-7590B98EB39B}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "building-blocks", "building-blocks", "{C13E73B6-616D-3195-CD22-1E55A7D1F969}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "SharedKernel", "SharedKernel", "{0691B57A-016E-CC34-00A2-3232A94D6774}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "HealthTech.BuildingBlocks.SharedKernel", "src\building-blocks\SharedKernel\HealthTech.BuildingBlocks.SharedKernel.csproj", "{0AE35F2E-82D8-48C7-9BCC-930B40B7EE2D}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Abstractions", "Abstractions", "{17398E91-D91F-340C-59F4-1EE173D477A0}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "HealthTech.BuildingBlocks.Abstractions", "src\building-blocks\Abstractions\HealthTech.BuildingBlocks.Abstractions.csproj", "{BC988A2B-F816-4F6A-B4C9-CDF6BDDC3717}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "services", "services", "{984BB9B3-3FA3-BE33-9484-CAC21695A33C}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Patients", "Patients", "{0F7B896F-8F20-A128-DE69-B3ECEFF261A9}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Patients.Api", "Patients.Api", "{1F3947EB-E5A7-D4BC-7D18-598BC1F3E5A7}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "HealthTech.Patients.Api", "src\services\Patients\Patients.Api\HealthTech.Patients.Api.csproj", "{9655DF01-16E3-44E5-AFB2-94555025C6E7}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "HealthTech.Patients.Application", "src\services\Patients\Patients.Application\HealthTech.Patients.Application.csproj", "{CEBE1C78-646E-4A69-B261-414EBAA7459F}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "HealthTech.Patients.Domain", "src\services\Patients\Patients.Domain\HealthTech.Patients.Domain.csproj", "{352D00C4-135E-4D67-822C-C050A763EDA7}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "HealthTech.Patients.Infrastructure", "src\services\Patients\Patients.Infrastructure\HealthTech.Patients.Infrastructure.csproj", "{874E7206-992C-489F-83F5-3A47D6F9643B}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Appointments", "Appointments", "{18CD5D4C-C90B-F91A-C84E-055041F9F9F9}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Appointments.Api", "Appointments.Api", "{0C74EC83-CADA-D1AA-32D8-8BB46D5A82F0}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "HealthTech.Appointments.Api", "src\services\Appointments\Appointments.Api\HealthTech.Appointments.Api.csproj", "{6EB7CCED-ADFF-4901-966D-69E234C78B29}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "HealthTech.Appointments.Application", "src\services\Appointments\Appointments.Application\HealthTech.Appointments.Application.csproj", "{A1F171D8-39C5-483D-AC3E-56787B184FFD}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "HealthTech.Appointments.Domain", "src\services\Appointments\Appointments.Domain\HealthTech.Appointments.Domain.csproj", "{07CEB35E-FC4E-47FC-A2FC-1385DD3E07F8}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "HealthTech.Appointments.Infrastructure", "src\services\Appointments\Appointments.Infrastructure\HealthTech.Appointments.Infrastructure.csproj", "{2CCBC6F5-6AC4-42EB-8AF1-7996AF8744D7}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Identity", "Identity", "{1BFC9479-12BF-35C3-06B8-42D89D95092B}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Identity.Api", "Identity.Api", "{08275E4F-9D48-4557-7502-064122A05153}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "HealthTech.Identity.Api", "src\services\Identity\Identity.Api\HealthTech.Identity.Api.csproj", "{F16924D6-9020-4289-9366-BA1DB28288F0}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "HealthTech.Identity.Application", "src\services\Identity\Identity.Application\HealthTech.Identity.Application.csproj", "{C085460D-4B6E-468B-9BE8-91E42074BF37}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "HealthTech.Identity.Domain", "src\services\Identity\Identity.Domain\HealthTech.Identity.Domain.csproj", "{C31E7F32-B32F-4494-9AF1-476AC7719CB4}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "HealthTech.Identity.Infrastructure", "src\services\Identity\Identity.Infrastructure\HealthTech.Identity.Infrastructure.csproj", "{42CF600E-AD15-417A-B687-58C8643F65DA}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "gateway", "gateway", "{6306A8FB-679E-111F-6585-8F70E0EE6013}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "HealthTech.Gateway", "src\gateway\HealthTech.Gateway.csproj", "{A2C5261E-29FF-4D08-A5BA-8EE8800767CA}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "apphost", "apphost", "{63882A7C-90E5-DEE3-63BB-E6ABDBC8A365}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "HealthTech.AppHost", "src\apphost\HealthTech.AppHost.csproj", "{A800594F-9A9C-4B78-9983-4130828E5508}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "frontends", "frontends", "{FD11E8FD-FA37-D09A-0E3B-962F97776DA7}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "app-shell", "app-shell", "{4D2E7B1B-2526-EBBC-F6B3-37229C795DD3}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "HealthTech.AppShell", "src\frontends\app-shell\HealthTech.AppShell.csproj", "{1D15B8CF-C2C5-41D9-B69C-BDE4B05BEBF2}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "mf-patient", "mf-patient", "{C13D1D1E-F56B-E13F-2CB3-41C225EB9028}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "HealthTech.mf.patient", "src\frontends\mf-patient\HealthTech.mf.patient.csproj", "{24A2D484-1155-4459-B861-5A312E37C22C}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "mf-appointment", "mf-appointment", "{EC13D640-B63E-5DF5-8423-78F5754BCB04}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "HealthTech.mf.appointment", "src\frontends\mf-appointment\HealthTech.mf.appointment.csproj", "{2AE4552D-A806-4312-83FF-48BCA0AC7D8B}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "mf-ehr", "mf-ehr", "{D0EA2945-A714-1FCD-8E4D-522B2AAF6602}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "HealthTech.mf.ehr", "src\frontends\mf-ehr\HealthTech.mf.ehr.csproj", "{3C1906DD-01DE-4C8A-AE7D-E61180A86680}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "mf-billing", "mf-billing", "{FA49C31E-91F7-FF19-6028-9B3D7133ABE9}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "HealthTech.mf.billing", "src\frontends\mf-billing\HealthTech.mf.billing.csproj", "{C1FCE134-ED4B-482A-BE54-37DECD357735}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "ui-kit", "ui-kit", "{83D8EDDF-4362-4EAC-9DFF-24DE79F1740B}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "HealthTech.ui.kit", "src\frontends\ui-kit\HealthTech.ui.kit.csproj", "{C1543FD9-44DE-4DE9-949C-5AC6527AC45A}"
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Debug|x64 = Debug|x64
		Debug|x86 = Debug|x86
		Release|Any CPU = Release|Any CPU
		Release|x64 = Release|x64
		Release|x86 = Release|x86
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{0AE35F2E-82D8-48C7-9BCC-930B40B7EE2D}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{0AE35F2E-82D8-48C7-9BCC-930B40B7EE2D}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{0AE35F2E-82D8-48C7-9BCC-930B40B7EE2D}.Debug|x64.ActiveCfg = Debug|Any CPU
		{0AE35F2E-82D8-48C7-9BCC-930B40B7EE2D}.Debug|x64.Build.0 = Debug|Any CPU
		{0AE35F2E-82D8-48C7-9BCC-930B40B7EE2D}.Debug|x86.ActiveCfg = Debug|Any CPU
		{0AE35F2E-82D8-48C7-9BCC-930B40B7EE2D}.Debug|x86.Build.0 = Debug|Any CPU
		{0AE35F2E-82D8-48C7-9BCC-930B40B7EE2D}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{0AE35F2E-82D8-48C7-9BCC-930B40B7EE2D}.Release|Any CPU.Build.0 = Release|Any CPU
		{0AE35F2E-82D8-48C7-9BCC-930B40B7EE2D}.Release|x64.ActiveCfg = Release|Any CPU
		{0AE35F2E-82D8-48C7-9BCC-930B40B7EE2D}.Release|x64.Build.0 = Release|Any CPU
		{0AE35F2E-82D8-48C7-9BCC-930B40B7EE2D}.Release|x86.ActiveCfg = Release|Any CPU
		{0AE35F2E-82D8-48C7-9BCC-930B40B7EE2D}.Release|x86.Build.0 = Release|Any CPU
		{BC988A2B-F816-4F6A-B4C9-CDF6BDDC3717}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{BC988A2B-F816-4F6A-B4C9-CDF6BDDC3717}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{BC988A2B-F816-4F6A-B4C9-CDF6BDDC3717}.Debug|x64.ActiveCfg = Debug|Any CPU
		{BC988A2B-F816-4F6A-B4C9-CDF6BDDC3717}.Debug|x64.Build.0 = Debug|Any CPU
		{BC988A2B-F816-4F6A-B4C9-CDF6BDDC3717}.Debug|x86.ActiveCfg = Debug|Any CPU
		{BC988A2B-F816-4F6A-B4C9-CDF6BDDC3717}.Debug|x86.Build.0 = Debug|Any CPU
		{BC988A2B-F816-4F6A-B4C9-CDF6BDDC3717}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{BC988A2B-F816-4F6A-B4C9-CDF6BDDC3717}.Release|Any CPU.Build.0 = Release|Any CPU
		{BC988A2B-F816-4F6A-B4C9-CDF6BDDC3717}.Release|x64.ActiveCfg = Release|Any CPU
		{BC988A2B-F816-4F6A-B4C9-CDF6BDDC3717}.Release|x64.Build.0 = Release|Any CPU
		{BC988A2B-F816-4F6A-B4C9-CDF6BDDC3717}.Release|x86.ActiveCfg = Release|Any CPU
		{BC988A2B-F816-4F6A-B4C9-CDF6BDDC3717}.Release|x86.Build.0 = Release|Any CPU
		{9655DF01-16E3-44E5-AFB2-94555025C6E7}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{9655DF01-16E3-44E5-AFB2-94555025C6E7}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{9655DF01-16E3-44E5-AFB2-94555025C6E7}.Debug|x64.ActiveCfg = Debug|Any CPU
		{9655DF01-16E3-44E5-AFB2-94555025C6E7}.Debug|x64.Build.0 = Debug|Any CPU
		{9655DF01-16E3-44E5-AFB2-94555025C6E7}.Debug|x86.ActiveCfg = Debug|Any CPU
		{9655DF01-16E3-44E5-AFB2-94555025C6E7}.Debug|x86.Build.0 = Debug|Any CPU
		{9655DF01-16E3-44E5-AFB2-94555025C6E7}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{9655DF01-16E3-44E5-AFB2-94555025C6E7}.Release|Any CPU.Build.0 = Release|Any CPU
		{9655DF01-16E3-44E5-AFB2-94555025C6E7}.Release|x64.ActiveCfg = Release|Any CPU
		{9655DF01-16E3-44E5-AFB2-94555025C6E7}.Release|x64.Build.0 = Release|Any CPU
		{9655DF01-16E3-44E5-AFB2-94555025C6E7}.Release|x86.ActiveCfg = Release|Any CPU
		{9655DF01-16E3-44E5-AFB2-94555025C6E7}.Release|x86.Build.0 = Release|Any CPU
		{CEBE1C78-646E-4A69-B261-414EBAA7459F}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{CEBE1C78-646E-4A69-B261-414EBAA7459F}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{CEBE1C78-646E-4A69-B261-414EBAA7459F}.Debug|x64.ActiveCfg = Debug|Any CPU
		{CEBE1C78-646E-4A69-B261-414EBAA7459F}.Debug|x64.Build.0 = Debug|Any CPU
		{CEBE1C78-646E-4A69-B261-414EBAA7459F}.Debug|x86.ActiveCfg = Debug|Any CPU
		{CEBE1C78-646E-4A69-B261-414EBAA7459F}.Debug|x86.Build.0 = Debug|Any CPU
		{CEBE1C78-646E-4A69-B261-414EBAA7459F}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{CEBE1C78-646E-4A69-B261-414EBAA7459F}.Release|Any CPU.Build.0 = Release|Any CPU
		{CEBE1C78-646E-4A69-B261-414EBAA7459F}.Release|x64.ActiveCfg = Release|Any CPU
		{CEBE1C78-646E-4A69-B261-414EBAA7459F}.Release|x64.Build.0 = Release|Any CPU
		{CEBE1C78-646E-4A69-B261-414EBAA7459F}.Release|x86.ActiveCfg = Release|Any CPU
		{CEBE1C78-646E-4A69-B261-414EBAA7459F}.Release|x86.Build.0 = Release|Any CPU
		{352D00C4-135E-4D67-822C-C050A763EDA7}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{352D00C4-135E-4D67-822C-C050A763EDA7}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{352D00C4-135E-4D67-822C-C050A763EDA7}.Debug|x64.ActiveCfg = Debug|Any CPU
		{352D00C4-135E-4D67-822C-C050A763EDA7}.Debug|x64.Build.0 = Debug|Any CPU
		{352D00C4-135E-4D67-822C-C050A763EDA7}.Debug|x86.ActiveCfg = Debug|Any CPU
		{352D00C4-135E-4D67-822C-C050A763EDA7}.Debug|x86.Build.0 = Debug|Any CPU
		{352D00C4-135E-4D67-822C-C050A763EDA7}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{352D00C4-135E-4D67-822C-C050A763EDA7}.Release|Any CPU.Build.0 = Release|Any CPU
		{352D00C4-135E-4D67-822C-C050A763EDA7}.Release|x64.ActiveCfg = Release|Any CPU
		{352D00C4-135E-4D67-822C-C050A763EDA7}.Release|x64.Build.0 = Release|Any CPU
		{352D00C4-135E-4D67-822C-C050A763EDA7}.Release|x86.ActiveCfg = Release|Any CPU
		{352D00C4-135E-4D67-822C-C050A763EDA7}.Release|x86.Build.0 = Release|Any CPU
		{874E7206-992C-489F-83F5-3A47D6F9643B}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{874E7206-992C-489F-83F5-3A47D6F9643B}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{874E7206-992C-489F-83F5-3A47D6F9643B}.Debug|x64.ActiveCfg = Debug|Any CPU
		{874E7206-992C-489F-83F5-3A47D6F9643B}.Debug|x64.Build.0 = Debug|Any CPU
		{874E7206-992C-489F-83F5-3A47D6F9643B}.Debug|x86.ActiveCfg = Debug|Any CPU
		{874E7206-992C-489F-83F5-3A47D6F9643B}.Debug|x86.Build.0 = Debug|Any CPU
		{874E7206-992C-489F-83F5-3A47D6F9643B}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{874E7206-992C-489F-83F5-3A47D6F9643B}.Release|Any CPU.Build.0 = Release|Any CPU
		{874E7206-992C-489F-83F5-3A47D6F9643B}.Release|x64.ActiveCfg = Release|Any CPU
		{874E7206-992C-489F-83F5-3A47D6F9643B}.Release|x64.Build.0 = Release|Any CPU
		{874E7206-992C-489F-83F5-3A47D6F9643B}.Release|x86.ActiveCfg = Release|Any CPU
		{874E7206-992C-489F-83F5-3A47D6F9643B}.Release|x86.Build.0 = Release|Any CPU
		{6EB7CCED-ADFF-4901-966D-69E234C78B29}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{6EB7CCED-ADFF-4901-966D-69E234C78B29}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{6EB7CCED-ADFF-4901-966D-69E234C78B29}.Debug|x64.ActiveCfg = Debug|Any CPU
		{6EB7CCED-ADFF-4901-966D-69E234C78B29}.Debug|x64.Build.0 = Debug|Any CPU
		{6EB7CCED-ADFF-4901-966D-69E234C78B29}.Debug|x86.ActiveCfg = Debug|Any CPU
		{6EB7CCED-ADFF-4901-966D-69E234C78B29}.Debug|x86.Build.0 = Debug|Any CPU
		{6EB7CCED-ADFF-4901-966D-69E234C78B29}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{6EB7CCED-ADFF-4901-966D-69E234C78B29}.Release|Any CPU.Build.0 = Release|Any CPU
		{6EB7CCED-ADFF-4901-966D-69E234C78B29}.Release|x64.ActiveCfg = Release|Any CPU
		{6EB7CCED-ADFF-4901-966D-69E234C78B29}.Release|x64.Build.0 = Release|Any CPU
		{6EB7CCED-ADFF-4901-966D-69E234C78B29}.Release|x86.ActiveCfg = Release|Any CPU
		{6EB7CCED-ADFF-4901-966D-69E234C78B29}.Release|x86.Build.0 = Release|Any CPU
		{A1F171D8-39C5-483D-AC3E-56787B184FFD}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{A1F171D8-39C5-483D-AC3E-56787B184FFD}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{A1F171D8-39C5-483D-AC3E-56787B184FFD}.Debug|x64.ActiveCfg = Debug|Any CPU
		{A1F171D8-39C5-483D-AC3E-56787B184FFD}.Debug|x64.Build.0 = Debug|Any CPU
		{A1F171D8-39C5-483D-AC3E-56787B184FFD}.Debug|x86.ActiveCfg = Debug|Any CPU
		{A1F171D8-39C5-483D-AC3E-56787B184FFD}.Debug|x86.Build.0 = Debug|Any CPU
		{A1F171D8-39C5-483D-AC3E-56787B184FFD}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{A1F171D8-39C5-483D-AC3E-56787B184FFD}.Release|Any CPU.Build.0 = Release|Any CPU
		{A1F171D8-39C5-483D-AC3E-56787B184FFD}.Release|x64.ActiveCfg = Release|Any CPU
		{A1F171D8-39C5-483D-AC3E-56787B184FFD}.Release|x64.Build.0 = Release|Any CPU
		{A1F171D8-39C5-483D-AC3E-56787B184FFD}.Release|x86.ActiveCfg = Release|Any CPU
		{A1F171D8-39C5-483D-AC3E-56787B184FFD}.Release|x86.Build.0 = Release|Any CPU
		{07CEB35E-FC4E-47FC-A2FC-1385DD3E07F8}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{07CEB35E-FC4E-47FC-A2FC-1385DD3E07F8}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{07CEB35E-FC4E-47FC-A2FC-1385DD3E07F8}.Debug|x64.ActiveCfg = Debug|Any CPU
		{07CEB35E-FC4E-47FC-A2FC-1385DD3E07F8}.Debug|x64.Build.0 = Debug|Any CPU
		{07CEB35E-FC4E-47FC-A2FC-1385DD3E07F8}.Debug|x86.ActiveCfg = Debug|Any CPU
		{07CEB35E-FC4E-47FC-A2FC-1385DD3E07F8}.Debug|x86.Build.0 = Debug|Any CPU
		{07CEB35E-FC4E-47FC-A2FC-1385DD3E07F8}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{07CEB35E-FC4E-47FC-A2FC-1385DD3E07F8}.Release|Any CPU.Build.0 = Release|Any CPU
		{07CEB35E-FC4E-47FC-A2FC-1385DD3E07F8}.Release|x64.ActiveCfg = Release|Any CPU
		{07CEB35E-FC4E-47FC-A2FC-1385DD3E07F8}.Release|x64.Build.0 = Release|Any CPU
		{07CEB35E-FC4E-47FC-A2FC-1385DD3E07F8}.Release|x86.ActiveCfg = Release|Any CPU
		{07CEB35E-FC4E-47FC-A2FC-1385DD3E07F8}.Release|x86.Build.0 = Release|Any CPU
		{2CCBC6F5-6AC4-42EB-8AF1-7996AF8744D7}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{2CCBC6F5-6AC4-42EB-8AF1-7996AF8744D7}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{2CCBC6F5-6AC4-42EB-8AF1-7996AF8744D7}.Debug|x64.ActiveCfg = Debug|Any CPU
		{2CCBC6F5-6AC4-42EB-8AF1-7996AF8744D7}.Debug|x64.Build.0 = Debug|Any CPU
		{2CCBC6F5-6AC4-42EB-8AF1-7996AF8744D7}.Debug|x86.ActiveCfg = Debug|Any CPU
		{2CCBC6F5-6AC4-42EB-8AF1-7996AF8744D7}.Debug|x86.Build.0 = Debug|Any CPU
		{2CCBC6F5-6AC4-42EB-8AF1-7996AF8744D7}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{2CCBC6F5-6AC4-42EB-8AF1-7996AF8744D7}.Release|Any CPU.Build.0 = Release|Any CPU
		{2CCBC6F5-6AC4-42EB-8AF1-7996AF8744D7}.Release|x64.ActiveCfg = Release|Any CPU
		{2CCBC6F5-6AC4-42EB-8AF1-7996AF8744D7}.Release|x64.Build.0 = Release|Any CPU
		{2CCBC6F5-6AC4-42EB-8AF1-7996AF8744D7}.Release|x86.ActiveCfg = Release|Any CPU
		{2CCBC6F5-6AC4-42EB-8AF1-7996AF8744D7}.Release|x86.Build.0 = Release|Any CPU
		{F16924D6-9020-4289-9366-BA1DB28288F0}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{F16924D6-9020-4289-9366-BA1DB28288F0}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{F16924D6-9020-4289-9366-BA1DB28288F0}.Debug|x64.ActiveCfg = Debug|Any CPU
		{F16924D6-9020-4289-9366-BA1DB28288F0}.Debug|x64.Build.0 = Debug|Any CPU
		{F16924D6-9020-4289-9366-BA1DB28288F0}.Debug|x86.ActiveCfg = Debug|Any CPU
		{F16924D6-9020-4289-9366-BA1DB28288F0}.Debug|x86.Build.0 = Debug|Any CPU
		{F16924D6-9020-4289-9366-BA1DB28288F0}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{F16924D6-9020-4289-9366-BA1DB28288F0}.Release|Any CPU.Build.0 = Release|Any CPU
		{F16924D6-9020-4289-9366-BA1DB28288F0}.Release|x64.ActiveCfg = Release|Any CPU
		{F16924D6-9020-4289-9366-BA1DB28288F0}.Release|x64.Build.0 = Release|Any CPU
		{F16924D6-9020-4289-9366-BA1DB28288F0}.Release|x86.ActiveCfg = Release|Any CPU
		{F16924D6-9020-4289-9366-BA1DB28288F0}.Release|x86.Build.0 = Release|Any CPU
		{C085460D-4B6E-468B-9BE8-91E42074BF37}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{C085460D-4B6E-468B-9BE8-91E42074BF37}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{C085460D-4B6E-468B-9BE8-91E42074BF37}.Debug|x64.ActiveCfg = Debug|Any CPU
		{C085460D-4B6E-468B-9BE8-91E42074BF37}.Debug|x64.Build.0 = Debug|Any CPU
		{C085460D-4B6E-468B-9BE8-91E42074BF37}.Debug|x86.ActiveCfg = Debug|Any CPU
		{C085460D-4B6E-468B-9BE8-91E42074BF37}.Debug|x86.Build.0 = Debug|Any CPU
		{C085460D-4B6E-468B-9BE8-91E42074BF37}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{C085460D-4B6E-468B-9BE8-91E42074BF37}.Release|Any CPU.Build.0 = Release|Any CPU
		{C085460D-4B6E-468B-9BE8-91E42074BF37}.Release|x64.ActiveCfg = Release|Any CPU
		{C085460D-4B6E-468B-9BE8-91E42074BF37}.Release|x64.Build.0 = Release|Any CPU
		{C085460D-4B6E-468B-9BE8-91E42074BF37}.Release|x86.ActiveCfg = Release|Any CPU
		{C085460D-4B6E-468B-9BE8-91E42074BF37}.Release|x86.Build.0 = Release|Any CPU
		{C31E7F32-B32F-4494-9AF1-476AC7719CB4}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{C31E7F32-B32F-4494-9AF1-476AC7719CB4}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{C31E7F32-B32F-4494-9AF1-476AC7719CB4}.Debug|x64.ActiveCfg = Debug|Any CPU
		{C31E7F32-B32F-4494-9AF1-476AC7719CB4}.Debug|x64.Build.0 = Debug|Any CPU
		{C31E7F32-B32F-4494-9AF1-476AC7719CB4}.Debug|x86.ActiveCfg = Debug|Any CPU
		{C31E7F32-B32F-4494-9AF1-476AC7719CB4}.Debug|x86.Build.0 = Debug|Any CPU
		{C31E7F32-B32F-4494-9AF1-476AC7719CB4}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{C31E7F32-B32F-4494-9AF1-476AC7719CB4}.Release|Any CPU.Build.0 = Release|Any CPU
		{C31E7F32-B32F-4494-9AF1-476AC7719CB4}.Release|x64.ActiveCfg = Release|Any CPU
		{C31E7F32-B32F-4494-9AF1-476AC7719CB4}.Release|x64.Build.0 = Release|Any CPU
		{C31E7F32-B32F-4494-9AF1-476AC7719CB4}.Release|x86.ActiveCfg = Release|Any CPU
		{C31E7F32-B32F-4494-9AF1-476AC7719CB4}.Release|x86.Build.0 = Release|Any CPU
		{42CF600E-AD15-417A-B687-58C8643F65DA}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{42CF600E-AD15-417A-B687-58C8643F65DA}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{42CF600E-AD15-417A-B687-58C8643F65DA}.Debug|x64.ActiveCfg = Debug|Any CPU
		{42CF600E-AD15-417A-B687-58C8643F65DA}.Debug|x64.Build.0 = Debug|Any CPU
		{42CF600E-AD15-417A-B687-58C8643F65DA}.Debug|x86.ActiveCfg = Debug|Any CPU
		{42CF600E-AD15-417A-B687-58C8643F65DA}.Debug|x86.Build.0 = Debug|Any CPU
		{42CF600E-AD15-417A-B687-58C8643F65DA}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{42CF600E-AD15-417A-B687-58C8643F65DA}.Release|Any CPU.Build.0 = Release|Any CPU
		{42CF600E-AD15-417A-B687-58C8643F65DA}.Release|x64.ActiveCfg = Release|Any CPU
		{42CF600E-AD15-417A-B687-58C8643F65DA}.Release|x64.Build.0 = Release|Any CPU
		{42CF600E-AD15-417A-B687-58C8643F65DA}.Release|x86.ActiveCfg = Release|Any CPU
		{42CF600E-AD15-417A-B687-58C8643F65DA}.Release|x86.Build.0 = Release|Any CPU
		{A2C5261E-29FF-4D08-A5BA-8EE8800767CA}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{A2C5261E-29FF-4D08-A5BA-8EE8800767CA}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{A2C5261E-29FF-4D08-A5BA-8EE8800767CA}.Debug|x64.ActiveCfg = Debug|Any CPU
		{A2C5261E-29FF-4D08-A5BA-8EE8800767CA}.Debug|x64.Build.0 = Debug|Any CPU
		{A2C5261E-29FF-4D08-A5BA-8EE8800767CA}.Debug|x86.ActiveCfg = Debug|Any CPU
		{A2C5261E-29FF-4D08-A5BA-8EE8800767CA}.Debug|x86.Build.0 = Debug|Any CPU
		{A2C5261E-29FF-4D08-A5BA-8EE8800767CA}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{A2C5261E-29FF-4D08-A5BA-8EE8800767CA}.Release|Any CPU.Build.0 = Release|Any CPU
		{A2C5261E-29FF-4D08-A5BA-8EE8800767CA}.Release|x64.ActiveCfg = Release|Any CPU
		{A2C5261E-29FF-4D08-A5BA-8EE8800767CA}.Release|x64.Build.0 = Release|Any CPU
		{A2C5261E-29FF-4D08-A5BA-8EE8800767CA}.Release|x86.ActiveCfg = Release|Any CPU
		{A2C5261E-29FF-4D08-A5BA-8EE8800767CA}.Release|x86.Build.0 = Release|Any CPU
		{A800594F-9A9C-4B78-9983-4130828E5508}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{A800594F-9A9C-4B78-9983-4130828E5508}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{A800594F-9A9C-4B78-9983-4130828E5508}.Debug|x64.ActiveCfg = Debug|Any CPU
		{A800594F-9A9C-4B78-9983-4130828E5508}.Debug|x64.Build.0 = Debug|Any CPU
		{A800594F-9A9C-4B78-9983-4130828E5508}.Debug|x86.ActiveCfg = Debug|Any CPU
		{A800594F-9A9C-4B78-9983-4130828E5508}.Debug|x86.Build.0 = Debug|Any CPU
		{A800594F-9A9C-4B78-9983-4130828E5508}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{A800594F-9A9C-4B78-9983-4130828E5508}.Release|Any CPU.Build.0 = Release|Any CPU
		{A800594F-9A9C-4B78-9983-4130828E5508}.Release|x64.ActiveCfg = Release|Any CPU
		{A800594F-9A9C-4B78-9983-4130828E5508}.Release|x64.Build.0 = Release|Any CPU
		{A800594F-9A9C-4B78-9983-4130828E5508}.Release|x86.ActiveCfg = Release|Any CPU
		{A800594F-9A9C-4B78-9983-4130828E5508}.Release|x86.Build.0 = Release|Any CPU
		{1D15B8CF-C2C5-41D9-B69C-BDE4B05BEBF2}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{1D15B8CF-C2C5-41D9-B69C-BDE4B05BEBF2}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{1D15B8CF-C2C5-41D9-B69C-BDE4B05BEBF2}.Debug|x64.ActiveCfg = Debug|Any CPU
		{1D15B8CF-C2C5-41D9-B69C-BDE4B05BEBF2}.Debug|x64.Build.0 = Debug|Any CPU
		{1D15B8CF-C2C5-41D9-B69C-BDE4B05BEBF2}.Debug|x86.ActiveCfg = Debug|Any CPU
		{1D15B8CF-C2C5-41D9-B69C-BDE4B05BEBF2}.Debug|x86.Build.0 = Debug|Any CPU
		{1D15B8CF-C2C5-41D9-B69C-BDE4B05BEBF2}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{1D15B8CF-C2C5-41D9-B69C-BDE4B05BEBF2}.Release|Any CPU.Build.0 = Release|Any CPU
		{1D15B8CF-C2C5-41D9-B69C-BDE4B05BEBF2}.Release|x64.ActiveCfg = Release|Any CPU
		{1D15B8CF-C2C5-41D9-B69C-BDE4B05BEBF2}.Release|x64.Build.0 = Release|Any CPU
		{1D15B8CF-C2C5-41D9-B69C-BDE4B05BEBF2}.Release|x86.ActiveCfg = Release|Any CPU
		{1D15B8CF-C2C5-41D9-B69C-BDE4B05BEBF2}.Release|x86.Build.0 = Release|Any CPU
		{24A2D484-1155-4459-B861-5A312E37C22C}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{24A2D484-1155-4459-B861-5A312E37C22C}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{24A2D484-1155-4459-B861-5A312E37C22C}.Debug|x64.ActiveCfg = Debug|Any CPU
		{24A2D484-1155-4459-B861-5A312E37C22C}.Debug|x64.Build.0 = Debug|Any CPU
		{24A2D484-1155-4459-B861-5A312E37C22C}.Debug|x86.ActiveCfg = Debug|Any CPU
		{24A2D484-1155-4459-B861-5A312E37C22C}.Debug|x86.Build.0 = Debug|Any CPU
		{24A2D484-1155-4459-B861-5A312E37C22C}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{24A2D484-1155-4459-B861-5A312E37C22C}.Release|Any CPU.Build.0 = Release|Any CPU
		{24A2D484-1155-4459-B861-5A312E37C22C}.Release|x64.ActiveCfg = Release|Any CPU
		{24A2D484-1155-4459-B861-5A312E37C22C}.Release|x64.Build.0 = Release|Any CPU
		{24A2D484-1155-4459-B861-5A312E37C22C}.Release|x86.ActiveCfg = Release|Any CPU
		{24A2D484-1155-4459-B861-5A312E37C22C}.Release|x86.Build.0 = Release|Any CPU
		{2AE4552D-A806-4312-83FF-48BCA0AC7D8B}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{2AE4552D-A806-4312-83FF-48BCA0AC7D8B}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{2AE4552D-A806-4312-83FF-48BCA0AC7D8B}.Debug|x64.ActiveCfg = Debug|Any CPU
		{2AE4552D-A806-4312-83FF-48BCA0AC7D8B}.Debug|x64.Build.0 = Debug|Any CPU
		{2AE4552D-A806-4312-83FF-48BCA0AC7D8B}.Debug|x86.ActiveCfg = Debug|Any CPU
		{2AE4552D-A806-4312-83FF-48BCA0AC7D8B}.Debug|x86.Build.0 = Debug|Any CPU
		{2AE4552D-A806-4312-83FF-48BCA0AC7D8B}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{2AE4552D-A806-4312-83FF-48BCA0AC7D8B}.Release|Any CPU.Build.0 = Release|Any CPU
		{2AE4552D-A806-4312-83FF-48BCA0AC7D8B}.Release|x64.ActiveCfg = Release|Any CPU
		{2AE4552D-A806-4312-83FF-48BCA0AC7D8B}.Release|x64.Build.0 = Release|Any CPU
		{2AE4552D-A806-4312-83FF-48BCA0AC7D8B}.Release|x86.ActiveCfg = Release|Any CPU
		{2AE4552D-A806-4312-83FF-48BCA0AC7D8B}.Release|x86.Build.0 = Release|Any CPU
		{3C1906DD-01DE-4C8A-AE7D-E61180A86680}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{3C1906DD-01DE-4C8A-AE7D-E61180A86680}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{3C1906DD-01DE-4C8A-AE7D-E61180A86680}.Debug|x64.ActiveCfg = Debug|Any CPU
		{3C1906DD-01DE-4C8A-AE7D-E61180A86680}.Debug|x64.Build.0 = Debug|Any CPU
		{3C1906DD-01DE-4C8A-AE7D-E61180A86680}.Debug|x86.ActiveCfg = Debug|Any CPU
		{3C1906DD-01DE-4C8A-AE7D-E61180A86680}.Debug|x86.Build.0 = Debug|Any CPU
		{3C1906DD-01DE-4C8A-AE7D-E61180A86680}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{3C1906DD-01DE-4C8A-AE7D-E61180A86680}.Release|Any CPU.Build.0 = Release|Any CPU
		{3C1906DD-01DE-4C8A-AE7D-E61180A86680}.Release|x64.ActiveCfg = Release|Any CPU
		{3C1906DD-01DE-4C8A-AE7D-E61180A86680}.Release|x64.Build.0 = Release|Any CPU
		{3C1906DD-01DE-4C8A-AE7D-E61180A86680}.Release|x86.ActiveCfg = Release|Any CPU
		{3C1906DD-01DE-4C8A-AE7D-E61180A86680}.Release|x86.Build.0 = Release|Any CPU
		{C1FCE134-ED4B-482A-BE54-37DECD357735}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{C1FCE134-ED4B-482A-BE54-37DECD357735}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{C1FCE134-ED4B-482A-BE54-37DECD357735}.Debug|x64.ActiveCfg = Debug|Any CPU
		{C1FCE134-ED4B-482A-BE54-37DECD357735}.Debug|x64.Build.0 = Debug|Any CPU
		{C1FCE134-ED4B-482A-BE54-37DECD357735}.Debug|x86.ActiveCfg = Debug|Any CPU
		{C1FCE134-ED4B-482A-BE54-37DECD357735}.Debug|x86.Build.0 = Debug|Any CPU
		{C1FCE134-ED4B-482A-BE54-37DECD357735}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{C1FCE134-ED4B-482A-BE54-37DECD357735}.Release|Any CPU.Build.0 = Release|Any CPU
		{C1FCE134-ED4B-482A-BE54-37DECD357735}.Release|x64.ActiveCfg = Release|Any CPU
		{C1FCE134-ED4B-482A-BE54-37DECD357735}.Release|x64.Build.0 = Release|Any CPU
		{C1FCE134-ED4B-482A-BE54-37DECD357735}.Release|x86.ActiveCfg = Release|Any CPU
		{C1FCE134-ED4B-482A-BE54-37DECD357735}.Release|x86.Build.0 = Release|Any CPU
		{C1543FD9-44DE-4DE9-949C-5AC6527AC45A}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{C1543FD9-44DE-4DE9-949C-5AC6527AC45A}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{C1543FD9-44DE-4DE9-949C-5AC6527AC45A}.Debug|x64.ActiveCfg = Debug|Any CPU
		{C1543FD9-44DE-4DE9-949C-5AC6527AC45A}.Debug|x64.Build.0 = Debug|Any CPU
		{C1543FD9-44DE-4DE9-949C-5AC6527AC45A}.Debug|x86.ActiveCfg = Debug|Any CPU
		{C1543FD9-44DE-4DE9-949C-5AC6527AC45A}.Debug|x86.Build.0 = Debug|Any CPU
		{C1543FD9-44DE-4DE9-949C-5AC6527AC45A}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{C1543FD9-44DE-4DE9-949C-5AC6527AC45A}.Release|Any CPU.Build.0 = Release|Any CPU
		{C1543FD9-44DE-4DE9-949C-5AC6527AC45A}.Release|x64.ActiveCfg = Release|Any CPU
		{C1543FD9-44DE-4DE9-949C-5AC6527AC45A}.Release|x64.Build.0 = Release|Any CPU
		{C1543FD9-44DE-4DE9-949C-5AC6527AC45A}.Release|x86.ActiveCfg = Release|Any CPU
		{C1543FD9-44DE-4DE9-949C-5AC6527AC45A}.Release|x86.Build.0 = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
	GlobalSection(NestedProjects) = preSolution
		{C13E73B6-616D-3195-CD22-1E55A7D1F969} = {827E0CD3-B72D-47B6-A68D-7590B98EB39B}
		{0691B57A-016E-CC34-00A2-3232A94D6774} = {C13E73B6-616D-3195-CD22-1E55A7D1F969}
		{0AE35F2E-82D8-48C7-9BCC-930B40B7EE2D} = {0691B57A-016E-CC34-00A2-3232A94D6774}
		{17398E91-D91F-340C-59F4-1EE173D477A0} = {C13E73B6-616D-3195-CD22-1E55A7D1F969}
		{BC988A2B-F816-4F6A-B4C9-CDF6BDDC3717} = {17398E91-D91F-340C-59F4-1EE173D477A0}
		{984BB9B3-3FA3-BE33-9484-CAC21695A33C} = {827E0CD3-B72D-47B6-A68D-7590B98EB39B}
		{0F7B896F-8F20-A128-DE69-B3ECEFF261A9} = {984BB9B3-3FA3-BE33-9484-CAC21695A33C}
		{1F3947EB-E5A7-D4BC-7D18-598BC1F3E5A7} = {0F7B896F-8F20-A128-DE69-B3ECEFF261A9}
		{9655DF01-16E3-44E5-AFB2-94555025C6E7} = {1F3947EB-E5A7-D4BC-7D18-598BC1F3E5A7}
		{CEBE1C78-646E-4A69-B261-414EBAA7459F} = {1F3947EB-E5A7-D4BC-7D18-598BC1F3E5A7}
		{352D00C4-135E-4D67-822C-C050A763EDA7} = {1F3947EB-E5A7-D4BC-7D18-598BC1F3E5A7}
		{874E7206-992C-489F-83F5-3A47D6F9643B} = {1F3947EB-E5A7-D4BC-7D18-598BC1F3E5A7}
		{18CD5D4C-C90B-F91A-C84E-055041F9F9F9} = {984BB9B3-3FA3-BE33-9484-CAC21695A33C}
		{0C74EC83-CADA-D1AA-32D8-8BB46D5A82F0} = {18CD5D4C-C90B-F91A-C84E-055041F9F9F9}
		{6EB7CCED-ADFF-4901-966D-69E234C78B29} = {0C74EC83-CADA-D1AA-32D8-8BB46D5A82F0}
		{A1F171D8-39C5-483D-AC3E-56787B184FFD} = {0C74EC83-CADA-D1AA-32D8-8BB46D5A82F0}
		{07CEB35E-FC4E-47FC-A2FC-1385DD3E07F8} = {0C74EC83-CADA-D1AA-32D8-8BB46D5A82F0}
		{2CCBC6F5-6AC4-42EB-8AF1-7996AF8744D7} = {0C74EC83-CADA-D1AA-32D8-8BB46D5A82F0}
		{1BFC9479-12BF-35C3-06B8-42D89D95092B} = {984BB9B3-3FA3-BE33-9484-CAC21695A33C}
		{08275E4F-9D48-4557-7502-064122A05153} = {1BFC9479-12BF-35C3-06B8-42D89D95092B}
		{F16924D6-9020-4289-9366-BA1DB28288F0} = {08275E4F-9D48-4557-7502-064122A05153}
		{C085460D-4B6E-468B-9BE8-91E42074BF37} = {08275E4F-9D48-4557-7502-064122A05153}
		{C31E7F32-B32F-4494-9AF1-476AC7719CB4} = {08275E4F-9D48-4557-7502-064122A05153}
		{42CF600E-AD15-417A-B687-58C8643F65DA} = {08275E4F-9D48-4557-7502-064122A05153}
		{6306A8FB-679E-111F-6585-8F70E0EE6013} = {827E0CD3-B72D-47B6-A68D-7590B98EB39B}
		{A2C5261E-29FF-4D08-A5BA-8EE8800767CA} = {6306A8FB-679E-111F-6585-8F70E0EE6013}
		{63882A7C-90E5-DEE3-63BB-E6ABDBC8A365} = {827E0CD3-B72D-47B6-A68D-7590B98EB39B}
		{A800594F-9A9C-4B78-9983-4130828E5508} = {63882A7C-90E5-DEE3-63BB-E6ABDBC8A365}
		{FD11E8FD-FA37-D09A-0E3B-962F97776DA7} = {827E0CD3-B72D-47B6-A68D-7590B98EB39B}
		{4D2E7B1B-2526-EBBC-F6B3-37229C795DD3} = {FD11E8FD-FA37-D09A-0E3B-962F97776DA7}
		{1D15B8CF-C2C5-41D9-B69C-BDE4B05BEBF2} = {4D2E7B1B-2526-EBBC-F6B3-37229C795DD3}
		{C13D1D1E-F56B-E13F-2CB3-41C225EB9028} = {FD11E8FD-FA37-D09A-0E3B-962F97776DA7}
		{24A2D484-1155-4459-B861-5A312E37C22C} = {C13D1D1E-F56B-E13F-2CB3-41C225EB9028}
		{EC13D640-B63E-5DF5-8423-78F5754BCB04} = {FD11E8FD-FA37-D09A-0E3B-962F97776DA7}
		{2AE4552D-A806-4312-83FF-48BCA0AC7D8B} = {EC13D640-B63E-5DF5-8423-78F5754BCB04}
		{D0EA2945-A714-1FCD-8E4D-522B2AAF6602} = {FD11E8FD-FA37-D09A-0E3B-962F97776DA7}
		{3C1906DD-01DE-4C8A-AE7D-E61180A86680} = {D0EA2945-A714-1FCD-8E4D-522B2AAF6602}
		{FA49C31E-91F7-FF19-6028-9B3D7133ABE9} = {FD11E8FD-FA37-D09A-0E3B-962F97776DA7}
		{C1FCE134-ED4B-482A-BE54-37DECD357735} = {FA49C31E-91F7-FF19-6028-9B3D7133ABE9}
		{83D8EDDF-4362-4EAC-9DFF-24DE79F1740B} = {FD11E8FD-FA37-D09A-0E3B-962F97776DA7}
		{C1543FD9-44DE-4DE9-949C-5AC6527AC45A} = {83D8EDDF-4362-4EAC-9DFF-24DE79F1740B}
	EndGlobalSection
EndGlobal

```

## 5. .\src\apphost\appsettings.json
<a id="005---src-apphost-appsettings-json.ToLower()"></a>

```json
{
  "ConnectionStrings": {
    "PatientsDb": "User Id=postgres.vostvnaslerainoeelox;Password=pb5FfdhYVUrYpClg;Server=aws-1-sa-east-1.pooler.supabase.com;Port=6543;Database=postgres"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Aspire.Hosting.Dcp": "Warning"
    }
  }
}

```

## 6. .\src\apphost\HealthTech.AppHost.csproj
<a id="006---src-apphost-HealthTech-AppHost-csproj.ToLower()"></a>

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <Sdk Name="Aspire.AppHost.Sdk" Version="9.5.0" />

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <UserSecretsId>018c24a5-9098-49c7-aee3-2f8453516fe2</UserSecretsId>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Aspire.Hosting.AppHost" Version="9.5.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\frontends\app-shell\HealthTech.AppShell.csproj" />
    <ProjectReference Include="..\gateway\HealthTech.Gateway.csproj" />
    <ProjectReference Include="..\services\Patients\Patients.Api\HealthTech.Patients.Api.csproj" />
  </ItemGroup>

</Project>

```

## 7. .\src\apphost\Program.cs
<a id="007---src-apphost-Program-cs.ToLower()"></a>

```csharp
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// ===== Recursos de infraestrutura =====

// ConnectionStrings:schedulingdb sera repassada ao Scheduling
var schedulingDb = builder.AddConnectionString("PatientsDb");

// ===== Projetos (suas APIs e gateway) =====

// Patients API
var patientsApi = builder.AddProject<Projects.HealthTech_Patients_Api>("patients-api")
                         .WithReference(schedulingDb);
//                         .WithReference(redis)        // injeta services__redis__...
//                         .WithHttpEndpoint(env: "ASPNETCORE_URLS"); // expõe endpoint http

// Appointments API (se existir, mesmo padrão)
// var appointmentsDb = pg.AddDatabase("appointmentsdb");
// var appointmentsApi = builder.AddProject<Projects.HealthTech_Appointments_Api>("appointments-api")
//                              .WithReference(appointmentsDb)
//                              .WithReference(redis)
//                              .WithHttpEndpoint(env: "ASPNETCORE_URLS");

// Gateway (YARP) – vamos usar service discovery para localizar as APIs
var gateway = builder.AddProject<Projects.HealthTech_Gateway>("gateway")
                     .WithReference(patientsApi);

// (Opcional) Frontend Blazor WASM como projeto .NET (dev server)
var appShell = builder.AddProject<Projects.HealthTech_AppShell>("app-shell")
                      .WithReference(gateway);


// Dashboard do Aspire abre automaticamente ao rodar o AppHost
builder.Build().Run();

```

## 8. .\src\apphost\Properties\launchSettings.json
<a id="008---src-apphost-Properties-launchSettings-json.ToLower()"></a>

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:17257;http://localhost:15117",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "DOTNET_ENVIRONMENT": "Development",
        "ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL": "https://localhost:21279",
        "ASPIRE_RESOURCE_SERVICE_ENDPOINT_URL": "https://localhost:22100"
      }
    },
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "http://localhost:15117",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "DOTNET_ENVIRONMENT": "Development",
        "ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL": "http://localhost:19138",
        "ASPIRE_RESOURCE_SERVICE_ENDPOINT_URL": "http://localhost:20196"
      }
    }
  }
}

```

## 9. .\src\building-blocks\Abstractions\Class1.cs
<a id="009---src-building-blocks-Abstractions-Class1-cs.ToLower()"></a>

```csharp
namespace HealthTech.BuildingBlocks.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IDbConnectionFactory
{
    System.Data.IDbConnection Create();
}

```

## 10. .\src\building-blocks\Abstractions\HealthTech.BuildingBlocks.Abstractions.csproj
<a id="010---src-building-blocks-Abstractions-HealthTech-BuildingBlocks-Abstractions-csproj.ToLower()"></a>

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>

```

## 11. .\src\building-blocks\SharedKernel\Class1.cs
<a id="011---src-building-blocks-SharedKernel-Class1-cs.ToLower()"></a>

```csharp
namespace HealthTech.BuildingBlocks.SharedKernel;

public abstract class Entity<TId>
{
    public TId Id { get; protected set; }
    protected Entity(TId id) => Id = id;
}

public readonly record struct Result<T>(bool Success, T? Value, string? Error)
{
    public static Result<T> Ok(T value) => new(true, value, null);
    public static Result<T> Fail(string error) => new(false, default, error);
}

```

## 12. .\src\building-blocks\SharedKernel\HealthTech.BuildingBlocks.SharedKernel.csproj
<a id="012---src-building-blocks-SharedKernel-HealthTech-BuildingBlocks-SharedKernel-csproj.ToLower()"></a>

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>

```

## 13. .\src\frontends\app-shell\_Imports.razor
<a id="013---src-frontends-app-shell-_Imports-razor.ToLower()"></a>

```razor
@using System.Net.Http
@using System.Net.Http.Json
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Web.Virtualization
@using Microsoft.AspNetCore.Components.WebAssembly.Http
@using Microsoft.JSInterop
@using HealthTech.AppShell
@using HealthTech.AppShell.Layout
@using MudBlazor
@using MudBlazor.Components

```

## 14. .\src\frontends\app-shell\App.razor
<a id="014---src-frontends-app-shell-App-razor.ToLower()"></a>

```razor
@using Microsoft.AspNetCore.Components.Routing

<Router AppAssembly="@typeof(App).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
    </Found>
    <NotFound>
        <LayoutView Layout="@typeof(MainLayout)">
            <p role="alert">Página não encontrada.</p>
        </LayoutView>
    </NotFound>
</Router>

```

## 15. .\src\frontends\app-shell\appsettings.json
<a id="015---src-frontends-app-shell-appsettings-json.ToLower()"></a>

```json
{
  "Supabase": {
    "Url": "https://vostvnaslerainoeelox.supabase.co",
    "Key": "f7+B5ww1pljFVjoUlCdSMf3/WeYz3+hrXtox5pXPCYHqQ8kChtwUu1daNzjC39CBU7SMXoHPWDgBbodgWmuWtQ=="
  }
}
```

## 16. .\src\frontends\app-shell\HealthTech.AppShell.csproj
<a id="016---src-frontends-app-shell-HealthTech-AppShell-csproj.ToLower()"></a>

```xml
<Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="9.0.9" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="9.0.9" PrivateAssets="all" />
    <PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
    <PackageReference Include="MudBlazor" Version="8.13.0" />
    <PackageReference Include="Supabase" Version="1.1.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\mf-patient\HealthTech.mf.patient.csproj" />
    <ProjectReference Include="..\mf-appointment\HealthTech.mf.appointment.csproj" />
    <ProjectReference Include="..\mf-ehr\HealthTech.mf.ehr.csproj" />
    <ProjectReference Include="..\mf-billing\HealthTech.mf.billing.csproj" />
    <ProjectReference Include="..\ui-kit\HealthTech.ui.kit.csproj" />
  </ItemGroup>

</Project>

```

## 17. .\src\frontends\app-shell\Layout\MainLayout.razor
<a id="017---src-frontends-app-shell-Layout-MainLayout-razor.ToLower()"></a>

```razor
@inherits LayoutComponentBase

<MudThemeProvider Theme="@_theme" IsDarkMode="_isDarkMode" />
<MudPopoverProvider />
<MudDialogProvider />
<MudSnackbarProvider />
<MudLayout>
    <MudAppBar Elevation="1">
        <MudIconButton Icon="@Icons.Material.Filled.Menu" Color="Color.Inherit" Edge="Edge.Start" OnClick="@((e) => DrawerToggle())" />
        <MudText Typo="Typo.h5" Class="ml-3">Application</MudText>
        <MudSpacer />
        <MudIconButton Icon="@(DarkLightModeButtonIcon)" Color="Color.Inherit" OnClick="@DarkModeToggle" />
        <MudIconButton Icon="@Icons.Material.Filled.MoreVert" Color="Color.Inherit" Edge="Edge.End" />
    </MudAppBar>
    <MudDrawer id="nav-drawer" @bind-Open="_drawerOpen" ClipMode="DrawerClipMode.Always" Elevation="2">
        <NavMenu />
    </MudDrawer>
    <MudMainContent Class="pt-16 pa-4">
        @Body
    </MudMainContent>
</MudLayout>


<div id="blazor-error-ui" data-nosnippet>
    An unhandled error has occurred.
    <a href="." class="reload">Reload</a>
    <span class="dismiss">🗙</span>
</div>

@code {
    private bool _drawerOpen = true;
    private bool _isDarkMode = true;
    private MudTheme? _theme = null;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _theme = new()
        {
            PaletteLight = _lightPalette,
            PaletteDark = _darkPalette,
            LayoutProperties = new LayoutProperties()
        };
    }

    private void DrawerToggle()
    {
        _drawerOpen = !_drawerOpen;
    }

    private void DarkModeToggle()
    {
        _isDarkMode = !_isDarkMode;
    }

    private readonly PaletteLight _lightPalette = new()
    {
        Black = "#110e2d",
        AppbarText = "#424242",
        AppbarBackground = "rgba(255,255,255,0.8)",
        DrawerBackground = "#ffffff",
        GrayLight = "#e8e8e8",
        GrayLighter = "#f9f9f9",
    };

    private readonly PaletteDark _darkPalette = new()
    {
        Primary = "#7e6fff",
        Surface = "#1e1e2d",
        Background = "#1a1a27",
        BackgroundGray = "#151521",
        AppbarText = "#92929f",
        AppbarBackground = "rgba(26,26,39,0.8)",
        DrawerBackground = "#1a1a27",
        ActionDefault = "#74718e",
        ActionDisabled = "#9999994d",
        ActionDisabledBackground = "#605f6d4d",
        TextPrimary = "#b2b0bf",
        TextSecondary = "#92929f",
        TextDisabled = "#ffffff33",
        DrawerIcon = "#92929f",
        DrawerText = "#92929f",
        GrayLight = "#2a2833",
        GrayLighter = "#1e1e2d",
        Info = "#4a86ff",
        Success = "#3dcb6c",
        Warning = "#ffb545",
        Error = "#ff3f5f",
        LinesDefault = "#33323e",
        TableLines = "#33323e",
        Divider = "#292838",
        OverlayLight = "#1e1e2d80",
    };

    public string DarkLightModeButtonIcon => _isDarkMode switch
    {
        true => Icons.Material.Rounded.AutoMode,
        false => Icons.Material.Outlined.DarkMode,
    };
}



```

## 18. .\src\frontends\app-shell\Layout\NavMenu.razor
<a id="018---src-frontends-app-shell-Layout-NavMenu-razor.ToLower()"></a>

```razor

<MudNavMenu>
    <MudNavLink Href="" Match="NavLinkMatch.All" Icon="@Icons.Material.Filled.Home">Home</MudNavLink>
    <MudNavLink Href="counter" Match="NavLinkMatch.Prefix" Icon="@Icons.Material.Filled.Add">Counter</MudNavLink>
    
    <MudNavLink Href="weather" Match="NavLinkMatch.Prefix" Icon="@Icons.Material.Filled.List">Weather</MudNavLink>

</MudNavMenu>
```

## 19. .\src\frontends\app-shell\Pages\Counter.razor
<a id="019---src-frontends-app-shell-Pages-Counter-razor.ToLower()"></a>

```razor
@page "/counter"


<PageTitle>Counter</PageTitle>

<MudText Typo="Typo.h3" GutterBottom="true">Counter</MudText>

<MudText Class="mb-4">Current count: @currentCount</MudText>

<MudButton Color="Color.Primary" Variant="Variant.Filled" @onclick="IncrementCount">Click me</MudButton>

@code {
    private int currentCount = 0;

    private void IncrementCount()
    {
        currentCount++;
    }
}

```

## 20. .\src\frontends\app-shell\Pages\Home.razor
<a id="020---src-frontends-app-shell-Pages-Home-razor.ToLower()"></a>

```razor
@page "/"

<PageTitle>Home</PageTitle>

<MudText Typo="Typo.h3" GutterBottom="true">Hello, world!</MudText>
<MudText Class="mb-8">Welcome to your new app, powered by MudBlazor and the .NET 9 Template!</MudText>

<MudAlert Severity="Severity.Normal" ContentAlignment="HorizontalAlignment.Start">
    You can find documentation and examples on our website here:
    <MudLink Href="https://mudblazor.com" Target="_blank" Typo="Typo.body2" Color="Color.Primary">
        <b>www.mudblazor.com</b>
    </MudLink>
</MudAlert>

<br />
<MudText Typo="Typo.h5" GutterBottom="true">Interactivity in this Template</MudText>
<br />
<MudText Typo="Typo.body2">
    When you opt for the "Global" Interactivity Location, <br />
    the render modes are defined in App.razor and consequently apply to all child components.<br />
    In this case, providers are globally set in the MainLayout.<br />
    <br />
    On the other hand, if you choose the "Per page/component" Interactivity Location,<br />
    it is necessary to include the <br />
    <br />
    &lt;MudPopoverProvider /&gt; <br />
    &lt;MudDialogProvider /&gt; <br />
    &lt;MudSnackbarProvider /&gt; <br />
    <br />
    components on every interactive page.<br />
    <br />
    If a render mode is not specified for a page, it defaults to Server-Side Rendering (SSR),<br />
    similar to this page. While MudBlazor allows pages to be rendered in SSR,<br />
    please note that interactive features, such as buttons and dropdown menus, will not be functional.
</MudText>

<br />
<MudText Typo="Typo.h5" GutterBottom="true">What's New in Blazor with the Release of .NET 9</MudText>
<br />

<MudText Typo="Typo.h6" GutterBottom="true">Prerendering</MudText>
<MudText Typo="Typo.body2" GutterBottom="true">
    If you're exploring the features of .NET 9 Blazor,<br /> you might be pleasantly surprised to learn that each page is prerendered on the server,<br /> regardless of the selected render mode.<br /><br />
    This means that you'll need to inject all necessary services on the server,<br /> even when opting for the wasm (WebAssembly) render mode.<br /><br />
    This prerendering functionality is crucial to ensuring that WebAssembly mode feels fast and responsive,<br /> especially when it comes to initial page load times.<br /><br />
    For more information on how to detect prerendering and leverage the RenderContext, you can refer to the following link:
    <MudLink Href="https://github.com/dotnet/aspnetcore/issues/51468#issuecomment-1783568121" Target="_blank" Typo="Typo.body2" Color="Color.Primary">
        More details
    </MudLink>
</MudText>

<br />
<MudText Typo="Typo.h6" GutterBottom="true">InteractiveAuto</MudText>
<MudText Typo="Typo.body2">
    A discussion on how to achieve this can be found here:
    <MudLink Href="https://github.com/dotnet/aspnetcore/issues/51468#issue-1950424116" Target="_blank" Typo="Typo.body2" Color="Color.Primary">
        More details
    </MudLink>
</MudText>

```

## 21. .\src\frontends\app-shell\Pages\Login.razor
<a id="021---src-frontends-app-shell-Pages-Login-razor.ToLower()"></a>

```razor
@page "/login"
@inject HealthTech.AppShell.Services.SupabaseAuthService Auth
@inject IHttpClientFactory HttpClientFactory
@using System.Net.Http.Json

<h3>Login</h3>

@if (!string.IsNullOrWhiteSpace(Error))
{
    <p style="color:red">@Error</p>
}

<input @bind="Email" placeholder="email" />
<br />
<input @bind="Password" type="password" placeholder="senha" />
<br />
<button @onclick="DoLogin">Entrar</button>

@if (UserEmail is not null)
{
    <p>Logado como: @UserEmail</p>
    <button @onclick="CallProtected">Chamar API protegida (WhoAmI)</button>

    <p>@ApiResponse</p>
}

@code {
    string Email { get; set; } = "";
    string Password { get; set; } = "";
    string? UserEmail { get; set; }
    string? ApiResponse { get; set; }
    string? Error { get; set; }

    private async Task DoLogin()
    {
        Error = null;
        try
        {
            var user = await Auth.SignInAsync(Email, Password);
            UserEmail = user?.Email;

            if (UserEmail is null)
                Error = "Falha no login";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    private async Task CallProtected()
    {
        var client = HttpClientFactory.CreateClient("IdentityApi");
        // BearerAuthorizationMessageHandler injeta o token automaticamente
        var result = await client.GetFromJsonAsync<object>("api/whoami");
        ApiResponse = System.Text.Json.JsonSerializer.Serialize(result);
    }
}

```

## 22. .\src\frontends\app-shell\Pages\Weather.razor
<a id="022---src-frontends-app-shell-Pages-Weather-razor.ToLower()"></a>

```razor
@page "/weather"



<PageTitle>Weather</PageTitle>

<MudText Typo="Typo.h3" GutterBottom="true">Weather forecast</MudText>
<MudText Typo="Typo.body1" Class="mb-8">This component demonstrates fetching data from the server.</MudText>

@if (forecasts == null)
{
    <MudProgressCircular Color="Color.Default" Indeterminate="true" />
}
else
{
  <MudTable Items="forecasts" Hover="true" SortLabel="Sort By" Elevation="0" AllowUnsorted="false">
        <HeaderContent>
            <MudTh><MudTableSortLabel InitialDirection="SortDirection.Ascending" SortBy="new Func<WeatherForecast, object>(x=>x.Date)">Date</MudTableSortLabel></MudTh>
            <MudTh><MudTableSortLabel SortBy="new Func<WeatherForecast, object>(x=>x.TemperatureC)">Temp. (C)</MudTableSortLabel></MudTh>
            <MudTh><MudTableSortLabel SortBy="new Func<WeatherForecast, object>(x=>x.TemperatureF)">Temp. (F)</MudTableSortLabel></MudTh>
            <MudTh><MudTableSortLabel SortBy="new Func<WeatherForecast, object>(x=>x.Summary!)">Summary</MudTableSortLabel></MudTh>
        </HeaderContent>
        <RowTemplate>
            <MudTd DataLabel="Date">@context.Date</MudTd>
            <MudTd DataLabel="Temp. (C)">@context.TemperatureC</MudTd>
            <MudTd DataLabel="Temp. (F)">@context.TemperatureF</MudTd>
            <MudTd DataLabel="Summary">@context.Summary</MudTd>
        </RowTemplate>
        <PagerContent>
            <MudTablePager PageSizeOptions="new int[]{50, 100}" />
        </PagerContent>
    </MudTable>
}

@code {
    private WeatherForecast[]? forecasts;

    protected override async Task OnInitializedAsync()
    {
        // Simulate asynchronous loading to demonstrate a loading indicator
        await Task.Delay(500);

        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
        forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = startDate.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = summaries[Random.Shared.Next(summaries.Length)]
        }).ToArray();
    }

    private class WeatherForecast
    {
        public DateOnly Date { get; set; }
        public int TemperatureC { get; set; }
        public string? Summary { get; set; }
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}

```

## 23. .\src\frontends\app-shell\Program.cs
<a id="023---src-frontends-app-shell-Program-cs.ToLower()"></a>

```csharp
using HealthTech.AppShell;
using HealthTech.AppShell.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection; 

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

// Serviços
builder.Services.AddScoped<SupabaseAuthService>();
builder.Services.AddScoped<BearerAuthorizationMessageHandler>();

builder.Services.AddMudServices();

// HttpClient nomeado para a Identity API (lê URL de appsettings.json)
builder.Services.AddHttpClient("IdentityApi", client =>
{
    var baseUrl = builder.Configuration["Apis:Identity"];
    client.BaseAddress = new Uri(baseUrl!);
})
.AddHttpMessageHandler<BearerAuthorizationMessageHandler>();

// HttpClient “padrão” (útil para chamadas ao mesmo host do app)
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

await builder.Build().RunAsync();

```

## 24. .\src\frontends\app-shell\Properties\launchSettings.json
<a id="024---src-frontends-app-shell-Properties-launchSettings-json.ToLower()"></a>

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "inspectUri": "{wsProtocol}://{url.hostname}:{url.port}/_framework/debug/ws-proxy?browser={browserInspectUri}",
      "applicationUrl": "http://localhost:5191",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "inspectUri": "{wsProtocol}://{url.hostname}:{url.port}/_framework/debug/ws-proxy?browser={browserInspectUri}",
      "applicationUrl": "https://localhost:7069;http://localhost:5191",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}

```

## 25. .\src\frontends\app-shell\Services\BearerAuthorizationMessageHandler.cs
<a id="025---src-frontends-app-shell-Services-BearerAuthorizationMessageHandler-cs.ToLower()"></a>

```csharp
using System.Net.Http.Headers;

namespace HealthTech.AppShell.Services;

public class BearerAuthorizationMessageHandler : DelegatingHandler
{
    private readonly SupabaseAuthService _auth;

    public BearerAuthorizationMessageHandler(SupabaseAuthService auth)
    {
        _auth = auth;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // se precisar garantir que o cliente está inicializado/refresh, você pode chamar InitializeAsync aqui
        var token = _auth.GetJwt();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return await base.SendAsync(request, cancellationToken);
    }
}

```

## 26. .\src\frontends\app-shell\Services\SupabaseAuthService.cs
<a id="026---src-frontends-app-shell-Services-SupabaseAuthService-cs.ToLower()"></a>

```csharp
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Supabase.Gotrue;                  // User/Session
using SbClient = Supabase.Client;       // evita ambiguidade com Gotrue.Client

namespace HealthTech.AppShell.Services
{
    public class SupabaseAuthService
    {
        private readonly SbClient _client;

        public SupabaseAuthService(IConfiguration config)
        {
            var url = config["Supabase:Url"]!;
            var key = config["Supabase:AnonKey"]!;

            var options = new Supabase.SupabaseOptions
            {
                AutoRefreshToken = true
                // PersistSession foi removido nas versões 1.x
            };

            _client = new SbClient(url, key, options);
        }

        public async Task<User?> SignInAsync(string email, string password)
        {
            await _client.InitializeAsync();

            // Na 1.1.1 este método existe:
            var session = await _client.Auth.SignIn(email, password);

            // (se seu intellisense mostrar SignInWithPassword(email, password), pode usar esse nome)
            return session?.User;
        }

        public async Task SignOutAsync() => await _client.Auth.SignOut();

        public string? GetJwt() => _client.Auth.CurrentSession?.AccessToken;

        public User? CurrentUser() => _client.Auth.CurrentUser;
    }
}

```

## 27. .\src\frontends\app-shell\wwwroot\css\app.css
<a id="027---src-frontends-app-shell-wwwroot-css-app-css.ToLower()"></a>

```css
html, body {
    font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif;
}

h1:focus {
    outline: none;
}

a, .btn-link {
    color: #0071c1;
}

.btn-primary {
    color: #fff;
    background-color: #1b6ec2;
    border-color: #1861ac;
}

.btn:focus, .btn:active:focus, .btn-link.nav-link:focus, .form-control:focus, .form-check-input:focus {
  box-shadow: 0 0 0 0.1rem white, 0 0 0 0.25rem #258cfb;
}

.content {
    padding-top: 1.1rem;
}

.valid.modified:not([type=checkbox]) {
    outline: 1px solid #26b050;
}

.invalid {
    outline: 1px solid red;
}

.validation-message {
    color: red;
}

#blazor-error-ui {
    color-scheme: light only;
    background: lightyellow;
    bottom: 0;
    box-shadow: 0 -1px 2px rgba(0, 0, 0, 0.2);
    box-sizing: border-box;
    display: none;
    left: 0;
    padding: 0.6rem 1.25rem 0.7rem 1.25rem;
    position: fixed;
    width: 100%;
    z-index: 1000;
}

    #blazor-error-ui .dismiss {
        cursor: pointer;
        position: absolute;
        right: 0.75rem;
        top: 0.5rem;
    }

.blazor-error-boundary {
    background: url(data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNTYiIGhlaWdodD0iNDkiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyIgeG1sbnM6eGxpbms9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkveGxpbmsiIG92ZXJmbG93PSJoaWRkZW4iPjxkZWZzPjxjbGlwUGF0aCBpZD0iY2xpcDAiPjxyZWN0IHg9IjIzNSIgeT0iNTEiIHdpZHRoPSI1NiIgaGVpZ2h0PSI0OSIvPjwvY2xpcFBhdGg+PC9kZWZzPjxnIGNsaXAtcGF0aD0idXJsKCNjbGlwMCkiIHRyYW5zZm9ybT0idHJhbnNsYXRlKC0yMzUgLTUxKSI+PHBhdGggZD0iTTI2My41MDYgNTFDMjY0LjcxNyA1MSAyNjUuODEzIDUxLjQ4MzcgMjY2LjYwNiA1Mi4yNjU4TDI2Ny4wNTIgNTIuNzk4NyAyNjcuNTM5IDUzLjYyODMgMjkwLjE4NSA5Mi4xODMxIDI5MC41NDUgOTIuNzk1IDI5MC42NTYgOTIuOTk2QzI5MC44NzcgOTMuNTEzIDI5MSA5NC4wODE1IDI5MSA5NC42NzgyIDI5MSA5Ny4wNjUxIDI4OS4wMzggOTkgMjg2LjYxNyA5OUwyNDAuMzgzIDk5QzIzNy45NjMgOTkgMjM2IDk3LjA2NTEgMjM2IDk0LjY3ODIgMjM2IDk0LjM3OTkgMjM2LjAzMSA5NC4wODg2IDIzNi4wODkgOTMuODA3MkwyMzYuMzM4IDkzLjAxNjIgMjM2Ljg1OCA5Mi4xMzE0IDI1OS40NzMgNTMuNjI5NCAyNTkuOTYxIDUyLjc5ODUgMjYwLjQwNyA1Mi4yNjU4QzI2MS4yIDUxLjQ4MzcgMjYyLjI5NiA1MSAyNjMuNTA2IDUxWk0yNjMuNTg2IDY2LjAxODNDMjYwLjczNyA2Ni4wMTgzIDI1OS4zMTMgNjcuMTI0NSAyNTkuMzEzIDY5LjMzNyAyNTkuMzEzIDY5LjYxMDIgMjU5LjMzMiA2OS44NjA4IDI1OS4zNzEgNzAuMDg4N0wyNjEuNzk1IDg0LjAxNjEgMjY1LjM4IDg0LjAxNjEgMjY3LjgyMSA2OS43NDc1QzI2Ny44NiA2OS43MzA5IDI2Ny44NzkgNjkuNTg3NyAyNjcuODc5IDY5LjMxNzkgMjY3Ljg3OSA2Ny4xMTgyIDI2Ni40NDggNjYuMDE4MyAyNjMuNTg2IDY2LjAxODNaTTI2My41NzYgODYuMDU0N0MyNjEuMDQ5IDg2LjA1NDcgMjU5Ljc4NiA4Ny4zMDA1IDI1OS43ODYgODkuNzkyMSAyNTkuNzg2IDkyLjI4MzcgMjYxLjA0OSA5My41Mjk1IDI2My41NzYgOTMuNTI5NSAyNjYuMTE2IDkzLjUyOTUgMjY3LjM4NyA5Mi4yODM3IDI2Ny4zODcgODkuNzkyMSAyNjcuMzg3IDg3LjMwMDUgMjY2LjExNiA4Ni4wNTQ3IDI2My41NzYgODYuMDU0N1oiIGZpbGw9IiNGRkU1MDAiIGZpbGwtcnVsZT0iZXZlbm9kZCIvPjwvZz48L3N2Zz4=) no-repeat 1rem/1.8rem, #b32121;
    padding: 1rem 1rem 1rem 3.7rem;
    color: white;
}

    .blazor-error-boundary::after {
        content: "An error has occurred."
    }

.loading-progress {
    position: relative;
    display: block;
    width: 8rem;
    height: 8rem;
    margin: 20vh auto 1rem auto;
}

    .loading-progress circle {
        fill: none;
        stroke: #e0e0e0;
        stroke-width: 0.6rem;
        transform-origin: 50% 50%;
        transform: rotate(-90deg);
    }

        .loading-progress circle:last-child {
            stroke: #1b6ec2;
            stroke-dasharray: calc(3.141 * var(--blazor-load-percentage, 0%) * 0.8), 500%;
            transition: stroke-dasharray 0.05s ease-in-out;
        }

.loading-progress-text {
    position: absolute;
    text-align: center;
    font-weight: bold;
    inset: calc(20vh + 3.25rem) 0 auto 0.2rem;
}

    .loading-progress-text:after {
        content: var(--blazor-load-percentage-text, "Loading");
    }

code {
    color: #c02d76;
}

.form-floating > .form-control-plaintext::placeholder, .form-floating > .form-control::placeholder {
    color: var(--bs-secondary-color);
    text-align: end;
}

.form-floating > .form-control-plaintext:focus::placeholder, .form-floating > .form-control:focus::placeholder {
    text-align: start;
}
```

## 28. .\src\frontends\app-shell\wwwroot\sample-data\weather.json
<a id="028---src-frontends-app-shell-wwwroot-sample-data-weather-json.ToLower()"></a>

```json
[
  {
    "date": "2022-01-06",
    "temperatureC": 1,
    "summary": "Freezing"
  },
  {
    "date": "2022-01-07",
    "temperatureC": 14,
    "summary": "Bracing"
  },
  {
    "date": "2022-01-08",
    "temperatureC": -13,
    "summary": "Freezing"
  },
  {
    "date": "2022-01-09",
    "temperatureC": -16,
    "summary": "Balmy"
  },
  {
    "date": "2022-01-10",
    "temperatureC": -2,
    "summary": "Chilly"
  }
]

```

## 29. .\src\frontends\mf-appointment\_Imports.razor
<a id="029---src-frontends-mf-appointment-_Imports-razor.ToLower()"></a>

```razor
@using Microsoft.AspNetCore.Components.Web

```

## 30. .\src\frontends\mf-appointment\Component1.razor
<a id="030---src-frontends-mf-appointment-Component1-razor.ToLower()"></a>

```razor
<div class="my-component">
    This component is defined in the <strong>HealthTech.mf.appointment</strong> library.
</div>

```

## 31. .\src\frontends\mf-appointment\Component1.razor.css
<a id="031---src-frontends-mf-appointment-Component1-razor-css.ToLower()"></a>

```css
.my-component {
    border: 2px dashed red;
    padding: 1em;
    margin: 1em 0;
    background-image: url('background.png');
}

```

## 32. .\src\frontends\mf-appointment\ExampleJsInterop.cs
<a id="032---src-frontends-mf-appointment-ExampleJsInterop-cs.ToLower()"></a>

```csharp
using Microsoft.JSInterop;

namespace HealthTech.mf.appointment;

// This class provides an example of how JavaScript functionality can be wrapped
// in a .NET class for easy consumption. The associated JavaScript module is
// loaded on demand when first needed.
//
// This class can be registered as scoped DI service and then injected into Blazor
// components for use.

public class ExampleJsInterop : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    public ExampleJsInterop(IJSRuntime jsRuntime)
    {
        moduleTask = new (() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/HealthTech.mf.appointment/exampleJsInterop.js").AsTask());
    }

    public async ValueTask<string> Prompt(string message)
    {
        var module = await moduleTask.Value;
        return await module.InvokeAsync<string>("showPrompt", message);
    }

    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}

```

## 33. .\src\frontends\mf-appointment\HealthTech.mf.appointment.csproj
<a id="033---src-frontends-mf-appointment-HealthTech-mf-appointment-csproj.ToLower()"></a>

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>


  <ItemGroup>
    <SupportedPlatform Include="browser" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="9.0.9" />
  </ItemGroup>

</Project>

```

## 34. .\src\frontends\mf-appointment\wwwroot\exampleJsInterop.js
<a id="034---src-frontends-mf-appointment-wwwroot-exampleJsInterop-js.ToLower()"></a>

```javascript
// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

export function showPrompt(message) {
  return prompt(message, 'Type anything here');
}

```

## 35. .\src\frontends\mf-billing\_Imports.razor
<a id="035---src-frontends-mf-billing-_Imports-razor.ToLower()"></a>

```razor
@using Microsoft.AspNetCore.Components.Web

```

## 36. .\src\frontends\mf-billing\Component1.razor
<a id="036---src-frontends-mf-billing-Component1-razor.ToLower()"></a>

```razor
<div class="my-component">
    This component is defined in the <strong>HealthTech.mf.billing</strong> library.
</div>

```

## 37. .\src\frontends\mf-billing\Component1.razor.css
<a id="037---src-frontends-mf-billing-Component1-razor-css.ToLower()"></a>

```css
.my-component {
    border: 2px dashed red;
    padding: 1em;
    margin: 1em 0;
    background-image: url('background.png');
}

```

## 38. .\src\frontends\mf-billing\ExampleJsInterop.cs
<a id="038---src-frontends-mf-billing-ExampleJsInterop-cs.ToLower()"></a>

```csharp
using Microsoft.JSInterop;

namespace HealthTech.mf.billing;

// This class provides an example of how JavaScript functionality can be wrapped
// in a .NET class for easy consumption. The associated JavaScript module is
// loaded on demand when first needed.
//
// This class can be registered as scoped DI service and then injected into Blazor
// components for use.

public class ExampleJsInterop : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    public ExampleJsInterop(IJSRuntime jsRuntime)
    {
        moduleTask = new (() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/HealthTech.mf.billing/exampleJsInterop.js").AsTask());
    }

    public async ValueTask<string> Prompt(string message)
    {
        var module = await moduleTask.Value;
        return await module.InvokeAsync<string>("showPrompt", message);
    }

    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}

```

## 39. .\src\frontends\mf-billing\HealthTech.mf.billing.csproj
<a id="039---src-frontends-mf-billing-HealthTech-mf-billing-csproj.ToLower()"></a>

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>


  <ItemGroup>
    <SupportedPlatform Include="browser" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="9.0.9" />
  </ItemGroup>

</Project>

```

## 40. .\src\frontends\mf-billing\wwwroot\exampleJsInterop.js
<a id="040---src-frontends-mf-billing-wwwroot-exampleJsInterop-js.ToLower()"></a>

```javascript
// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

export function showPrompt(message) {
  return prompt(message, 'Type anything here');
}

```

## 41. .\src\frontends\mf-ehr\_Imports.razor
<a id="041---src-frontends-mf-ehr-_Imports-razor.ToLower()"></a>

```razor
@using Microsoft.AspNetCore.Components.Web

```

## 42. .\src\frontends\mf-ehr\Component1.razor
<a id="042---src-frontends-mf-ehr-Component1-razor.ToLower()"></a>

```razor
<div class="my-component">
    This component is defined in the <strong>HealthTech.mf.ehr</strong> library.
</div>

```

## 43. .\src\frontends\mf-ehr\Component1.razor.css
<a id="043---src-frontends-mf-ehr-Component1-razor-css.ToLower()"></a>

```css
.my-component {
    border: 2px dashed red;
    padding: 1em;
    margin: 1em 0;
    background-image: url('background.png');
}

```

## 44. .\src\frontends\mf-ehr\ExampleJsInterop.cs
<a id="044---src-frontends-mf-ehr-ExampleJsInterop-cs.ToLower()"></a>

```csharp
using Microsoft.JSInterop;

namespace HealthTech.mf.ehr;

// This class provides an example of how JavaScript functionality can be wrapped
// in a .NET class for easy consumption. The associated JavaScript module is
// loaded on demand when first needed.
//
// This class can be registered as scoped DI service and then injected into Blazor
// components for use.

public class ExampleJsInterop : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    public ExampleJsInterop(IJSRuntime jsRuntime)
    {
        moduleTask = new (() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/HealthTech.mf.ehr/exampleJsInterop.js").AsTask());
    }

    public async ValueTask<string> Prompt(string message)
    {
        var module = await moduleTask.Value;
        return await module.InvokeAsync<string>("showPrompt", message);
    }

    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}

```

## 45. .\src\frontends\mf-ehr\HealthTech.mf.ehr.csproj
<a id="045---src-frontends-mf-ehr-HealthTech-mf-ehr-csproj.ToLower()"></a>

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>


  <ItemGroup>
    <SupportedPlatform Include="browser" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="9.0.9" />
  </ItemGroup>

</Project>

```

## 46. .\src\frontends\mf-ehr\wwwroot\exampleJsInterop.js
<a id="046---src-frontends-mf-ehr-wwwroot-exampleJsInterop-js.ToLower()"></a>

```javascript
// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

export function showPrompt(message) {
  return prompt(message, 'Type anything here');
}

```

## 47. .\src\frontends\mf-patient\_Imports.razor
<a id="047---src-frontends-mf-patient-_Imports-razor.ToLower()"></a>

```razor
@using Microsoft.AspNetCore.Components.Web

```

## 48. .\src\frontends\mf-patient\Component1.razor
<a id="048---src-frontends-mf-patient-Component1-razor.ToLower()"></a>

```razor
<div class="my-component">
    This component is defined in the <strong>HealthTech.mf.patient</strong> library.
</div>

```

## 49. .\src\frontends\mf-patient\Component1.razor.css
<a id="049---src-frontends-mf-patient-Component1-razor-css.ToLower()"></a>

```css
.my-component {
    border: 2px dashed red;
    padding: 1em;
    margin: 1em 0;
    background-image: url('background.png');
}

```

## 50. .\src\frontends\mf-patient\ExampleJsInterop.cs
<a id="050---src-frontends-mf-patient-ExampleJsInterop-cs.ToLower()"></a>

```csharp
using Microsoft.JSInterop;

namespace HealthTech.mf.patient;

// This class provides an example of how JavaScript functionality can be wrapped
// in a .NET class for easy consumption. The associated JavaScript module is
// loaded on demand when first needed.
//
// This class can be registered as scoped DI service and then injected into Blazor
// components for use.

public class ExampleJsInterop : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    public ExampleJsInterop(IJSRuntime jsRuntime)
    {
        moduleTask = new (() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/HealthTech.mf.patient/exampleJsInterop.js").AsTask());
    }

    public async ValueTask<string> Prompt(string message)
    {
        var module = await moduleTask.Value;
        return await module.InvokeAsync<string>("showPrompt", message);
    }

    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}

```

## 51. .\src\frontends\mf-patient\HealthTech.mf.patient.csproj
<a id="051---src-frontends-mf-patient-HealthTech-mf-patient-csproj.ToLower()"></a>

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>


  <ItemGroup>
    <SupportedPlatform Include="browser" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="9.0.9" />
  </ItemGroup>

</Project>

```

## 52. .\src\frontends\mf-patient\wwwroot\exampleJsInterop.js
<a id="052---src-frontends-mf-patient-wwwroot-exampleJsInterop-js.ToLower()"></a>

```javascript
// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

export function showPrompt(message) {
  return prompt(message, 'Type anything here');
}

```

## 53. .\src\frontends\ui-kit\_Imports.razor
<a id="053---src-frontends-ui-kit-_Imports-razor.ToLower()"></a>

```razor
@using Microsoft.AspNetCore.Components.Web

```

## 54. .\src\frontends\ui-kit\Component1.razor
<a id="054---src-frontends-ui-kit-Component1-razor.ToLower()"></a>

```razor
<div class="my-component">
    This component is defined in the <strong>HealthTech.ui.kit</strong> library.
</div>

```

## 55. .\src\frontends\ui-kit\Component1.razor.css
<a id="055---src-frontends-ui-kit-Component1-razor-css.ToLower()"></a>

```css
.my-component {
    border: 2px dashed red;
    padding: 1em;
    margin: 1em 0;
    background-image: url('background.png');
}

```

## 56. .\src\frontends\ui-kit\ExampleJsInterop.cs
<a id="056---src-frontends-ui-kit-ExampleJsInterop-cs.ToLower()"></a>

```csharp
using Microsoft.JSInterop;

namespace HealthTech.ui.kit;

// This class provides an example of how JavaScript functionality can be wrapped
// in a .NET class for easy consumption. The associated JavaScript module is
// loaded on demand when first needed.
//
// This class can be registered as scoped DI service and then injected into Blazor
// components for use.

public class ExampleJsInterop : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    public ExampleJsInterop(IJSRuntime jsRuntime)
    {
        moduleTask = new (() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/HealthTech.ui.kit/exampleJsInterop.js").AsTask());
    }

    public async ValueTask<string> Prompt(string message)
    {
        var module = await moduleTask.Value;
        return await module.InvokeAsync<string>("showPrompt", message);
    }

    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}

```

## 57. .\src\frontends\ui-kit\HealthTech.ui.kit.csproj
<a id="057---src-frontends-ui-kit-HealthTech-ui-kit-csproj.ToLower()"></a>

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>


  <ItemGroup>
    <SupportedPlatform Include="browser" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="9.0.9" />
  </ItemGroup>

</Project>

```

## 58. .\src\frontends\ui-kit\wwwroot\exampleJsInterop.js
<a id="058---src-frontends-ui-kit-wwwroot-exampleJsInterop-js.ToLower()"></a>

```javascript
// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

export function showPrompt(message) {
  return prompt(message, 'Type anything here');
}

```

## 59. .\src\gateway\appsettings.Development.json
<a id="059---src-gateway-appsettings-Development-json.ToLower()"></a>

```json
{
  "ReverseProxy": {
    "Routes": {
      "patients": {
        "ClusterId": "patients",
        "Match": { "Path": "/patients/{**catch-all}" }
      }
    },
    "Clusters": {
      "patients": {
        "Destinations": {
          "d1": { "Address": "http://patients-api/" }
        }
      }
    }
  }
}

```

## 60. .\src\gateway\appsettings.json
<a id="060---src-gateway-appsettings-json.ToLower()"></a>

```json
{
  "ReverseProxy": {
    "Routes": {
      "patients": {
        "ClusterId": "patients",
        "Match": { "Path": "/patients/{**catch-all}" }
      }
    },
    "Clusters": {
      "patients": {
        "Destinations": {
          "d1": { "Address": "http://patients-api/" }
        }
      }
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}

```

## 61. .\src\gateway\HealthTech.Gateway.csproj
<a id="061---src-gateway-HealthTech-Gateway-csproj.ToLower()"></a>

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
      <PackageReference Include="Yarp.ReverseProxy" Version="*" />
      <PackageReference Include="Microsoft.Extensions.ServiceDiscovery" Version="*" />
      <PackageReference Include="Microsoft.Extensions.ServiceDiscovery.Yarp" Version="*" />
      <!-- <PackageReference Include="Microsoft.Extensions.ServiceDiscovery.Dns" Version="*" /> -->
  </ItemGroup>

</Project>

```

## 62. .\src\gateway\Program.cs
<a id="062---src-gateway-Program-cs.ToLower()"></a>

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceDiscovery();
builder.Services
    .AddReverseProxy()
    .AddServiceDiscoveryDestinationResolver()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();
app.MapReverseProxy();
app.Run();

```

## 63. .\src\gateway\Properties\launchSettings.json
<a id="063---src-gateway-Properties-launchSettings-json.ToLower()"></a>

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5026",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "https://localhost:7236;http://localhost:5026",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}

```

## 64. .\src\services\Appointments\Appointments.Api\appsettings.Development.json
<a id="064---src-services-Appointments-Appointments-Api-appsettings-Development-json.ToLower()"></a>

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}

```

## 65. .\src\services\Appointments\Appointments.Api\appsettings.json
<a id="065---src-services-Appointments-Appointments-Api-appsettings-json.ToLower()"></a>

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}

```

## 66. .\src\services\Appointments\Appointments.Api\HealthTech.Appointments.Api.csproj
<a id="066---src-services-Appointments-Appointments-Api-HealthTech-Appointments-Api-csproj.ToLower()"></a>

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="FluentValidation" Version="12.0.0" />
    <PackageReference Include="MediatR" Version="13.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.9" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.9" />
    <PackageReference Include="OpenTelemetry.Exporter.Console" Version="1.13.0" />
    <PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.13.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="9.0.6" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Appointments.Application\HealthTech.Appointments.Application.csproj" />
    <ProjectReference Include="..\Appointments.Infrastructure\HealthTech.Appointments.Infrastructure.csproj" />
    <ProjectReference Include="..\Appointments.Domain\HealthTech.Appointments.Domain.csproj" />
  </ItemGroup>

</Project>

```

## 67. .\src\services\Appointments\Appointments.Api\Program.cs
<a id="067---src-services-Appointments-Appointments-Api-Program-cs.ToLower()"></a>

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

```

## 68. .\src\services\Appointments\Appointments.Api\Properties\launchSettings.json
<a id="068---src-services-Appointments-Appointments-Api-Properties-launchSettings-json.ToLower()"></a>

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5033",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "https://localhost:7237;http://localhost:5033",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}

```

## 69. .\src\services\Appointments\Appointments.Application\Class1.cs
<a id="069---src-services-Appointments-Appointments-Application-Class1-cs.ToLower()"></a>

```csharp
namespace HealthTech.Appointments.Application;

public class Class1
{

}

```

## 70. .\src\services\Appointments\Appointments.Application\HealthTech.Appointments.Application.csproj
<a id="070---src-services-Appointments-Appointments-Application-HealthTech-Appointments-Application-csproj.ToLower()"></a>

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <ProjectReference Include="..\..\..\building-blocks\SharedKernel\HealthTech.BuildingBlocks.SharedKernel.csproj" />
    <ProjectReference Include="..\..\..\building-blocks\Abstractions\HealthTech.BuildingBlocks.Abstractions.csproj" />
    <ProjectReference Include="..\Appointments.Domain\HealthTech.Appointments.Domain.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="FluentValidation" Version="12.0.0" />
    <PackageReference Include="MediatR" Version="13.0.0" />
  </ItemGroup>

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>

```

## 71. .\src\services\Appointments\Appointments.Domain\Class1.cs
<a id="071---src-services-Appointments-Appointments-Domain-Class1-cs.ToLower()"></a>

```csharp
namespace HealthTech.Appointments.Domain;

public class Class1
{

}

```

## 72. .\src\services\Appointments\Appointments.Domain\HealthTech.Appointments.Domain.csproj
<a id="072---src-services-Appointments-Appointments-Domain-HealthTech-Appointments-Domain-csproj.ToLower()"></a>

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <ProjectReference Include="..\..\..\building-blocks\SharedKernel\HealthTech.BuildingBlocks.SharedKernel.csproj" />
  </ItemGroup>

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>

```

## 73. .\src\services\Appointments\Appointments.Infrastructure\Class1.cs
<a id="073---src-services-Appointments-Appointments-Infrastructure-Class1-cs.ToLower()"></a>

```csharp
namespace HealthTech.Appointments.Infrastructure;

public class Class1
{

}

```

## 74. .\src\services\Appointments\Appointments.Infrastructure\HealthTech.Appointments.Infrastructure.csproj
<a id="074---src-services-Appointments-Appointments-Infrastructure-HealthTech-Appointments-Infrastructure-csproj.ToLower()"></a>

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <ProjectReference Include="..\Appointments.Domain\HealthTech.Appointments.Domain.csproj" />
    <ProjectReference Include="..\..\..\building-blocks\Abstractions\HealthTech.BuildingBlocks.Abstractions.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Dapper" Version="2.1.66" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="9.0.9" />
    <PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="9.0.9" />
    <PackageReference Include="Npgsql" Version="9.0.3" />
  </ItemGroup>

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>

```

## 75. .\src\services\Identity\Identity.Api\appsettings.Development.json
<a id="075---src-services-Identity-Identity-Api-appsettings-Development-json.ToLower()"></a>

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Auth": {
    "Authority": "https://vostvnaslerainoeelox.supabase.co/auth/v1",
    "Issuer": "https://vostvnaslerainoeelox.supabase.co/auth/v1",
    "Audience": "authenticated"
  },
  "Supabase": {
  "Url": "https://vostvnaslerainoeelox.supabase.co",
  "Key": "f7+B5ww1pljFVjoUlCdSMf3/WeYz3+hrXtox5pXPCYHqQ8kChtwUu1daNzjC39CBU7SMXoHPWDgBbodgWmuWtQ=="
  }
}

```

## 76. .\src\services\Identity\Identity.Api\appsettings.json
<a id="076---src-services-Identity-Identity-Api-appsettings-json.ToLower()"></a>

```json
{
  "Auth": {
    "Authority": "https://vostvnaslerainoeelox.supabase.co/auth/v1",
    "Issuer": "https://vostvnaslerainoeelox.supabase.co/auth/v1",
    "Audience": "authenticated"
  },
  "Supabase": {
  "Url": "https://vostvnaslerainoeelox.supabase.co",
  "Key": "f7+B5ww1pljFVjoUlCdSMf3/WeYz3+hrXtox5pXPCYHqQ8kChtwUu1daNzjC39CBU7SMXoHPWDgBbodgWmuWtQ=="
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}

```

## 77. .\src\services\Identity\Identity.Api\Controllers\WhoAmIController.cs
<a id="077---src-services-Identity-Identity-Api-Controllers-WhoAmIController-cs.ToLower()"></a>

```csharp

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthTech.Identity.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WhoAmIController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public IActionResult Get()
        {
            var userId = User.FindFirst("sub")?.Value;
            var email = User.FindFirst("email")?.Value;
            return Ok(new { userId, email, time = DateTime.UtcNow });
        }

        [HttpGet("public")]
        [AllowAnonymous]
        public IActionResult Public() => Ok(new { ok = true });
    }
}

```

## 78. .\src\services\Identity\Identity.Api\HealthTech.Identity.Api.csproj
<a id="078---src-services-Identity-Identity-Api-HealthTech-Identity-Api-csproj.ToLower()"></a>

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="FluentValidation" Version="12.0.0" />
    <PackageReference Include="MediatR" Version="13.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.9" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.9" />
    <PackageReference Include="OpenTelemetry.Exporter.Console" Version="1.13.0" />
    <PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.13.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="9.0.6" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Identity.Application\HealthTech.Identity.Application.csproj" />
    <ProjectReference Include="..\Identity.Infrastructure\HealthTech.Identity.Infrastructure.csproj" />
    <ProjectReference Include="..\Identity.Domain\HealthTech.Identity.Domain.csproj" />
  </ItemGroup>

</Project>

```

## 79. .\src\services\Identity\Identity.Api\Program.cs
<a id="079---src-services-Identity-Identity-Api-Program-cs.ToLower()"></a>

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var authSection = builder.Configuration.GetSection("Auth");

        options.Authority = authSection["Authority"];
        options.RequireHttpsMetadata = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = authSection["Audience"],
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();
// ---------- MVC / Controllers ----------
builder.Services.AddControllers();
// ---------- Swagger com Bearer ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity.Api", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Bearer recebido do Supabase. Ex: **Bearer {seu_token}**",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});


var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // por padrão, protegido por [Authorize] nos controllers/ações

app.Run();


```

## 80. .\src\services\Identity\Identity.Api\Properties\launchSettings.json
<a id="080---src-services-Identity-Identity-Api-Properties-launchSettings-json.ToLower()"></a>

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5134",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "https://localhost:7218;http://localhost:5134",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}

```

## 81. .\src\services\Identity\Identity.Application\Class1.cs
<a id="081---src-services-Identity-Identity-Application-Class1-cs.ToLower()"></a>

```csharp
namespace HealthTech.Identity.Application;

public class Class1
{

}

```

## 82. .\src\services\Identity\Identity.Application\HealthTech.Identity.Application.csproj
<a id="082---src-services-Identity-Identity-Application-HealthTech-Identity-Application-csproj.ToLower()"></a>

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <ProjectReference Include="..\..\..\building-blocks\SharedKernel\HealthTech.BuildingBlocks.SharedKernel.csproj" />
    <ProjectReference Include="..\..\..\building-blocks\Abstractions\HealthTech.BuildingBlocks.Abstractions.csproj" />
    <ProjectReference Include="..\Identity.Domain\HealthTech.Identity.Domain.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="FluentValidation" Version="12.0.0" />
    <PackageReference Include="MediatR" Version="13.0.0" />
  </ItemGroup>

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>

```

## 83. .\src\services\Identity\Identity.Domain\Class1.cs
<a id="083---src-services-Identity-Identity-Domain-Class1-cs.ToLower()"></a>

```csharp
namespace HealthTech.Identity.Domain;

public class Class1
{

}

```

## 84. .\src\services\Identity\Identity.Domain\HealthTech.Identity.Domain.csproj
<a id="084---src-services-Identity-Identity-Domain-HealthTech-Identity-Domain-csproj.ToLower()"></a>

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <ProjectReference Include="..\..\..\building-blocks\SharedKernel\HealthTech.BuildingBlocks.SharedKernel.csproj" />
  </ItemGroup>

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>

```

## 85. .\src\services\Identity\Identity.Infrastructure\Class1.cs
<a id="085---src-services-Identity-Identity-Infrastructure-Class1-cs.ToLower()"></a>

```csharp
namespace HealthTech.Identity.Infrastructure;

public class Class1
{

}

```

## 86. .\src\services\Identity\Identity.Infrastructure\HealthTech.Identity.Infrastructure.csproj
<a id="086---src-services-Identity-Identity-Infrastructure-HealthTech-Identity-Infrastructure-csproj.ToLower()"></a>

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <ProjectReference Include="..\Identity.Domain\HealthTech.Identity.Domain.csproj" />
    <ProjectReference Include="..\..\..\building-blocks\Abstractions\HealthTech.BuildingBlocks.Abstractions.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Dapper" Version="2.1.66" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="9.0.9" />
    <PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="9.0.9" />
    <PackageReference Include="Npgsql" Version="9.0.3" />
  </ItemGroup>

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>

```

## 87. .\src\services\Patients\Patients.Api\appsettings.Development.json
<a id="087---src-services-Patients-Patients-Api-appsettings-Development-json.ToLower()"></a>

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }, 
  "ConnectionStrings": {
    "PatientsDb": "User Id=postgres.vostvnaslerainoeelox;Password=pb5FfdhYVUrYpClg;Server=aws-1-sa-east-1.pooler.supabase.com;Port=6543;Database=postgres;Command Timeout=120;Multiplexing =true;Include Error Detail=true;MaxPoolSize = 100;Pooling = true;SSL Mode=Require;Trust Server Certificate=true;"
  }
}
```

## 88. .\src\services\Patients\Patients.Api\appsettings.json
<a id="088---src-services-Patients-Patients-Api-appsettings-json.ToLower()"></a>

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}

```

## 89. .\src\services\Patients\Patients.Api\HealthTech.Patients.Api.csproj
<a id="089---src-services-Patients-Patients-Api-HealthTech-Patients-Api-csproj.ToLower()"></a>

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="FluentValidation" Version="12.0.0" />
    <PackageReference Include="MediatR" Version="13.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.9" />
    <PackageReference Include="OpenTelemetry.Exporter.Console" Version="1.13.0" />
    <PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.13.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="9.0.6" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Patients.Application\HealthTech.Patients.Application.csproj" />
    <ProjectReference Include="..\Patients.Infrastructure\HealthTech.Patients.Infrastructure.csproj" />
    <ProjectReference Include="..\Patients.Domain\HealthTech.Patients.Domain.csproj" />
  </ItemGroup>

</Project>

```

## 90. .\src\services\Patients\Patients.Api\Program.cs
<a id="090---src-services-Patients-Patients-Api-Program-cs.ToLower()"></a>

```csharp
using MediatR;
using HealthTech.Patients.Application;
using HealthTech.Patients.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models; // ✅ correct namespace


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<RegisterPatientHandler>());

builder.Services.AddPatientsInfra();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.Authority = builder.Configuration["Auth:Authority"]; // Supabase JWKS/Issuer
        opt.TokenValidationParameters.ValidateAudience = false;
        opt.TokenValidationParameters.ValidIssuer = builder.Configuration["Auth:Issuer"]; 
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Patients API", Version = "v1" }));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/patients", async (RegisterPatientCommand cmd, ISender sender) =>
{
    var result = await sender.Send(cmd);
    return result.Success ? Results.Created($"/patients/{result.Value}", new { id = result.Value }) : Results.BadRequest(result.Error);
}).RequireAuthorization();

app.Run();

```

## 91. .\src\services\Patients\Patients.Api\Properties\launchSettings.json
<a id="091---src-services-Patients-Patients-Api-Properties-launchSettings-json.ToLower()"></a>

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "launchUrl": "swagger",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5291",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "launchUrl": "swagger",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "https://localhost:7277;http://localhost:5291",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}

```

## 92. .\src\services\Patients\Patients.Application\Application.cs
<a id="092---src-services-Patients-Patients-Application-Application-cs.ToLower()"></a>

```csharp
namespace HealthTech.Patients.Application;
using MediatR;
using HealthTech.BuildingBlocks.SharedKernel;
using HealthTech.Patients.Domain;

public record RegisterPatientCommand(string FullName, string Document, DateOnly BirthDate) : IRequest<Result<Guid>>;

public interface IPatientRepository
{
    Task AddAsync(Patient entity, CancellationToken ct);
    Task<Patient?> GetAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<Patient>> ListAsync(int skip, int take, CancellationToken ct);
}

public sealed class RegisterPatientHandler(IPatientRepository repo, HealthTech.BuildingBlocks.Abstractions.IUnitOfWork uow) : IRequestHandler<RegisterPatientCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterPatientCommand request, CancellationToken ct)
    {
        var entity = new Patient(PatientId.New(), request.FullName, request.Document, request.BirthDate);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return Result<Guid>.Ok(entity.Id.Value);
    }
}

```

## 93. .\src\services\Patients\Patients.Application\HealthTech.Patients.Application.csproj
<a id="093---src-services-Patients-Patients-Application-HealthTech-Patients-Application-csproj.ToLower()"></a>

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <ProjectReference Include="..\..\..\building-blocks\SharedKernel\HealthTech.BuildingBlocks.SharedKernel.csproj" />
    <ProjectReference Include="..\..\..\building-blocks\Abstractions\HealthTech.BuildingBlocks.Abstractions.csproj" />
    <ProjectReference Include="..\Patients.Domain\HealthTech.Patients.Domain.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="FluentValidation" Version="12.0.0" />
    <PackageReference Include="MediatR" Version="13.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.9" />
    <PackageReference Include="Microsoft.OpenApi" Version="1.6.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
  </ItemGroup>

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>

```

## 94. .\src\services\Patients\Patients.Domain\HealthTech.Patients.Domain.csproj
<a id="094---src-services-Patients-Patients-Domain-HealthTech-Patients-Domain-csproj.ToLower()"></a>

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <ProjectReference Include="..\..\..\building-blocks\SharedKernel\HealthTech.BuildingBlocks.SharedKernel.csproj" />
  </ItemGroup>

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>

```

## 95. .\src\services\Patients\Patients.Domain\Patient.cs
<a id="095---src-services-Patients-Patients-Domain-Patient-cs.ToLower()"></a>

```csharp
namespace HealthTech.Patients.Domain;
using HealthTech.BuildingBlocks.SharedKernel;

public readonly record struct PatientId(Guid Value)
{
    public static PatientId New() => new(Guid.NewGuid());
}

public sealed class Patient : Entity<PatientId>
{
    public string FullName { get; private set; }
    public string Document { get; private set; }
    public DateOnly BirthDate { get; private set; }
    
    private Patient() : base(new PatientId(Guid.Empty)) { FullName = Document = string.Empty; BirthDate = default; }

    public Patient(PatientId id, string fullName, string document, DateOnly birthDate) : base(id)
    {

        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("FullName obrigatório");
        if (string.IsNullOrWhiteSpace(document)) throw new ArgumentException("Document obrigatório");
        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow)) throw new ArgumentException("BirthDate inválido");
        FullName = fullName; Document = document; BirthDate = BirthDate ;
    }
}

```

## 96. .\src\services\Patients\Patients.Infrastructure\HealthTech.Patients.Infrastructure.csproj
<a id="096---src-services-Patients-Patients-Infrastructure-HealthTech-Patients-Infrastructure-csproj.ToLower()"></a>

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <ProjectReference Include="..\Patients.Domain\HealthTech.Patients.Domain.csproj" />
    <ProjectReference Include="..\Patients.Application\HealthTech.Patients.Application.csproj" />
    <ProjectReference Include="..\..\..\building-blocks\Abstractions\HealthTech.BuildingBlocks.Abstractions.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Dapper" Version="2.1.66" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="9.0.9" />
    <PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="9.0.9" />
    <PackageReference Include="Npgsql" Version="9.0.3" />
  </ItemGroup>

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>

```

## 97. .\src\services\Patients\Patients.Infrastructure\Infrastructure.cs
<a id="097---src-services-Patients-Patients-Infrastructure-Infrastructure-cs.ToLower()"></a>

```csharp
namespace HealthTech.Patients.Infrastructure;
using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HealthTech.BuildingBlocks.Abstractions;
using HealthTech.Patients.Application;
using HealthTech.Patients.Domain;

public sealed class NpgsqlConnectionFactory(IConfiguration cfg) : IDbConnectionFactory
{
    public IDbConnection Create()
    {
        var cs = cfg.GetConnectionString("PatientsDb") ?? throw new InvalidOperationException("Missing PatientsDb connection string");
        return new Npgsql.NpgsqlConnection(cs);
    }
}

public sealed class UnitOfWork(IDbConnectionFactory f) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(0); // Dapper: transacione por operação conforme necessidade.
}

public sealed class PatientRepository(IDbConnectionFactory f) : IPatientRepository
{
    public async Task AddAsync(Patient entity, CancellationToken ct)
    {
        const string sql = "INSERT INTO patients.patient(id, full_name, document, birth_date) VALUES (@Id, @FullName, @Document, @BirthDate);";
        using var con = f.Create();
        await con.ExecuteAsync(new CommandDefinition(sql, new { Id = entity.Id.Value, entity.FullName, entity.Document, BirthDate = entity.BirthDate }, cancellationToken: ct));
    }

    public async Task<Patient?> GetAsync(Guid id, CancellationToken ct)
    {
        const string sql = "SELECT id, full_name, document, birth_date FROM patients.patient WHERE id = @Id";
        using var con = f.Create();
        var row = await con.QuerySingleOrDefaultAsync(sql, new { Id = id });
        if (row is null) return null;
        return new Patient(new PatientId((Guid)row.id), (string)row.full_name, (string)row.document, DateOnly.FromDateTime((DateTime)row.birth_date));
    }

    public async Task<IEnumerable<Patient>> ListAsync(int skip, int take, CancellationToken ct)
    {
        const string sql = "SELECT id, full_name, document, birth_date FROM patients.patient ORDER BY full_name OFFSET @Skip LIMIT @Take";
        using var con = f.Create();
        var rows = await con.QueryAsync(sql, new { Skip = skip, Take = take });
        return rows.Select(r => new Patient(new PatientId((Guid)r.id), (string)r.full_name, (string)r.document, DateOnly.FromDateTime((DateTime)r.birth_date)));
    }
}

public static class PatientsInfraRegistration
{
    public static IServiceCollection AddPatientsInfra(this IServiceCollection services)
    {
        services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        return services;
    }
}

```

