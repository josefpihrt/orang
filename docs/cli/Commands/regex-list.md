---
sidebar_label: regex list
---

# `orang regex list`

Lists regular expression patterns\.

## Synopsis

```
orang regex list <CHAR>
    --char-group
-f, --filter <PATTERN> [<PATTERN_OPTIONS>]
-h, --help
-o, --options <REGEX_OPTIONS>
-s, --section <SYNTAX_SECTIONS>
-v, --verbosity <VERBOSITY>
```

## Arguments

**`<CHAR>`**

Character or a decimal number that represents the character\. For a number literal use escape like \\1\.

## Options

##### `--char-group`

Treat character as if it is in the character group\.

##### `-f, --filter <PATTERN> [<PATTERN_OPTIONS>]`

Regular expression to filter patterns \(case\-insensitive by default\)\.

##### `-h, --help`

Show command line help\.

##### `-o, --options <REGEX_OPTIONS>`

Regex options that should be used\. Relevant values are ecma\-script or ignore\-case\.

[&lt;REGEX_OPTIONS&gt;](../OptionValues.md#regex_options): `c[ompiled]`, `c[ulture-]i[nvariant]`, `e[cma-]s[cript]`, `n [explicit-capture]`, `i[gnore-case]`, `x [ignore-pattern-whitespace]`, `m[ultiline]`, `r[ight-to-left]`, `s[ingleline]`\.

##### `-s, --section <SYNTAX_SECTIONS>`

Syntax sections to filter\.

[&lt;SYNTAX_SECTIONS&gt;](../OptionValues.md#syntax_sections): `a[lternation-]c[onstructs]`, `a[nchors]`, `b[ackreference-constructs]`, `c[haracter-classes]`, `c[haracter-]e[scapes]`, `g[eneral-]c[ategories]`, `g[rouping-constructs]`, `m[iscellaneous]`, `n[amed-blocks]`, `o[ptions]`, `r[egex-]o[ptions]`, `q[uantifiers]`, `s[ubstitutions]`, `all`\.

##### `-v, --verbosity <VERBOSITY>`

The amount of information to display in the log\.

[&lt;VERBOSITY&gt;](../OptionValues.md#verbosity): `q[uiet]`, `m[inimal]`, `n[ormal]`, `d[etailed]`, `di[agnostic]`\.

### Sample

Display regular expression anchors.

#### Syntax

```
orang regex list --section anchors
```

#### Short Syntax

```
orang regex list -s a
```

### Sample

Display general categories that matches filter string "num".

#### Syntax

```
orang regex list --section general-categories --filter num

```

#### Short Syntax

```
orang regex list -s gc -f num
```

### Sample

Display named blocks that matches filter string "han".

#### Syntax

```
orang regex list --section named-blocks --filter han

```

#### Short Syntax

```
orang regex list -s nb -f han
```

*\(Generated with [DotMarkdown](http://github.com/JosefPihrt/DotMarkdown)\)*