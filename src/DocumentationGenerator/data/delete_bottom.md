## Use Redirected Input

Piped output from a previous command will be treated as a list of paths separated by newlines.

## Samples

### Sample

Delete empty directories in a current directory.

#### Syntax

```
orang delete ^
 --name "^(?=)" ^
 --attributes directory empty
```

#### Short Syntax

```
orang delete ^
 -n "^(?=)" ^
 -a d e
```

### Sample

Delete content of "bin" or "obj" directories in a current directory.

#### Syntax

```
orang delete ^
 --name "bin|obj" whole-input ^
 --attributes directory ^
 --content-only
```

#### Short Syntax

```
orang delete ^
 -n "bin|obj" wi ^
 -a d ^
 --content-only
```
