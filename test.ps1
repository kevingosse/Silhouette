$ErrorActionPreference = "Stop"

$profilerDll = Resolve-Path ./src/ManagedDotnetProfiler/bin/Release/net9.0/win-x64/native/ManagedDotnetProfiler.dll

$env:CORECLR_ENABLE_PROFILING=1
$env:CORECLR_PROFILER="{0A96F866-D763-4099-8E4E-ED1801BE9FBC}"
$env:CORECLR_PROFILER_PATH=$profilerDll

Write-Host "Running TestApp with profiler..."
./src/TestApp/bin/Release/net9.0/TestApp.exe
$exitCode = $LASTEXITCODE

if ($exitCode -ne 0) {
    Write-Error "TestApp failed with exit code $exitCode"
}
exit $exitCode
