msbuild /restore /p:Configuration=Release

Push-Location (Join-Path $PSScriptRoot "releases")

Move-Item "Setup.exe" "AspectSetup.exe"
Move-Item "Setup.msi" "AspectSetup.msi"

Pop-Location