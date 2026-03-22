$ErrorActionPreference = "Stop"

$profilerDll = Resolve-Path ./src/ManagedDotnetProfiler/bin/Release/net9.0/win-x64/native/ManagedDotnetProfiler.dll
$testApp = Resolve-Path ./src/TestApp/bin/Release/net9.0/TestApp.exe

$envVars = @{
    'CORECLR_ENABLE_PROFILING' = '1'
    'CORECLR_PROFILER' = '{0A96F866-D763-4099-8E4E-ED1801BE9FBC}'
    'CORECLR_PROFILER_PATH' = "$profilerDll"
}

# Mode 1: Startup profiler
Write-Host "Running TestApp with profiler (startup mode)..."

$p = Start-Process -FilePath $testApp `
    -NoNewWindow -Wait -PassThru -Environment $envVars

if ($p.ExitCode -ne 0) {
    Write-Error "TestApp (startup mode) failed with exit code $($p.ExitCode)"
}

# Mode 2: Attach profiler at runtime
Write-Host ""
Write-Host "Running TestApp with profiler (attach mode)..."

$p = Start-Process -FilePath $testApp `
    -ArgumentList "--attach","$profilerDll" `
    -NoNewWindow -Wait -PassThru

if ($p.ExitCode -ne 0) {
    Write-Error "TestApp (attach mode) failed with exit code $($p.ExitCode)"
}

exit 0
