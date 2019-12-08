# `orang replace`

Searches the file system for files and replaces its content\.

## Synopsis

```
orang replace [<PATH>]
[   --ask]                <ASK_MODE>
[-a|--attributes]         <ATTRIBUTES>
[   --attributes-to-skip] <ATTRIBUTES>
 -c|--content             <REGEX>
[-y|--display]            <DISPLAY_OPTIONS>
[-d|--dry-run]
[   --encoding]           <ENCODING>
[   --evaluator]          <EVALUATOR>
[-e|--extension]          <EXTENSION_FILTER>
[-h|--help]
[-t|--highlight]          <HIGHLIGHT>
[-i|--include-directory]  <REGEX>
[   --input]              <INPUT>
[-m|--max-count]          <MAX_OPTIONS>
[-n|--name]               <REGEX>
[   --no-recurse]
[   --paths-from]         <FILE_PATH>
[   --progress]
[-p|--properties]         <FILE_PROPERTIES>
[-r|--replacement]        <REPLACEMENT>
[-s|--sort]               <SORT_OPTIONS>
[-v|--verbosity]          <VERBOSITY>
[   --file-log]           <FILE_LOG>
```

## Arguments

**`<PATH>`**

Path to one or more files and/or directories that should be searched\.

## Options

**`[--ask] <ASK_MODE>`**

Ask for permission after each file or value\. Allowed values are \[f\]ile and \[v\]alue\.

**`[-a|--attributes] <ATTRIBUTES>`**

File attributes that are required\. Allowed values are archive, compressed, \[d\]irectory, \[e\]mpty, encrypted, \[f\]ile, \[h\]idden, normal, offline, \[r\]ead\-only, \[r\]eparse\-\[p\]oint, \[s\]ystem and temporary\.

**`[--attributes-to-skip] <ATTRIBUTES>`**

File attributes that should be skipped\. Allowed values are archive, compressed, \[e\]mpty, encrypted, \[h\]idden, normal, offline, \[r\]ead\-only, \[r\]eparse\-\[p\]oint, \[s\]ystem and temporary\.

**`-c|--content <REGEX>`**

Regular expression for files' content\. Syntax is \<PATTERN> \[\<PATTERN\_OPTIONS>\]\. Allowed values are compiled, \[c\]ulture\-\[i\]nvariant, \[e\]cma\-\[s\]cript, \[n\] explicit\-capture, \[f\]rom\-file, \[i\]gnore\-case, \[x\] ignore\-pattern\-whitespace, \[li\]st, length=\<NUM>, \[li\]st\-\[s\]eparator, \[l\]iteral, \[m\]ultiline, \[r\]ight\-to\-left, \[s\]ingleline, timeout=\<NUM>, \[w\]hole\-\[i\]nput, \[w\]hole\-\[l\]ine and \[w\]hole\-word\.

**`[-y|--display] <DISPLAY_OPTIONS>`**

Display of the results\. Allowed values are \[c\]ontent=\<CONTENT\_DISPLAY>, count, \[c\]reation\-time, indent=\<INDENT>, \[l\]ine\-number, \[m\]odified\-time, \[p\]ath=\<PATH\_DISPLAY>, \[s\]ize, separator=\<SEPARATOR>, summary and \[t\]rim\-line\.

**`[-d|--dry-run]`**

Display which files should be updated but do not actually update any file\.

**`[--encoding] <ENCODING>`**

Encoding to use when a file does not contain byte order mark\. Default encoding is UTF\-8\.

**`[--evaluator] <EVALUATOR>`**

Path to the evaluator method to compute replacements\. The format is "LibraryPath,FullTypeName\.MethodName"\.

**`[-e|--extension] <EXTENSION_FILTER>`**

A filter for file extensions\. Syntax is EXT1\[,EXT2,\.\.\.\] \[\<EXTENSION\_OPTIONS>\]\. Allowed values are \[c\]ulture\-\[i\]nvariant, \[f\]rom\-file, \[i\]gnore\-case, \[li\]st\-\[s\]eparator, \[l\]iteral, n\[e\]gative and timeout=\<NUM>\.

**`[-h|--help]`**

Show command line help\.

**`[-t|--highlight] <HIGHLIGHT>`**

Parts of the output to highlight\. Allowed values are \[n\]one, \[m\]atch, \[r\]eplacement, \[e\]mpty\-\[m\]atch, \[e\]mpty\-\[r\]eplacement, \[e\]mpty\-\[s\]plit, \[e\]mpty, \[b\]oundary, \[t\]ab, \[c\]arriage\-\[r\]eturn, \[l\]ine\[f\]eed, \[n\]ew\-\[l\]ine and space\.

**`[-i|--include-directory] <REGEX>`**

Regular expression for a directory name\. Syntax is \<PATTERN> \[\<PATTERN\_OPTIONS>\]\. Allowed values are compiled, \[c\]ulture\-\[i\]nvariant, \[e\]cma\-\[s\]cript, \[n\] explicit\-capture, \[f\]rom\-file, \[g\]roup=\<GROUP\_NAME>, \[i\]gnore\-case, \[x\] ignore\-pattern\-whitespace, \[li\]st, length=\<NUM>, \[li\]st\-\[s\]eparator, \[l\]iteral, \[m\]ultiline, n\[e\]gative, \[p\]art=\<NAME\_PART>, \[r\]ight\-to\-left, \[s\]ingleline, timeout=\<NUM>, \[w\]hole\-\[i\]nput, \[w\]hole\-\[l\]ine and \[w\]hole\-word\.

**`[--input] <INPUT>`**

Text to search\.

**`[-m|--max-count] <MAX_OPTIONS>`**

Stop searching after specified number is reached\. Allowed values are \<NUM>, \[m\]atches=\<NUM> and \[m\]atching\-\[f\]iles\.

**`[-n|--name] <REGEX>`**

Regular expression for file or directory name\. Syntax is \<PATTERN> \[\<PATTERN\_OPTIONS>\]\. Allowed values are compiled, \[c\]ulture\-\[i\]nvariant, \[e\]cma\-\[s\]cript, \[n\] explicit\-capture, \[f\]rom\-file, \[g\]roup=\<GROUP\_NAME>, \[i\]gnore\-case, \[x\] ignore\-pattern\-whitespace, \[li\]st, length=\<NUM>, \[li\]st\-\[s\]eparator, \[l\]iteral, \[m\]ultiline, n\[e\]gative, \[p\]art=\<NAME\_PART>, \[r\]ight\-to\-left, \[s\]ingleline, timeout=\<NUM>, \[w\]hole\-\[i\]nput, \[w\]hole\-\[l\]ine and \[w\]hole\-word\.

**`[--no-recurse]`**

Do not search subdirectories\.

**`[--paths-from] <FILE_PATH>`**

Read the list of paths to search from a file\. Paths should be separated by newlines\.

**`[--progress]`**

Display dot \(\.\) for every hundredth searched file or directory\.

**`[-p|--properties] <FILE_PROPERTIES>`**

A filter for file properties\. Allowed values are \[c\]reation\-time=\<DATE>, \[m\]odified\-time=\<DATE> and \[s\]ize=\<NUM>\.

**`[-r|--replacement] <REPLACEMENT>`**

Replacement pattern\. Syntax is \<REPLACEMENT> \[\<REPLACEMENT\_OPTIONS>\]\. Allowed values are \[f\]rom\-file, \[l\]iteral and \[m\]ultiline\.

**`[-s|--sort] <SORT_OPTIONS>`**

Sort matched files and directories\. Allowed values are \[a\]scending, \[c\]reation\-time, \[d\]escending, max=\<NUM>, \[m\]odified\-time, \[n\]ame and \[s\]ize\.

**`[-v|--verbosity] <VERBOSITY>`**

The amount of information to display in the log\. Allowed values are \[q\]uiet, \[m\]inimal, \[n\]ormal, \[d\]etailed and \[diag\]nostic\.

**`[--file-log] <FILE_LOG>`**

Syntax is \<LOG\_PATH> \[\<LOG\_OPTIONS>\]\. Allowed values are \[v\]erbosity=\<VERBOSITY> and \[a\]ppend\.

## Use Redirected Input

Piped output from a previous command will be treated as a list of paths separated by newlines.

## Samples

### Sample

Update version in csproj and vbproj files in a current directory.

Current directory contains file **pattern.txt** with a following content:

```
(?mx)
(?<=
  ^\ *\<Version\>
)
\d+\.\d+\.\d+\.\d+
(?=
  \</Version\>
)
```

#### Syntax

```
orang replace ^
 --extension csproj,vbproj ^
 --content "pattern.txt" from-file ^
 --replacement "1.2.3.0" ^
 --highlight match replacement
```

#### Short Syntax

```
orang replace ^
 -e csproj,vbproj ^
 -c "pattern.txt" f ^
 -r "1.2.3.0" ^
 -t m r
```

### Sample

Remove duplicate words in C# comments from source files in a current directory.

Current directory contains file **pattern.txt** with a following content:

```
(?mx)
(?<=
  ^
  (\ |\t)*
  //[^\r\n]*\b(?<g>\w+)\b
)
\ +
\b\k<g>\b
```

#### Syntax

```
orang replace ^
 --extension cs ^
 --content "pattern.txt" ^
 --include-directory ".git" whole-input negative ^
 --highlight match
```

#### Short Syntax

```
orang replace ^
 -e cs ^
 -c "pattern.txt" ^
 -i ".git" wi e ^
 -t m

```

### Sample

Normalize newline to CR+LF for all files in a current directory.

#### Syntax

```
orang replace ^
 --content "(?<!\r)\n" ^
 --replacement "\r\n" multiline ^
 --verbosity minimal
```

#### Short Syntax

```
orang replace ^
 -c "(?<!\r)\n" ^
 -r "\r\n" m ^
 -v m
```
*\(Generated with [DotMarkdown](http://github.com/JosefPihrt/DotMarkdown)\)*