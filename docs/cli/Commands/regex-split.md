---
sidebar_label: regex split
---

# `orang regex split`

Splits the input string into an list of substrings at the positions defined by a regular expression\.

## Synopsis

```
orang regex split <PATH>
-c, --content <PATTERN> [<PATTERN_OPTIONS>]
    --content-mode <CONTENT_MODE>
-h, --help
-t, --highlight <HIGHLIGHT>
-i, --input <INPUT> [<INPUT_OPTIONS>]
-m, --max-count <NUM>
    --modify <MODIFY_OPTIONS>
    --no-groups
-o, --output <PATH> [<OUTPUT_OPTIONS>]
-u, --summary
-v, --verbosity <VERBOSITY>
```

## Arguments

**`<PATH>`**

Path to a file that should be analyzed\.

## Options

##### `-c, --content <PATTERN> [<PATTERN_OPTIONS>]`

Regular expression for the input string\.

[&lt;PATTERN_OPTIONS&gt;](../OptionValues.md#pattern_options): `compiled`, `c[ulture-]i[nvariant]`, `e[cma-]s[cript]`, `e[nds-]w[ith]`, `e[quals]`, `n [explicit-capture]`, `f[rom-file]`, `i[gnore-case]`, `x [ignore-pattern-whitespace]`, `li[st]`, `length=<NUM>`, `l[ist-]s[eparator]`, `l[iteral]`, `m[ultiline]`, `r[ight-to-left]`, `s[ingleline]`, `s[tarts-]w[ith]`, `timeout=<NUM>`, `w[hole-]l[ine]`, `w[hole-word]`\.

##### `--content-mode <CONTENT_MODE>`

Defines which parts of a content should be included in the results\.

[&lt;CONTENT_MODE&gt;](../OptionValues.md#content_mode): `a[ll-lines]`, `l[ine]`, `u[nmatched-lines]`, `v[alue]`, `d [value-detail]`, `o[mit]`\.

##### `-h, --help`

Show command line help\.

##### `-t, --highlight <HIGHLIGHT>`

Parts of the output to highlight\.

[&lt;HIGHLIGHT&gt;](../OptionValues.md#highlight): `n[one]`, `s[plit]`, `e[mpty-]s[plit]`, `e[mpty]`, `b[oundary]`, `t[ab]`, `c[arriage-]r[eturn]`, `l[ine]f[eed]`, `newline`, `space`\.

##### `-i, --input <INPUT> [<INPUT_OPTIONS>]`

The input string to be searched\.

##### `-m, --max-count <NUM>`

Maximum number of times the split can occur\.

##### `--modify <MODIFY_OPTIONS>`

Functions to modify results\.

[&lt;MODIFY_OPTIONS&gt;](../OptionValues.md#modify_options): `ag[gregate]`, `o [aggregate-only]`, `a[scending]`, `ci [culture-invariant]`, `d[escending]`, `di[stinct]`, `except`, `intersect`, `group`, `count`, `i[gnore-case]`, `re[move-empty]`, `rw [remove-white-space]`, `sort-by=`[&lt;SORT_PROPERTY&gt;](../OptionValues.md#sort_property), `tl [to-lower]`, `tu [to-upper]`, `t[rim]`, `te [trim-end]`, `ts [trim-start]`\.

##### `--no-groups`

Do not include groups in the results\.

##### `-o, --output <PATH> [<OUTPUT_OPTIONS>]`

Path to a file that should store output\.

##### `-u, --summary`

Show summary at the end of search\.

##### `-v, --verbosity <VERBOSITY>`

The amount of information to display in the log\.

[&lt;VERBOSITY&gt;](../OptionValues.md#verbosity): `q[uiet]`, `m[inimal]`, `n[ormal]`, `d[etailed]`, `di[agnostic]`\.

## Redirected/Piped Input

Redirected/piped input will be used as a text to be searched.

*\(Generated with [DotMarkdown](http://github.com/JosefPihrt/DotMarkdown)\)*