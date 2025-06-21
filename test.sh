#!/usr/bin/env bash
set -e

PROFILER_DLL=$(realpath ./src/ManagedDotnetProfiler/bin/Release/net9.0/linux-x64/native/ManagedDotnetProfiler.so)

export LD_LIBRARY_PATH=$(realpath ./src/ManagedDotnetProfiler/bin/Release/net9.0/linux-x64/native/):$LD_LIBRARY_PATH

export CORECLR_ENABLE_PROFILING=1
export CORECLR_PROFILER="{0A96F866-D763-4099-8E4E-ED1801BE9FBC}"
export CORECLR_PROFILER_PATH="$PROFILER_DLL"

echo "Running TestApp with profiler..."
./src/TestApp/bin/Release/net9.0/TestApp
exit $?
