dotnet build "../src/DocumentationGenerator/DocumentationGenerator.csproj" -c Debug /p:Deterministic=true -v m

if(!$?) { Read-Host; Exit }

dotnet "../src/DocumentationGenerator/bin/Debug/net7.0/Orang.DocumentationGenerator.dll" "build/cli" "../src/DocumentationGenerator/data"

Write-Host "DONE"
