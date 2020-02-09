# `orang list-syntax`

Lists regular expression syntax\.

## Synopsis

```
orang list-syntax [<CHAR>]
[   --char-group]
[-f|--filter]     <FILTER>
[-o|--options]    <REGEX_OPTIONS>
[-s|--section]    <SYNTAX_SECTIONS>
```

## Arguments

**`<CHAR>`**

Character or a decimal number that represents the character\. For a number literal use escape like \\1\.

## Options

**`[--char-group]`**

Treat character as if it is in the character group\.

**`[-f|--filter] <FILTER>`**

Filter string that should be used to filter results\.

**`[-o|--options] <REGEX_OPTIONS>`**

Regex options that should be used\. Relevant values are \[e\]cma\-\[s\]cript or \[i\]gnore\-case\. Allowed values are c\[ompiled\], c\[ulture\]\-i\[nvariant\], e\[cma\]\-s\[cript\], n \[explicit\-capture\], i\[gnore\-case\], x \[ignore\-pattern\-whitespace\], m\[ultiline\], r\[ight\-to\-left\] and s\[ingleline\]\.

**`[-s|--section] <SYNTAX_SECTIONS>`**

Syntax sections to filter\. Allowed values are a\[lternation\-\]c\[onstructs\], a\[nchors\], b\[ackreference\-constructs\], c\[haracter\-classes\], c\[haracter\-\]e\[scapes\], g\[eneral\-\]c\[ategories\], g\[rouping\-constructs\], m\[iscellaneous\], n\[amed\-blocks\], o\[ptions\], q\[uantifiers\] and s\[ubstitutions\]\.

### Sample

Display regular expression anchors.

#### Syntax

```
orang list-syntax --section anchors
```

#### Short Syntax

```
orang list-syntax -s a
```

### Sample

Display general categories that matches filter string "num".

#### Syntax

```
orang list-syntax --section general-categories --filter num

```

#### Short Syntax

```
orang list-syntax -s gc -f num
```

### Sample

Display named blocks that matches filter string "han".

#### Syntax

```
orang list-syntax --section named-blocks --filter han

```

#### Short Syntax

```
orang list-syntax -s nb -f han
```

*\(Generated with [DotMarkdown](http://github.com/JosefPihrt/DotMarkdown)\)*