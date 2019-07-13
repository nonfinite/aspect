Push-Location $PSScriptRoot

msbuild /restore /p:Configuration=Release "/p:Platform=Any CPU" /t:aspect:PackageRelease

Push-Location (Join-Path $PSScriptRoot "releases")

Move-Item -Force "Setup.exe" "AspectSetup.exe"

Pop-Location

Pop-Location
