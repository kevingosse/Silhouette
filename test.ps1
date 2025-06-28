$ErrorActionPreference = "Stop"

$profilerDll = Resolve-Path ./src/ManagedDotnetProfiler/bin/Release/net9.0/win-x64/native/ManagedDotnetProfiler.dll

$envVars = @{
    'CORECLR_ENABLE_PROFILING' = '1'
    'CORECLR_PROFILER' = '{0A96F866-D763-4099-8E4E-ED1801BE9FBC}'
    'CORECLR_PROFILER_PATH' = "$profilerDll"
}

Write-Host "Running TestApp with profiler..."

$p = Start-Process -FilePath ./src/TestApp/bin/Release/net9.0/TestApp.exe `
    -NoNewWindow -Wait -PassThru -Environment $envVars

$exitCode = $p.ExitCode

if ($exitCode -ne 0) {
    Write-Error "TestApp failed with exit code $exitCode"
}
exit $exitCode
