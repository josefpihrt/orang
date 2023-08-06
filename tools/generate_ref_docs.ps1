roslynator generate-doc "../src/Orang.sln" `
 --properties "Configuration=Release" `
 --projects "Common" "FileSystem" `
 --heading ".NET API Reference" `
 -o "build/ref" `
 --host docusaurus `
 --group-by-common-namespace `
 --ignored-common-parts content `
 --ignored-root-parts all `
 --max-derived-types 10

roslynator generate-doc-root "../src/Orang.sln" `
 --properties "Configuration=Release" `
 --projects "Common" "FileSystem" `
 -o "build/ref.md" `
 --host docusaurus `
 --heading ".NET API Reference" `
 --ignored-parts content `
 --root-directory-url "ref"

Write-Host "DONE"
