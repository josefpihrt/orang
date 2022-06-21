#dotnet tool install -g roslynator.dotnet.cli

roslynator list-symbols "../src/FileSystem/FileSystem.csproj" `
 --depth type `
 --visibility public `
 --layout type-hierarchy `
 --ignored-parts assemblies containing-namespace-in-type-hierarchy assembly-attributes accessibility base-type constraints `
 --output orang_filesystem_api_hierarchy.txt `
 --verbosity d `
 --file-log "roslynator.log" `
 --file-log-verbosity diag

Write-Host "DONE"
Read-Host
