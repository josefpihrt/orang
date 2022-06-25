---
sidebar_label: sync
---

# `orang sync`

Synchronizes content of two directories\.

## Synopsis

```
orang sync <PATH>
    --after-context <NUM>
    --align-columns
    --ask
-a, --attributes <ATTRIBUTES>
-A, --attributes-to-skip <ATTRIBUTES>
    --before-context <NUM>
    --compare <COMPARE_OPTIONS>
    --conflict <SYNC_CONFLICT_RESOLUTION>
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
    --line-number
-m, --max-count <NUM>
    --max-matches-in-file <NUM>
    --max-matching-files <NUM>
-n, --name <PATTERN> [<PATTERN_OPTIONS>]
-C, --no-content
-P, --no-path
    --no-recurse
-o, --output <PATH> [<OUTPUT_OPTIONS>]
    --path-mode <PATH_MODE>
    --paths <PATH>
    --paths-from <FILE_PATH>
    --progress
-p, --properties <FILE_PROPERTIES>
    --second <DIRECTORY_PATH>
-s, --sort <SORT_OPTIONS>
-u, --summary
-v, --verbosity <VERBOSITY>
```

## Arguments

**`<PATH>`**

Path to a first directory to be synchronized and optionally a second directory\.

## Options

##### `--after-context <NUM>`

Number of lines to show after matching line\.

##### `--align-columns`

Align columns\.

##### `--ask`

Ask for a permission to synchronize file or directory\.

##### `-a, --attributes <ATTRIBUTES>`

File attributes that are required\.

[&lt;ATTRIBUTES&gt;](../OptionValues.md#attributes): `archive`, `compressed`, `d[irectory]`, `e[mpty]`, `encrypted`, `f[ile]`, `h[idden]`, `normal`, `offline`, `r[ead-only]`, `r[eparse-]p[oint]`, `s[ystem]`, `temporary`\.

##### `-A, --attributes-to-skip <ATTRIBUTES>`

File attributes that should be skipped\.

[&lt;ATTRIBUTES&gt;](../OptionValues.md#attributes): `archive`, `compressed`, `e[mpty]`, `encrypted`, `h[idden]`, `normal`, `offline`, `r[ead-only]`, `r[eparse-]p[oint]`, `s[ystem]`, `temporary`\.

##### `--before-context <NUM>`

Number of lines to show before matching line\.

##### `--compare <COMPARE_OPTIONS>`

File properties to be compared\.

[&lt;COMPARE_OPTIONS&gt;](../OptionValues.md#compare_options): `n[one]`, `a[ttributes]`, `c[ontent]`, `m[odified-]t[ime]`, `s[ize]`\.

##### `--conflict <SYNC_CONFLICT_RESOLUTION>`

Action to choose if a file or directory exists in one directory and it is missing in the second directory\.

[&lt;SYNC_CONFLICT_RESOLUTION&gt;](../OptionValues.md#sync_conflict_resolution): `a[sk]`, `f[irst-wins]`, `s[econd-wins]`\.

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

##### `-d, --dry-run`

Display which files or directories should be copied/deleted but do not actually copy/delete any file or directory\.

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

##### `--progress`

Display dot \(\.\) for every hundredth searched file or directory\.

##### `-p, --properties <FILE_PROPERTIES>`

Display file's properties and optionally filter by that properties\.

[&lt;FILE_PROPERTIES&gt;](../OptionValues.md#file_properties): `c[reation-]t[ime][=<DATE>]`, `m[odified-]t[ime][=<DATE>]`, `s[ize][=<NUM>]`\.

##### `--second <DIRECTORY_PATH>`

A directory to be synchronized with a first directory\. It can be also specified as a last unnamed parameter\.

##### `-s, --sort <SORT_OPTIONS>`

Sort matched files and directories\.

[&lt;SORT_OPTIONS&gt;](../OptionValues.md#sort_options): `a[scending]`, `c[reation-]t[ime]`, `ci [culture-invariant]`, `d[escending]`, `m[ax-count]=<NUM>`, `m[odified-]t[ime]`, `n[ame]`, `s[ize]`\.

##### `-u, --summary`

Show summary at the end of search\.

##### `-v, --verbosity <VERBOSITY>`

The amount of information to display in the log\.

[&lt;VERBOSITY&gt;](../OptionValues.md#verbosity): `q[uiet]`, `m[inimal]`, `n[ormal]`, `d[etailed]`, `di[agnostic]`\.


*\(Generated with [DotMarkdown](http://github.com/JosefPihrt/DotMarkdown)\)*