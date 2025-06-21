$ErrorActionPreference = "Stop"

Write-Host "Building all projects..."

dotnet build ./src/Silhouette/Silhouette.csproj -c Release
dotnet publish ./src/ManagedDotnetProfiler/ManagedDotnetProfiler.csproj -c Release -r win-x64
dotnet build ./src/TestApp/TestApp.csproj -c Release

Write-Host "Build finished."
