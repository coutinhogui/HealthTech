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
