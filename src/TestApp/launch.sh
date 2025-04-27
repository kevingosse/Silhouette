export CORECLR_ENABLE_PROFILING=1

# debug so loading:
#export LD_DEBUG=libs

export CORECLR_PROFILER={0A96F866-D763-4099-8E4E-ED1801BE9FBC}
export CORECLR_PROFILER_PATH=./../ManagedDotnetProfiler/bin/Release/net9.0/linux-x64/native/ManagedDotnetProfiler.so

# create dump and crash json

#export DOTNET_EnableCrashReport=1
#export DOTNET_CreateDumpDiagnostics=1
#export DOTNET_DbgEnableMiniDump=1
#export DOTNET_DbgMiniDumpType=4
#export DOTNET_DbgMiniDumpName="dump.dmp"

export LD_LIBRARY_PATH=./../ManagedDotnetProfiler/bin/Release/net9.0/linux-x64/native/:$LD_LIBRARY_PATH

# dotnet publish /p:NativeLib=Shared /p:SelfContained=true -r linux-x64 -c Release

./bin/Release/net9.0/TestApp