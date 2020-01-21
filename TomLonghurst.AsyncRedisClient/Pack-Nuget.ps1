$version = Read-Host "Please enter the version number to use in the build"

dotnet pack -c Release -p:PackageVersion=$version

Pause