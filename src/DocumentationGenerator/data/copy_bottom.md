## Use Redirected Input

Piped output from a previous command will be treated as a list of paths separated by newlines.

## Samples

### Sample

Backup Visual Studio project but exclude directories that are not necessary.

#### Syntax

```
orang copy MyProject ^
 --target "backup/MyProject" ^
 --conflict overwrite ^
 --compare attributes size content ^
 --include-directory "bin,obj,packages,node_modules,.vs" literal list equals negative
```

#### Short Syntax

```
orang copy MyProject ^
 --target "backup/MyProject" ^
 --conflict o ^
 --compare a s c ^
 -i "bin,obj,packages,node_modules,.vs" l li e ne
```
