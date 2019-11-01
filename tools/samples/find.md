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
