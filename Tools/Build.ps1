param(
    [ValidateSet('Verify','Windows','Web','Android')][string]$Target = 'Verify',
    [string]$Unity = 'C:\Program Files\Unity\Hub\Editor\6000.6.9f1\Editor\Unity.exe'
)
$ErrorActionPreference = 'Stop'
$projectPath = Split-Path $PSScriptRoot -Parent
$logDirectory = Join-Path $projectPath 'Builds\Logs'
New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
$logPath = Join-Path $logDirectory ($Target + '.log')
$method = if ($Target -eq 'Verify') { 'ProjectBuild.SetupAndVerify' } else { 'ProjectBuild.' + $Target }
$buildTarget = switch ($Target) { 'Web' { ' -buildTarget WebGL' } 'Android' { ' -buildTarget Android' } default { '' } }
$arguments = '-batchmode -nographics -quit' + $buildTarget + ' -projectPath "' + $projectPath + '" -executeMethod ' + $method + ' -logFile "' + $logPath + '"'
$process = Start-Process -FilePath $Unity -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
Get-Content -LiteralPath $logPath -Tail 35
if ($process.ExitCode -ne 0) { throw "Unity exited with code $($process.ExitCode). See $logPath" }
Write-Output "Completed $Target. Log: $logPath"
