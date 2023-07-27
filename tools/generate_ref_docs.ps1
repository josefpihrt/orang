$roslynatorExe="../../roslynator/src/CommandLine/bin/Debug/net7.0/Roslynator"

dotnet restore "../../Roslynator/src/CommandLine.sln" /p:Configuration=Debug -v m
dotnet build "../../Roslynator/src/CommandLine.sln" --no-restore /p:Configuration=Debug /v:m /m

& $roslynatorExe generate-doc "../src/Orang.sln" `
 --properties "Configuration=Release" `
 --projects "Core" "FileSystem" `
 --heading ".NET API Reference" `
 -o "build/ref" `
 --host docusaurus `
 --group-by-common-namespace `
 --ignored-common-parts content `
 --ignored-root-parts all `
 --max-derived-types 10

& $roslynatorExe generate-doc-root "../src/Orang.sln" `
 --properties "Configuration=Release" `
 --projects "Core" "FileSystem" `
 -o "build/ref.md" `
 --host docusaurus `
 --heading ".NET API Reference" `
 --ignored-parts content `
 --root-directory-url "ref"

Write-Host "DONE"
