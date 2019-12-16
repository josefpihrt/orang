# `orang match`

Searches the input string for occurrences of the regular expression\.

## Synopsis

```
orang match [<PATH>]
 -c|--content    <REGEX>
[-y|--display]   <DISPLAY_OPTIONS>
[-h|--help]
[-t|--highlight] <HIGHLIGHT>
[   --input]     <INPUT>
[-m|--max-count] <NUM>
[-o|--output]    <OUTPUT_OPTIONS>
[-v|--verbosity] <VERBOSITY>
```

## Arguments

**`<PATH>`**

Path to a file that should be analyzed\.

## Options

**`-c|--content <REGEX>`**

Regular expression for the input string\. Syntax is \<PATTERN> \[\<PATTERN\_OPTIONS>\]\. Allowed values are compiled, c\[ulture\-\]i\[nvariant\], e\[cma\-\]s\[cript\], n \[explicit\-capture\], f\[rom\-file\], g\[roup\]=\<GROUP\_NAME>, i\[gnore\-case\], x \[ignore\-pattern\-whitespace\], li\[st\], length=\<NUM>, l\[ist\-\]s\[eparator\], l\[iteral\], m\[ultiline\], r\[ight\-to\-left\], s\[ingleline\], timeout=\<NUM>, w\[hole\-\]i\[nput\], w\[hole\-\]l\[ine\] and w\[hole\-word\]\.

**`[-y|--display] <DISPLAY_OPTIONS>`**

Display of the results\. Allowed values are c\[ontent\]=\<CONTENT\_DISPLAY>, indent=\<INDENT>, separator=\<SEPARATOR> and summary\.

**`[-h|--help]`**

Show command line help\.

**`[-t|--highlight] <HIGHLIGHT>`**

Parts of the output to highlight\. Allowed values are n\[one\], m\[atch\], e\[mpty\-\]m\[atch\], e\[mpty\-\]s\[plit\], e\[mpty\], b\[oundary\], t\[ab\], c\[arriage\-\]r\[eturn\], l\[ine\]f\[eed\], n\[ew\-\]l\[ine\] and space\.

**`[--input] <INPUT>`**

Text to search\.

**`[-m|--max-count] <NUM>`**

Maximum number of matches returned\.

**`[-o|--output] <OUTPUT_OPTIONS>`**

Path to a file that should store output\. Syntax is \<PATH> \[\<OUTPUT\_OPTIONS>\]\. Allowed values are e\[ncoding\]=\<ENCODING>, v\[erbosity\]=\<VERBOSITY> and a\[ppend\]\.

**`[-v|--verbosity] <VERBOSITY>`**

The amount of information to display in the log\. Allowed values are q\[uiet\], m\[inimal\], n\[ormal\], d\[etailed\] and di\[agnostic\]\.

## Use Redirected Input

Piped output from a previous command will be treated as input.

*\(Generated with [DotMarkdown](http://github.com/JosefPihrt/DotMarkdown)\)*