dotnet new tool-manifest --force
dotnet tool install roslynator.dotnet.cli

dotnet roslynator find-symbols "../src/Orang.sln" `
 --visibility public internal private `
 --symbol-groups type member `
 --without-attributes `
  "System.ObsoleteAttribute" `
 --ignored-projects CommandLine.Tests `
 --unused-only `
 --verbosity n `
 --file-log "roslynator.log" `
 --file-log-verbosity diag

Write-Host "DONE"
