# `orang rename`

Renames files and directories\.

## Synopsis

```
orang rename [<PATH>]
[   --ask]
[-a|--attributes]         <ATTRIBUTES>
[   --attributes-to-skip] <ATTRIBUTES>
[-c|--content]            <REGEX>
[-y|--display]            <DISPLAY_OPTIONS>
[-d|--dry-run]
[   --encoding]           <ENCODING>
[   --evaluator]          <EVALUATOR>
[-e|--extension]          <EXTENSION_FILTER>
[-h|--help]
[-t|--highlight]          <HIGHLIGHT>
[-i|--include-directory]  <REGEX>
[-m|--max-count]          <NUMBER>
 -n|--name                <REGEX>
[   --no-recurse]
[-o|--output]             <PATH>
[   --progress]
[-r|--replacement]        <REPLACEMENT>
[-v|--verbosity]          <VERBOSITY>
[   --file-log]           <FILE_LOG>
```

## Arguments

**`<PATH>`**

Path to one or more files and/or directories that should be searched\.

## Options

**`[--ask]`**

Ask for a permission to rename file or directory\.

**`[-a|--attributes] <ATTRIBUTES>`**

File attributes that are required\. Allowed values are archive, compressed, \[d\]irectory, \[e\]mpty, encrypted, \[f\]ile, \[h\]idden, normal, offline, \[r\]ead\-only, \[s\]ystem and temporary\.

**`[--attributes-to-skip] <ATTRIBUTES>`**

File attributes that should be skipped\. Allowed values are archive, compressed, \[e\]mpty, encrypted, \[h\]idden, normal, offline, \[r\]ead\-only, \[s\]ystem and temporary\.

**`[-c|--content] <REGEX>`**

Regular expression for files' content\. Syntax is \<PATTERN> \[\<PATTERN\_OPTIONS>\]\. Allowed values are compiled, \[c\]ulture\-\[i\]nvariant, \[e\]cma\-\[s\]cript, \[n\] explicit\-capture, \[f\]rom\-file, \[g\]roup=\<GROUP\_NAME>, \[i\]gnore\-case, \[x\] ignore\-pattern\-whitespace, \[li\]st, \[li\]st\-\[s\]eparator, \[l\]iteral, \[m\]ultiline, \[neg\]ative, \[r\]ight\-to\-left, \[s\]ingleline, timeout=\<NUM>, \[w\]hole\-\[i\]nput, \[w\]hole\-\[l\]ine and \[w\]hole\-word\.

**`[-y|--display] <DISPLAY_OPTIONS>`**

Display of the results\. Allowed values are \[c\]ontent=\<CONTENT\_DISPLAY> and \[p\]ath=\<PATH\_DISPLAY>\.

**`[-d|--dry-run]`**

Display which files or directories should be renamed but do not actually rename any file or directory\.

**`[--encoding] <ENCODING>`**

Encoding to use when a file does not contain byte order mark\. Default encoding is UTF\-8\.

**`[--evaluator] <EVALUATOR>`**

Path to the evaluator method to compute replacements\. The format is "LibraryPath,FullTypeName\.MethodName"\.

**`[-e|--extension] <EXTENSION_FILTER>`**

A filter for file extensions\. Syntax is EXT1\[,EXT2,\.\.\.\] \[\<EXTENSION\_OPTIONS>\]\. Allowed values are \[c\]ulture\-\[i\]nvariant, \[f\]rom\-file, \[i\]gnore\-case, \[li\]st\-\[s\]eparator, \[l\]iteral, \[neg\]ative and timeout=\<NUM>\.

**`[-h|--help]`**

Show command line help\.

**`[-t|--highlight] <HIGHLIGHT>`**

Parts of the output to highlight\. Allowed values are \[n\]one, \[m\]atch, \[r\]eplacement, \[e\]mpty\-\[s\]plit and \[e\]mpty\.

**`[-i|--include-directory] <REGEX>`**

Regular expression for a directory name\. Syntax is \<PATTERN> \[\<PATTERN\_OPTIONS>\]\. Allowed values are compiled, \[c\]ulture\-\[i\]nvariant, \[e\]cma\-\[s\]cript, \[n\] explicit\-capture, \[f\]rom\-file, \[g\]roup=\<GROUP\_NAME>, \[i\]gnore\-case, \[x\] ignore\-pattern\-whitespace, \[li\]st, \[li\]st\-\[s\]eparator, \[l\]iteral, \[m\]ultiline, \[neg\]ative, \[p\]art=\<NAME\_PART>, \[r\]ight\-to\-left, \[s\]ingleline, timeout=\<NUM>, \[w\]hole\-\[i\]nput, \[w\]hole\-\[l\]ine and \[w\]hole\-word\.

**`[-m|--max-count] <NUMBER>`**

Stop deleting after specified number is reached\.

**`-n|--name <REGEX>`**

Regular expression for file or directory name\. Syntax is \<PATTERN> \[\<PATTERN\_OPTIONS>\]\. Allowed values are compiled, \[c\]ulture\-\[i\]nvariant, \[e\]cma\-\[s\]cript, \[n\] explicit\-capture, \[f\]rom\-file, \[i\]gnore\-case, \[x\] ignore\-pattern\-whitespace, \[li\]st, \[li\]st\-\[s\]eparator, \[l\]iteral, \[m\]ultiline, \[p\]art=\<NAME\_PART>, \[r\]ight\-to\-left, \[s\]ingleline, timeout=\<NUM>, \[w\]hole\-\[i\]nput, \[w\]hole\-\[l\]ine and \[w\]hole\-word\.

**`[--no-recurse]`**

Do not search subdirectories\.

**`[-o|--output] <PATH>`**

Path to a file that should store results\.

**`[--progress]`**

Display dot \(\.\) for every tenth searched directory\.

**`[-r|--replacement] <REPLACEMENT>`**

Replacement pattern\. Syntax is \<REPLACEMENT> \[\<REPLACEMENT\_OPTIONS>\]\. Allowed values are \[f\]rom\-file, \[l\]iteral and \[m\]ultiline\.

**`[-v|--verbosity] <VERBOSITY>`**

The amount of information to display in the log\. Allowed values are \[q\]uiet, \[m\]inimal, \[n\]ormal, \[d\]etailed and \[diag\]nostic\.

**`[--file-log] <FILE_LOG>`**

Syntax is \<LOG\_PATH> \[\<LOG\_OPTIONS>\]\. Allowed values are \[v\]erbosity=\<VERBOSITY> and \[a\]ppend\.

## Samples

### Sample

Remove suffix "-foo" from all files in a current directory.

#### Syntax

```
orang rename ^
 --name "\-foo$" part=name-without-extension ^
 --replacement ""
```

#### Short Syntax

```
orang rename ^
 -n "\-foo$" p=w ^
 -r ""
```

Note: Syntax `--replacement ""` or `-r ""` can be omitted.
*\(Generated with [DotMarkdown](http://github.com/JosefPihrt/DotMarkdown)\)*