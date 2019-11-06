# `orang match`

Searches the input string for occurrences of the regular expression\.

## Synopsis

```
orang match [<PATH>]
 -c|--content    <REGEX>
[-y|--display]   <DISPLAY_OPTIONS>
[-h|--highlight] <HIGHLIGHT>
[   --input]     <INPUT>
[-m|--max-count] <NUMBER>
[-o|--output]    <PATH>
[-v|--verbosity] <VERBOSITY>
[   --file-log]  <FILE_LOG>
```

## Arguments

**`<PATH>`**

Path to a file that should be analyzed\.

## Options

**`-c|--content <REGEX>`**

Regular expression for the input string\. Syntax is \<PATTERN> \[\<PATTERN\_OPTIONS>\]\. Allowed values are compiled, \[c\]ulture\-\[i\]nvariant, \[e\]cma\-\[s\]cript, \[n\] explicit\-capture, \[f\]rom\-file, \[g\]roup=\<GROUP\_NAME>, \[i\]gnore\-case, \[x\] ignore\-pattern\-whitespace, \[li\]st, \[li\]st\-\[s\]eparator, \[l\]iteral, \[m\]ultiline, \[r\]ight\-to\-left, \[s\]ingleline, timeout=\<NUM>, \[w\]hole\-\[i\]nput, \[w\]hole\-\[l\]ine and \[w\]hole\-word\.

**`[-y|--display] <DISPLAY_OPTIONS>`**

Display of the results\. Allowed values are \[c\]ontent=\<CONTENT\_DISPLAY> and \[p\]ath=\<PATH\_DISPLAY>\.

**`[-h|--highlight] <HIGHLIGHT>`**

Parts of the output to highlight\. Allowed values are \[n\]one, \[m\]atch, \[e\]mpty\-\[m\]atch, \[e\]mpty\-\[s\]plit, \[e\]mpty, \[b\]oundary, \[t\]ab, \[c\]arriage\-\[r\]eturn, \[l\]ine\[f\]eed, \[n\]ew\-\[l\]ine and space\.

**`[--input] <INPUT>`**

Text to search\.

**`[-m|--max-count] <NUMBER>`**

Maximum number of matches returned\.

**`[-o|--output] <PATH>`**

Path to a file that should store results\.

**`[-v|--verbosity] <VERBOSITY>`**

The amount of information to display in the log\. Allowed values are \[q\]uiet, \[m\]inimal, \[n\]ormal, \[d\]etailed and \[diag\]nostic\.

**`[--file-log] <FILE_LOG>`**

Syntax is \<LOG\_PATH> \[\<LOG\_OPTIONS>\]\. Allowed values are \[v\]erbosity=\<VERBOSITY> and \[a\]ppend\.

## Samples


*\(Generated with [DotMarkdown](http://github.com/JosefPihrt/DotMarkdown)\)*