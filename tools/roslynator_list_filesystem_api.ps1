dotnet new tool-manifest --force
dotnet tool install roslynator.dotnet.cli

dotnet roslynator list-symbols "../src/FileSystem/FileSystem.csproj" `
 --depth member `
 --visibility public `
 --empty-line-between-members `
 --ignored-parts containing-namespace assemblies assembly-attributes `
 --output orang_filesystem_api.txt `
 --verbosity d `
 --file-log "roslynator.log" `
 --file-log-verbosity diag

Write-Host "DONE"
