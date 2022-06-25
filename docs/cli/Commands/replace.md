---
sidebar_label: replace
---

# `orang replace`

Searches the file system for files and replaces its content\.

## Synopsis

```
orang replace <PATH>
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
-d, --dry-run
    --encoding <ENCODING>
-e, --extension EXT1[,EXT2] [<EXTENSION_OPTIONS>]
-h, --help
-t, --highlight <HIGHLIGHT>
-i, --include-directory <PATTERN> [<PATTERN_OPTIONS>]
    --input <INPUT> [<INPUT_OPTIONS>]
    --interactive
    --line-number
-m, --max-count <NUM>
    --max-matches-in-file <NUM>
    --max-matching-files <NUM>
    --modify <REPLACE_MODIFY>
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
-r, --replacement <REPLACEMENT> [<REPLACEMENT_OPTIONS>]
-s, --sort <SORT_OPTIONS>
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

[&lt;PATTERN_OPTIONS&gt;](../OptionValues.md#pattern_options): `compiled`, `c[ulture-]i[nvariant]`, `e[cma-]s[cript]`, `e[nds-]w[ith]`, `e[quals]`, `n [explicit-capture]`, `f[rom-file]`, `i[gnore-case]`, `x [ignore-pattern-whitespace]`, `li[st]`, `length=<NUM>`, `l[ist-]s[eparator]`, `l[iteral]`, `m[ultiline]`, `r[ight-to-left]`, `s[ingleline]`, `s[tarts-]w[ith]`, `timeout=<NUM>`, `w[hole-]l[ine]`, `w[hole-word]`\.

##### `--content-mode <CONTENT_MODE>`

Defines which parts of a content should be included in the results\.

[&lt;CONTENT_MODE&gt;](../OptionValues.md#content_mode): `a[ll-lines]`, `l[ine]`, `u[nmatched-lines]`, `v[alue]`, `d [value-detail]`, `o[mit]`\.

##### `--context <NUM>`

Number of lines to show before and after matching line\.

##### `--count`

Show number of matches in a file\.

##### `-d, --dry-run`

Display which files should be updated but do not actually update any file\.

##### `--encoding <ENCODING>`

Encoding to use when a file does not contain BOM\. Default encoding is UTF\-8\.

##### `-e, --extension EXT1[,EXT2] [<EXTENSION_OPTIONS>]`

A filter for file extensions \(case\-insensitive by default\)\.

[&lt;EXTENSION_OPTIONS&gt;](../OptionValues.md#extension_options): `c[ase-]s[ensitive]`, `c[ulture-]i[nvariant]`, `f[rom-file]`, `l[ist-]s[eparator]`, `l[iteral]`, `ne[gative]`, `timeout=<NUM>`\.

##### `-h, --help`

Show command line help\.

##### `-t, --highlight <HIGHLIGHT>`

Parts of the output to highlight\.

[&lt;HIGHLIGHT&gt;](../OptionValues.md#highlight): `n[one]`, `m[atch]`, `r[eplacement]`, `e[mpty-]m[atch]`, `e[mpty-]r[eplacement]`, `e[mpty]`, `b[oundary]`, `t[ab]`, `c[arriage-]r[eturn]`, `l[ine]f[eed]`, `newline`, `space`\.

##### `-i, --include-directory <PATTERN> [<PATTERN_OPTIONS>]`

Regular expression for a directory name\.

##### `--input <INPUT> [<INPUT_OPTIONS>]`

The input string to be searched\.

##### `--interactive`

Enable editing of a replacement\.

##### `--line-number`

Include line number\.

##### `-m, --max-count <NUM>`

Stop searching after specified number is reached\.

##### `--max-matches-in-file <NUM>`

Stop searching in a file after specified number of matches is found\.

##### `--max-matching-files <NUM>`

Stop searching after specified number of files is found\.

##### `--modify <REPLACE_MODIFY>`

Functions to modify result\.

[&lt;REPLACE_MODIFY&gt;](../OptionValues.md#replace_modify): `ci [culture-invariant]`, `tl [to-lower]`, `tu [to-upper]`, `t[rim]`, `te [trim-end]`, `ts [trim-start]`\.

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

##### `-r, --replacement <REPLACEMENT> [<REPLACEMENT_OPTIONS>]`

Replacement pattern\.

##### `-s, --sort <SORT_OPTIONS>`

Sort matched files and directories\.

[&lt;SORT_OPTIONS&gt;](../OptionValues.md#sort_options): `a[scending]`, `c[reation-]t[ime]`, `ci [culture-invariant]`, `d[escending]`, `m[ax-count]=<NUM>`, `m[odified-]t[ime]`, `n[ame]`, `s[ize]`\.

##### `-u, --summary`

Show summary at the end of search\.

##### `-v, --verbosity <VERBOSITY>`

The amount of information to display in the log\.

[&lt;VERBOSITY&gt;](../OptionValues.md#verbosity): `q[uiet]`, `m[inimal]`, `n[ormal]`, `d[etailed]`, `di[agnostic]`\.

## Redirected/Piped Input

Redirected/piped input will be used either as a text to be searched (default) or as a list of paths separated with newlines (when `--pipe p[aths]` is specified.

## Samples

### Sample

Update version in csproj and vbproj files in a current directory.

Current directory contains file **pattern.txt** with a following content:

```
(?mx)
(?<=
  ^\ *\<Version\>
)
\d+\.\d+\.\d+\.\d+
(?=
  \</Version\>
)
```

#### Syntax

```
orang replace ^
 --extension csproj,vbproj ^
 --content "pattern.txt" from-file ^
 --replacement "1.2.3.0" ^
 --highlight match replacement
```

#### Short Syntax

```
orang replace ^
 -e csproj,vbproj ^
 -c "pattern.txt" f ^
 -r "1.2.3.0" ^
 -t m r
```

### Sample

Remove duplicate words in C# comments from source files in a current directory.

Current directory contains file **pattern.txt** with a following content:

```
(?mx)
(?<=
  ^
  (\ |\t)*
  //[^\r\n]*\b(?<g>\w+)\b
)
\ +
\b\k<g>\b
```

#### Syntax

```
orang replace ^
 --extension cs ^
 --content "pattern.txt" ^
 --include-directory ".git" equals negative ^
 --highlight match
```

#### Short Syntax

```
orang replace ^
 -e cs ^
 -c "pattern.txt" ^
 -i ".git" e e ^
 -t m

```

### Sample

Normalize newline to CR+LF for all files in a current directory.

#### Syntax

```
orang replace ^
 --content "(?<!\r)\n" ^
 --replacement "\r\n" escape ^
 --verbosity minimal
```

#### Short Syntax

```
orang replace ^
 -c "(?<!\r)\n" ^
 -r "\r\n" e ^
 -v m
```

*\(Generated with [DotMarkdown](http://github.com/JosefPihrt/DotMarkdown)\)*