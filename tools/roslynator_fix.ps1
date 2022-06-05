#dotnet tool install -g roslynator.dotnet.cli

$roslynatorPath=../../Roslynator/src

roslynator fix "../src/Orang.sln" `
 --analyzer-assemblies `
  "$roslynatorPath/Analyzers.CodeFixes/bin/Debug/netstandard2.0/Roslynator.CSharp.Analyzers.dll" `
  "$roslynatorPath/Analyzers.CodeFixes/bin/Debug/netstandard2.0/Roslynator.CSharp.Analyzers.CodeFixes.dll" `
  "$roslynatorPath/Formatting.Analyzers.CodeFixes/bin/Debug/netstandard2.0/Roslynator.Formatting.Analyzers.dll" `
  "$roslynatorPath/Formatting.Analyzers.CodeFixes/bin/Debug/netstandard2.0/Roslynator.Formatting.Analyzers.CodeFixes.dll" `
 --format `
 --verbosity d `
 --file-log "roslynator.log" `
 --file-log-verbosity diag `
 --diagnostic-fix-map "RCS1155=Roslynator.RCS1155.OrdinalIgnoreCase" `
 --file-banner " Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information."

 Write-Host "DONE"
 Read-Host
 