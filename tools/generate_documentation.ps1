dotnet build "../src/DocumentationGenerator/DocumentationGenerator.csproj" -c Release /p:Deterministic=true,TreatWarningsAsErrors=true,WarningsNotAsErrors=1591 -v m /m

if(!$?) { Read-Host; Exit }

dotnet "../src/DocumentationGenerator/bin/Release/net7.0/Orang.DocumentationGenerator.dll" "../docs/cli" "../src/DocumentationGenerator/data"

if(!$?) { Read-Host; Exit }

Write-Host "DONE"
