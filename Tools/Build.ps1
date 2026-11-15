param(
    [ValidateSet('Verify','Windows','Web','Android')][string]$Target = 'Verify',
    [string]$Unity = 'C:\Program Files\Unity\Hub\Editor\6000.6.9f1\Editor\Unity.exe',
    [string]$AndroidSdk,
    [string]$AndroidNdk
)
$ErrorActionPreference = 'Stop'
$projectPath = Split-Path $PSScriptRoot -Parent
$logDirectory = Join-Path $projectPath 'Builds\Logs'
New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
$logPath = Join-Path $logDirectory ($Target + '.log')
$method = if ($Target -eq 'Verify') { 'ProjectBuild.SetupAndVerify' } else { 'ProjectBuild.' + $Target }
$buildTarget = switch ($Target) { 'Web' { ' -buildTarget WebGL' } 'Android' { ' -buildTarget Android' } default { '' } }
$arguments = '-batchmode -nographics -quit' + $buildTarget + ' -projectPath "' + $projectPath + '" -executeMethod ' + $method + ' -logFile "' + $logPath + '"'
$previousSdk = $env:BRAMBLE_ANDROID_SDK
$previousNdk = $env:BRAMBLE_ANDROID_NDK
try {
    if ($Target -eq 'Android') {
        $taskRoot = Split-Path (Split-Path $projectPath -Parent) -Parent
        $localSdk = Join-Path $taskRoot 'work\android-toolchain\sdk'
        $localNdk = Join-Path $taskRoot 'work\android-ndk\extracted\android-ndk-r27c'
        if (!$AndroidSdk -and !$previousSdk -and (Test-Path (Join-Path $localSdk 'cmdline-tools\latest\bin\sdkmanager.bat'))) { $AndroidSdk = $localSdk }
        if (!$AndroidNdk -and !$previousNdk -and (Test-Path (Join-Path $localNdk 'source.properties'))) { $AndroidNdk = $localNdk }
        if ($AndroidSdk) { $env:BRAMBLE_ANDROID_SDK = $AndroidSdk }
        if ($AndroidNdk) { $env:BRAMBLE_ANDROID_NDK = $AndroidNdk }
    }
    $process = Start-Process -FilePath $Unity -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
} finally {
    $env:BRAMBLE_ANDROID_SDK = $previousSdk
    $env:BRAMBLE_ANDROID_NDK = $previousNdk
}
Get-Content -LiteralPath $logPath -Tail 35
if ($process.ExitCode -ne 0) { throw "Unity exited with code $($process.ExitCode). See $logPath" }
Write-Output "Completed $Target. Log: $logPath"
