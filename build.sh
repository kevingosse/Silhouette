#!/usr/bin/env bash
set -e

dotnet build ./src/Silhouette/Silhouette.csproj -c Release
dotnet publish ./src/ManagedDotnetProfiler/ManagedDotnetProfiler.csproj -c Release -r linux-x64
dotnet build ./src/TestApp/TestApp.csproj -c Release

echo "Build finished."
