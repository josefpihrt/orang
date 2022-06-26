dotnet build "../src/CommandLine/CommandLine.csproj" -c Debug -v n /p:RunCodeAnalysis=false /nr:false /m

if(!$?) { Read-Host; Exit }

dotnet pack -c Debug --no-build -v normal "../src/CommandLine/CommandLine.csproj"

dotnet tool uninstall orang.dotnet.cli -g

dotnet tool install orang.dotnet.cli --version 1.0.0 -g --add-source "../src/CommandLine/bin/Debug"

Write-Host "DONE"
Read-Host
