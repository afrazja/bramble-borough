$ErrorActionPreference = 'Stop'
$projectPath = Split-Path $PSScriptRoot -Parent
$sourcePath = Join-Path $projectPath 'Builds\Web'
$destinationPath = Join-Path $projectPath 'docs'
if (!(Test-Path (Join-Path $sourcePath 'index.html'))) { throw 'Build Web first.' }
New-Item -ItemType Directory -Path $destinationPath -Force | Out-Null
Copy-Item -Path (Join-Path $sourcePath '*') -Destination $destinationPath -Recurse -Force
$wasmPath = Join-Path $destinationPath 'Build\Web.wasm'
if (!(Test-Path $wasmPath)) { throw 'Web build is incomplete: Web.wasm is missing.' }
$version = (Get-FileHash -LiteralPath $wasmPath -Algorithm SHA256).Hash.Substring(0,12).ToLowerInvariant()
$indexPath = Join-Path $destinationPath 'index.html'
$html = Get-Content -LiteralPath $indexPath -Raw
foreach ($runtimeFile in @('Web.loader.js','Web.framework.js','Web.data','Web.wasm')) {
    $html = $html.Replace(('/' + $runtimeFile + '"'), ('/' + $runtimeFile + '?v=' + $version + '"'))
}
Set-Content -LiteralPath $indexPath -Value $html -Encoding utf8
New-Item -ItemType File -Path (Join-Path $destinationPath '.nojekyll') -Force | Out-Null
Write-Output 'Staged Web in docs. Review and commit the files before publishing.'
