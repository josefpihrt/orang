dotnet clean "../src/CommandLine/CommandLine.csproj"

dotnet build "../src/CommandLine/CommandLine.csproj" -c Release -v n /p:RunCodeAnalysis=false /nr:false /m

if(!$?) { Read-Host; Exit }

dotnet pack -c Release --no-build -v normal "../src/CommandLine/CommandLine.csproj"

dotnet tool uninstall orang.dotnet.cli -g

dotnet tool install orang.dotnet.cli --version 1.0.0 -g --add-source "../src/CommandLine/bin/Release"

Write-Host "DONE"
