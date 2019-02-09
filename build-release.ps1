Push-Location $PSScriptRoot

msbuild /restore /p:Configuration=Release /t:aspect:PackageRelease

Push-Location (Join-Path $PSScriptRoot "releases")

Move-Item -Force "Setup.exe" "AspectSetup.exe"
Move-Item -Force "Setup.msi" "AspectSetup.msi"

Pop-Location

Pop-Location
