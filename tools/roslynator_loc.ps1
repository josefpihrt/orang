#dotnet tool install -g roslynator.dotnet.cli

roslynator loc "../src/Orang.sln" `
 --ignore-block-boundary `
 --verbosity d

Write-Host "DONE"
Read-Host 