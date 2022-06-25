---
sidebar_label: find
---

# `orang find`

Searches the file system for files and directories and optionally searches files' content\.

## Synopsis

```
orang find <PATH>
    --after-context <NUM>
    --align-columns
    --ask <ASK_MODE>
-a, --attributes <ATTRIBUTES>
-A, --attributes-to-skip <ATTRIBUTES>
    --before-context <NUM>
-c, --content <PATTERN> [<PATTERN_OPTIONS>]
    --content-mode <CONTENT_MODE>
    --context <NUM>
    --count
    --encoding <ENCODING>
-e, --extension EXT1[,EXT2] [<EXTENSION_OPTIONS>]
-h, --help
-t, --highlight <HIGHLIGHT>
-i, --include-directory <PATTERN> [<PATTERN_OPTIONS>]
    --line-number
-m, --max-count <NUM>
    --max-matches-in-file <NUM>
    --max-matching-files <NUM>
    --modify <MODIFY_OPTIONS>
-n, --name <PATTERN> [<PATTERN_OPTIONS>]
-C, --no-content
-P, --no-path
    --no-recurse
-o, --output <PATH> [<OUTPUT_OPTIONS>]
    --path-mode <PATH_MODE>
    --paths <PATH>
    --paths-from <FILE_PATH>
    --pipe <PIPE_MODE>
    --progress
-p, --properties <FILE_PROPERTIES>
-s, --sort <SORT_OPTIONS>
    --split
-u, --summary
-v, --verbosity <VERBOSITY>
```

## Arguments

**`<PATH>`**

Path to one or more files and/or directories that should be searched\.

## Options

##### `--after-context <NUM>`

Number of lines to show after matching line\.

##### `--align-columns`

Align columns\.

##### `--ask <ASK_MODE>`

Ask for permission after each file or value\.

[&lt;ASK_MODE&gt;](../OptionValues.md#ask_mode): `f[ile]`, `v[alue]`\.

##### `-a, --attributes <ATTRIBUTES>`

File attributes that are required\.

[&lt;ATTRIBUTES&gt;](../OptionValues.md#attributes): `archive`, `compressed`, `d[irectory]`, `e[mpty]`, `encrypted`, `f[ile]`, `h[idden]`, `normal`, `offline`, `r[ead-only]`, `r[eparse-]p[oint]`, `s[ystem]`, `temporary`\.

##### `-A, --attributes-to-skip <ATTRIBUTES>`

File attributes that should be skipped\.

[&lt;ATTRIBUTES&gt;](../OptionValues.md#attributes): `archive`, `compressed`, `e[mpty]`, `encrypted`, `h[idden]`, `normal`, `offline`, `r[ead-only]`, `r[eparse-]p[oint]`, `s[ystem]`, `temporary`\.

##### `--before-context <NUM>`

Number of lines to show before matching line\.

##### `-c, --content <PATTERN> [<PATTERN_OPTIONS>]`

Regular expression for files' content\.

[&lt;PATTERN_OPTIONS&gt;](../OptionValues.md#pattern_options): `compiled`, `c[ulture-]i[nvariant]`, `e[cma-]s[cript]`, `e[nds-]w[ith]`, `e[quals]`, `n [explicit-capture]`, `f[rom-file]`, `g[roup]=<GROUP_NAME>`, `i[gnore-case]`, `x [ignore-pattern-whitespace]`, `li[st]`, `length=<NUM>`, `l[ist-]s[eparator]`, `l[iteral]`, `m[ultiline]`, `ne[gative]`, `r[ight-to-left]`, `s[ingleline]`, `s[tarts-]w[ith]`, `timeout=<NUM>`, `w[hole-]l[ine]`, `w[hole-word]`\.

##### `--content-mode <CONTENT_MODE>`

Defines which parts of a content should be included in the results\.

[&lt;CONTENT_MODE&gt;](../OptionValues.md#content_mode): `a[ll-lines]`, `l[ine]`, `u[nmatched-lines]`, `v[alue]`, `d [value-detail]`, `o[mit]`\.

##### `--context <NUM>`

Number of lines to show before and after matching line\.

##### `--count`

Show number of matches in a file\.

##### `--encoding <ENCODING>`

Encoding to use when a file does not contain BOM\. Default encoding is UTF\-8\.

##### `-e, --extension EXT1[,EXT2] [<EXTENSION_OPTIONS>]`

A filter for file extensions \(case\-insensitive by default\)\.

[&lt;EXTENSION_OPTIONS&gt;](../OptionValues.md#extension_options): `c[ase-]s[ensitive]`, `c[ulture-]i[nvariant]`, `f[rom-file]`, `l[ist-]s[eparator]`, `l[iteral]`, `ne[gative]`, `timeout=<NUM>`\.

##### `-h, --help`

Show command line help\.

##### `-t, --highlight <HIGHLIGHT>`

Parts of the output to highlight\.

[&lt;HIGHLIGHT&gt;](../OptionValues.md#highlight): `n[one]`, `m[atch]`, `e[mpty-]m[atch]`, `e[mpty]`, `b[oundary]`, `t[ab]`, `c[arriage-]r[eturn]`, `l[ine]f[eed]`, `newline`, `space`\.

##### `-i, --include-directory <PATTERN> [<PATTERN_OPTIONS>]`

Regular expression for a directory name\.

##### `--line-number`

Include line number\.

##### `-m, --max-count <NUM>`

Stop searching after specified number is reached\.

##### `--max-matches-in-file <NUM>`

Stop searching in a file after specified number of matches is found\.

##### `--max-matching-files <NUM>`

Stop searching after specified number of files is found\.

##### `--modify <MODIFY_OPTIONS>`

Functions to modify results\.

[&lt;MODIFY_OPTIONS&gt;](../OptionValues.md#modify_options): `ag[gregate]`, `o [aggregate-only]`, `a[scending]`, `ci [culture-invariant]`, `d[escending]`, `di[stinct]`, `except`, `intersect`, `group`, `count`, `i[gnore-case]`, `re[move-empty]`, `rw [remove-white-space]`, `sort-by=`[&lt;SORT_PROPERTY&gt;](../OptionValues.md#sort_property), `tl [to-lower]`, `tu [to-upper]`, `t[rim]`, `te [trim-end]`, `ts [trim-start]`\.

##### `-n, --name <PATTERN> [<PATTERN_OPTIONS>]`

Regular expression for file or directory name\.

##### `-C, --no-content`

A shortcut for '\-\-content\-mode omit'\.

##### `-P, --no-path`

A shortcut for '\-\-path\-mode omit'\.

##### `--no-recurse`

Do not search subdirectories\.

##### `-o, --output <PATH> [<OUTPUT_OPTIONS>]`

Path to a file that should store output\.

##### `--path-mode <PATH_MODE>`

Defines which part of a path should be included in the results\.

[&lt;PATH_MODE&gt;](../OptionValues.md#path_mode): `f[ull]`, `r[elative]`, `m[atch]`, `o[mit]`\.

##### `--paths <PATH>`

Path to one or more files and/or directories that should be searched\.

##### `--paths-from <FILE_PATH>`

Read the list of paths to search from a file\. Paths should be separated by newlines\.

##### `--pipe <PIPE_MODE>`

Defines how to use redirected/piped input\.

[&lt;PIPE_MODE&gt;](../OptionValues.md#pipe_mode): `t[ext]`, `p[aths]`\.

##### `--progress`

Display dot \(\.\) for every hundredth searched file or directory\.

##### `-p, --properties <FILE_PROPERTIES>`

Display file's properties and optionally filter by that properties\.

[&lt;FILE_PROPERTIES&gt;](../OptionValues.md#file_properties): `c[reation-]t[ime][=<DATE>]`, `m[odified-]t[ime][=<DATE>]`, `s[ize][=<NUM>]`\.

##### `-s, --sort <SORT_OPTIONS>`

Sort matched files and directories\.

[&lt;SORT_OPTIONS&gt;](../OptionValues.md#sort_options): `a[scending]`, `c[reation-]t[ime]`, `ci [culture-invariant]`, `d[escending]`, `m[ax-count]=<NUM>`, `m[odified-]t[ime]`, `n[ame]`, `s[ize]`\.

##### `--split`

Execute regex in a split mode\.

##### `-u, --summary`

Show summary at the end of search\.

##### `-v, --verbosity <VERBOSITY>`

The amount of information to display in the log\.

[&lt;VERBOSITY&gt;](../OptionValues.md#verbosity): `q[uiet]`, `m[inimal]`, `n[ormal]`, `d[etailed]`, `di[agnostic]`\.

## Redirected/Piped Input

Redirected/piped input will be used either as a text to be searched (default) or as a list of paths separated with newlines (when `--pipe p[aths]` is specified.

## Samples

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

### Sample

Add copyright banner to each C# file in a current directory.

#### Syntax

```
orang find ^
 --extension cs ^
 --content "banner.txt" from-file literal starts-with negative | ^
orang replace ^
 --content "" starts-with ^
 --replacement "banner.txt" from-file
```

#### Short Syntax

```
orang find ^
 -e cs ^
 -c "banner.txt" f l sw ne | ^
orang replace ^
 -c "" sw ^
 -r "banner.txt" f
```

*\(Generated with [DotMarkdown](http://github.com/JosefPihrt/DotMarkdown)\)*