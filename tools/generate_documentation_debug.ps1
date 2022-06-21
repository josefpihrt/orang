dotnet build "../src/DocumentationGenerator/DocumentationGenerator.csproj" -c Debug /p:Deterministic=true -v m

if(!$?) { Read-Host; Exit }

dotnet "../src/DocumentationGenerator/bin/Debug/netcoreapp3.1/Orang.DocumentationGenerator.dll" "../docs/cli"

Write-Host "DONE"
Read-Host
