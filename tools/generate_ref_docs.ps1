dotnet new tool-manifest --force
dotnet tool install roslynator.dotnet.cli

dotnet roslynator generate-doc "../src/Orang.sln" `
 --properties "Configuration=Release" `
 --projects "Common" "FileSystem" `
 --heading ".NET API Reference" `
 -o "build/ref" `
 --host docusaurus `
 --group-by-common-namespace `
 --ignored-common-parts content `
 --max-derived-types 10 `
 --root-file-path "build/ref.md"

Write-Host "DONE"
