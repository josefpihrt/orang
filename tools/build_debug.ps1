dotnet clean "../src/Orang.sln"
dotnet restore --force "../src/Orang.sln"
dotnet build "../src/Orang.sln" -c Debug /p:RunCodeAnalysis=false /m

Write-Host "DONE"
