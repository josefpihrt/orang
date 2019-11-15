# `orang escape`

Escapes special characters by replacing them with their escape codes\.

## Synopsis

```
orang escape
[   --char-group]
[-i|--input]       <INPUT>
[-r|--replacement]
```

## Options

**`[--char-group]`**

Text is part of a character group\.

**`[-i|--input] <INPUT>`**

Text to be escaped\.

**`[-r|--replacement]`**

Text is a replacement string\.

## Samples

### Sample

Escape a given path so it will be treated as a literal string and not a regular expression.

#### Syntax

```
orang escape --input "foo\bar.txt"
```

#### Short Syntax

```
orang escape -i "foo\bar.txt"
```
*\(Generated with [DotMarkdown](http://github.com/JosefPihrt/DotMarkdown)\)*