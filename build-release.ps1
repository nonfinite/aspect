msbuild /restore /p:Configuration=Release

Push-Location (Join-Path $PSScriptRoot "releases")

Move-Item -Force "Setup.exe" "AspectSetup.exe"
Move-Item -Force "Setup.msi" "AspectSetup.msi"

Pop-Location