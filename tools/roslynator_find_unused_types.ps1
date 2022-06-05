#dotnet tool install -g roslynator.dotnet.cli

roslynator find-symbols "../src/Orang.sln" `
 --visibility public internal private `
 --symbol-groups type `
 --without-attributes `
  "System.ObsoleteAttribute" `
 --ignored-projects CommandLine.Tests `
 --unused-only `
 --verbosity n `
 --file-log "roslynator.log" `
 --file-log-verbosity diag

Write-Host "DONE"
Read-Host
 