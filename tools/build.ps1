#dotnet tool install -g orang.dotnet.cli

orang delete "../src" -a d -n "bin|obj" e --content-only -t n -y su s
Write-Host

dotnet restore --force "../src/Orang.sln"

dotnet build "../src/Orang.sln" -c Release -v n /p:RunCodeAnalysis=false,Deterministic=true,TreatWarningsAsErrors=true,WarningsNotAsErrors=1591 /nr:false /m

 if(!$?) { Read-Host; Exit }

dotnet "../src/DocumentationGenerator/bin/Release/netcoreapp3.1/Orang.DocumentationGenerator.dll" "../docs/cli"

if(!$?) { Read-Host; Exit }

dotnet "../src/CommandLine/bin/Release/netcoreapp3.1/Orang.dll" help -m -v d > "../docs/cli/manual.txt"

if(!$?) { Read-Host; Exit }

dotnet test -c Release --no-build "../src/Tests/CommandLine.Tests/CommandLine.Tests.csproj"

if(!$?) { Read-Host; Exit }

dotnet pack -c Release --no-build -v normal "../src/CommandLine/CommandLine.csproj"

$outDir="../out/Release"

New-Item -Path "$outDir" -ItemType directory
Remove-Item "$outDir/*"
Copy-Item -Path "../src/CommandLine/bin/Release/Orang.DotNet.Cli.*.nupkg" -Destination "$outDir"

Write-Host "DONE"
Read-Host
