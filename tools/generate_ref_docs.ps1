$roslynatorExe="../../roslynator/src/CommandLine/bin/Debug/net7.0/Roslynator"

dotnet restore "../../Roslynator/src/CommandLine.sln" /p:Configuration=Debug -v m
dotnet build "../../Roslynator/src/CommandLine.sln" --no-restore /p:Configuration=Debug /v:m /m

& $roslynatorExe generate-doc "../src/FileSystem/FileSystem.csproj" `
 --properties "Configuration=Release" `
 --heading "Orang .NET API Reference" `
 -o "build/ref" `
 --host docusaurus `
 --group-by-common-namespace `
 --ignored-common-parts content `
 --ignored-root-parts all `
 --max-derived-types 10

& $roslynatorExe generate-doc-root "../src/FileSystem/FileSystem.csproj" `
 --properties "Configuration=Release" `
 -o "build/ref.md" `
 --host docusaurus `
 --heading "Orang .NET API Reference" `
 --ignored-parts content `
 --root-directory-url "ref"

Write-Host "DONE"
