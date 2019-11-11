### Sample

Find TODO comments in a C# source files in a current directory.

Current directory contains file **pattern.txt** with a following content:

```
(?im)
(?<=
  ^(\ |\t)*/{2,}\ *todo\ *
)
[^\r\n]+
```

#### Syntax

```
orang find ^
 --extension cs ^
 --content "pattern.txt" from-file
```

#### Short Syntax

```
orang find ^
 -e cs ^
 -c "pattern.txt" f
```

### Sample

Display content of new csproj files and pause after each file.

#### Syntax

```
orang find ^
 --extension csproj ^
 --content "<Project Sdk=\"Microsoft.NET.Sdk\">" literal ^
 --display content=all-lines ^
 --ask file
```

#### Short Syntax

```
orang find ^
 -e csproj ^
 -c "<Project Sdk=\"Microsoft.NET.Sdk\">" l ^
 -y c=a ^
 --ask f
```