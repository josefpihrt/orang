@echo off

rem This script requires Orang to be installed. Run command below to install Orang.
rem dotnet tool install orang.dotnet.cli

set _tempFile=josefpihrt-vscode.roslynator.version.tmp

orang find "%USERPROFILE%/.vscode/extensions" -a d -n "^josefpihrt-vscode\.roslynator-\d+\.\d+\.\d+" | ^
orang match -c "\d+\.\d+\.\d+" r -m 1 > %_tempFile%

set /p _version=<%_tempFile%

orang replace "%USERPROFILE%/.omnisharp/omnisharp.json" ^
 -c "(?<=josefpihrt-vscode\.roslynator-)\d+\.\d+\.\d+" -r "%_version%" -t m r -v d

orang delete %_tempFile% -n "" -v q

pause