# `orang find`

Searches the file system for files and directories and optionally searches files' content\.

## Synopsis

```
orang find [<PATH>]
[   --ask]                <ASK_MODE>
[-a|--attributes]         <ATTRIBUTES>
[   --attributes-to-skip] <ATTRIBUTES>
[-c|--content]            <REGEX>
[-y|--display]            <DISPLAY_OPTIONS>
[   --encoding]           <ENCODING>
[-e|--extension]          <EXTENSION_FILTER>
[-h|--help]
[-t|--highlight]          <HIGHLIGHT>
[-i|--include-directory]  <REGEX>
[-l|--line-number]
[-m|--max-count]          <MAX_OPTIONS>
[-n|--name]               <REGEX>
[   --no-recurse]
[-o|--output]             <OUTPUT_OPTIONS>
[   --paths-from]         <FILE_PATH>
[   --progress]
[-v|--verbosity]          <VERBOSITY>
[   --file-log]           <FILE_LOG>
```

## Arguments

**`<PATH>`**

Path to one or more files and/or directories that should be searched\.

## Options

**`[--ask] <ASK_MODE>`**

Ask for continuation after each file or value\. Allowed values are \[f\]ile and \[v\]alue\.

**`[-a|--attributes] <ATTRIBUTES>`**

File attributes that are required\. Allowed values are archive, compressed, \[d\]irectory, \[e\]mpty, encrypted, \[f\]ile, \[h\]idden, normal, offline, \[r\]ead\-only, \[r\]eparse\-\[p\]oint, \[s\]ystem and temporary\.

**`[--attributes-to-skip] <ATTRIBUTES>`**

File attributes that should be skipped\. Allowed values are archive, compressed, \[e\]mpty, encrypted, \[h\]idden, normal, offline, \[r\]ead\-only, \[r\]eparse\-\[p\]oint, \[s\]ystem and temporary\.

**`[-c|--content] <REGEX>`**

Regular expression for files' content\. Syntax is \<PATTERN> \[\<PATTERN\_OPTIONS>\]\. Allowed values are compiled, \[c\]ulture\-\[i\]nvariant, \[e\]cma\-\[s\]cript, \[n\] explicit\-capture, \[f\]rom\-file, \[g\]roup=\<GROUP\_NAME>, \[i\]gnore\-case, \[x\] ignore\-pattern\-whitespace, \[li\]st, \[li\]st\-\[s\]eparator, \[l\]iteral, \[m\]ultiline, \[neg\]ative, \[r\]ight\-to\-left, \[s\]ingleline, timeout=\<NUM>, \[w\]hole\-\[i\]nput, \[w\]hole\-\[l\]ine and \[w\]hole\-word\.

**`[-y|--display] <DISPLAY_OPTIONS>`**

Display of the results\. Allowed values are \[c\]ontent=\<CONTENT\_DISPLAY> and \[p\]ath=\<PATH\_DISPLAY>\.

**`[--encoding] <ENCODING>`**

Encoding to use when a file does not contain byte order mark\. Default encoding is UTF\-8\.

**`[-e|--extension] <EXTENSION_FILTER>`**

A filter for file extensions\. Syntax is EXT1\[,EXT2,\.\.\.\] \[\<EXTENSION\_OPTIONS>\]\. Allowed values are \[c\]ulture\-\[i\]nvariant, \[f\]rom\-file, \[i\]gnore\-case, \[li\]st\-\[s\]eparator, \[l\]iteral, \[neg\]ative and timeout=\<NUM>\.

**`[-h|--help]`**

Show command line help\.

**`[-t|--highlight] <HIGHLIGHT>`**

Parts of the output to highlight\. Allowed values are \[n\]one, \[m\]atch, \[e\]mpty\-\[m\]atch, \[e\]mpty\-\[s\]plit, \[e\]mpty, \[b\]oundary, \[t\]ab, \[c\]arriage\-\[r\]eturn, \[l\]ine\[f\]eed, \[n\]ew\-\[l\]ine and space\.

**`[-i|--include-directory] <REGEX>`**

Regular expression for a directory name\. Syntax is \<PATTERN> \[\<PATTERN\_OPTIONS>\]\. Allowed values are compiled, \[c\]ulture\-\[i\]nvariant, \[e\]cma\-\[s\]cript, \[n\] explicit\-capture, \[f\]rom\-file, \[g\]roup=\<GROUP\_NAME>, \[i\]gnore\-case, \[x\] ignore\-pattern\-whitespace, \[li\]st, \[li\]st\-\[s\]eparator, \[l\]iteral, \[m\]ultiline, \[neg\]ative, \[p\]art=\<NAME\_PART>, \[r\]ight\-to\-left, \[s\]ingleline, timeout=\<NUM>, \[w\]hole\-\[i\]nput, \[w\]hole\-\[l\]ine and \[w\]hole\-word\.

**`[-l|--line-number]`**

Include line number\.

**`[-m|--max-count] <MAX_OPTIONS>`**

Stop searching after specified number is reached\. Allowed values are \[m\]atches=\<NUM>, \[m\]atches\-\[i\]n\-\[f\]ile and \[m\]atching\-\[f\]iles\.

**`[-n|--name] <REGEX>`**

Regular expression for file or directory name\. Syntax is \<PATTERN> \[\<PATTERN\_OPTIONS>\]\. Allowed values are compiled, \[c\]ulture\-\[i\]nvariant, \[e\]cma\-\[s\]cript, \[n\] explicit\-capture, \[f\]rom\-file, \[g\]roup=\<GROUP\_NAME>, \[i\]gnore\-case, \[x\] ignore\-pattern\-whitespace, \[li\]st, \[li\]st\-\[s\]eparator, \[l\]iteral, \[m\]ultiline, \[neg\]ative, \[p\]art=\<NAME\_PART>, \[r\]ight\-to\-left, \[s\]ingleline, timeout=\<NUM>, \[w\]hole\-\[i\]nput, \[w\]hole\-\[l\]ine and \[w\]hole\-word\.

**`[--no-recurse]`**

Do not search subdirectories\.

**`[-o|--output] <OUTPUT_OPTIONS>`**

Path to a file that should store results\. Syntax is \<PATH> \[\<OUTPUT\_OPTIONS>\]\. Allowed values are \[c\]ontent, \[p\]ath and \[e\]ncoding=\<ENCODING>\.

**`[--paths-from] <FILE_PATH>`**

Read the list of paths to search from a file\. Paths should be separated by newlines\.

**`[--progress]`**

Display dot \(\.\) for every tenth searched directory\.

**`[-v|--verbosity] <VERBOSITY>`**

The amount of information to display in the log\. Allowed values are \[q\]uiet, \[m\]inimal, \[n\]ormal, \[d\]etailed and \[diag\]nostic\.

**`[--file-log] <FILE_LOG>`**

Syntax is \<LOG\_PATH> \[\<LOG\_OPTIONS>\]\. Allowed values are \[v\]erbosity=\<VERBOSITY> and \[a\]ppend\.

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

*\(Generated with [DotMarkdown](http://github.com/JosefPihrt/DotMarkdown)\)*