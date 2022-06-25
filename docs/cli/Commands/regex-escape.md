---
sidebar_label: regex escape
---

# `orang regex escape`

Escapes special characters by replacing them with their escape codes\.

## Synopsis

```
orang regex escape
    --char-group
-h, --help
-i, --input <INPUT> [<INPUT_OPTIONS>]
-o, --output <PATH> [<OUTPUT_OPTIONS>]
-r, --replacement
-v, --verbosity <VERBOSITY>
```

## Options

##### `--char-group`

Text is part of a character group\.

##### `-h, --help`

Show command line help\.

##### `-i, --input <INPUT> [<INPUT_OPTIONS>]`

Text to be escaped\.

##### `-o, --output <PATH> [<OUTPUT_OPTIONS>]`

Path to a file that should store output\.

##### `-r, --replacement`

Text is a replacement string\.

##### `-v, --verbosity <VERBOSITY>`

The amount of information to display in the log\.

[&lt;VERBOSITY&gt;](../OptionValues.md#verbosity): `q[uiet]`, `m[inimal]`, `n[ormal]`, `d[etailed]`, `di[agnostic]`\.

## Redirected/Piped Input

Redirected/piped input will be used as a text to be escaped.

## Samples

### Sample

Escape a given path so it will be treated as a literal string and not a regular expression.

#### Syntax

```
orang regex escape --input "foo\bar.txt"
```

#### Short Syntax

```
orang regex escape -i "foo\bar.txt"
```

*\(Generated with [DotMarkdown](http://github.com/JosefPihrt/DotMarkdown)\)*