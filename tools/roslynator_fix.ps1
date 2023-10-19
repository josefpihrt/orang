dotnet new tool-manifest --force
dotnet tool install roslynator.dotnet.cli

dotnet roslynator fix "../src/Orang.sln" `
 --format `
 --verbosity d `
 --file-log "roslynator.log" `
 --file-log-verbosity diag `
 --diagnostic-fix-map "RCS1155=Roslynator.RCS1155.OrdinalIgnoreCase" `
 --file-banner " Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information."

Write-Host "DONE"
