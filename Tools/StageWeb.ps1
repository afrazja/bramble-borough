$ErrorActionPreference = 'Stop'
$projectPath = Split-Path $PSScriptRoot -Parent
$sourcePath = Join-Path $projectPath 'Builds\Web'
$destinationPath = Join-Path $projectPath 'docs'
if (!(Test-Path (Join-Path $sourcePath 'index.html'))) { throw 'Build Web first.' }
New-Item -ItemType Directory -Path $destinationPath -Force | Out-Null
Copy-Item -Path (Join-Path $sourcePath '*') -Destination $destinationPath -Recurse -Force
New-Item -ItemType File -Path (Join-Path $destinationPath '.nojekyll') -Force | Out-Null
Write-Output 'Staged Web in docs. Review and commit the files before publishing.'
