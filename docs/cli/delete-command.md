# `orang delete`

Deletes files and directories\.

## Synopsis

```
orang delete [<PATH>]
[   --ask]
[-a|--attributes]         <ATTRIBUTES>
[   --attributes-to-skip] <ATTRIBUTES>
[-c|--content]            <REGEX>
[   --content-only]
[-y|--display]            <DISPLAY_OPTIONS>
[-d|--dry-run]
[   --encoding]           <ENCODING>
[-e|--extension]          <EXTENSION_FILTER>
[-h|--help]
[-t|--highlight]          <HIGHLIGHT>
[-i|--include-directory]  <REGEX>
[   --including-bom]
[-m|--max-count]          <NUMBER>
 -n|--name                <REGEX>
[   --no-recurse]
[-o|--output]             <OUTPUT_OPTIONS>
[   --paths-from]         <FILE_PATH>
[   --progress]
[-p|--properties]         <FILE_PROPERTIES>
[-s|--sort]               <SORT_OPTIONS>
[-v|--verbosity]          <VERBOSITY>
[   --file-log]           <FILE_LOG>
```

## Arguments

**`<PATH>`**

Path to one or more files and/or directories that should be searched\.

## Options

**`[--ask]`**

Ask for a permission to delete file or directory\.

**`[-a|--attributes] <ATTRIBUTES>`**

File attributes that are required\. Allowed values are archive, compressed, \[d\]irectory, \[e\]mpty, encrypted, \[f\]ile, \[h\]idden, normal, offline, \[r\]ead\-only, \[r\]eparse\-\[p\]oint, \[s\]ystem and temporary\.

**`[--attributes-to-skip] <ATTRIBUTES>`**

File attributes that should be skipped\. Allowed values are archive, compressed, \[e\]mpty, encrypted, \[h\]idden, normal, offline, \[r\]ead\-only, \[r\]eparse\-\[p\]oint, \[s\]ystem and temporary\.

**`[-c|--content] <REGEX>`**

Regular expression for files' content\. Syntax is \<PATTERN> \[\<PATTERN\_OPTIONS>\]\. Allowed values are compiled, \[c\]ulture\-\[i\]nvariant, \[e\]cma\-\[s\]cript, \[n\] explicit\-capture, \[f\]rom\-file, \[g\]roup=\<GROUP\_NAME>, \[i\]gnore\-case, \[x\] ignore\-pattern\-whitespace, \[li\]st, length=\<NUM>, \[li\]st\-\[s\]eparator, \[l\]iteral, \[m\]ultiline, n\[e\]gative, \[r\]ight\-to\-left, \[s\]ingleline, timeout=\<NUM>, \[w\]hole\-\[i\]nput, \[w\]hole\-\[l\]ine and \[w\]hole\-word\.

**`[--content-only]`**

Delete content of a file or directory but not the file or directory itself\.

**`[-y|--display] <DISPLAY_OPTIONS>`**

Display of the results\. Allowed values are \[c\]reation\-\[t\]ime, indent=\<INDENT>, \[m\]odified\-time, \[p\]ath=\<PATH\_DISPLAY>, \[s\]ize, separator=\<SEPARATOR> and summary\.

**`[-d|--dry-run]`**

Display which files or directories should be deleted but do not actually delete any file or directory\.

**`[--encoding] <ENCODING>`**

Encoding to use when a file does not contain byte order mark\. Default encoding is UTF\-8\.

**`[-e|--extension] <EXTENSION_FILTER>`**

A filter for file extensions\. Syntax is EXT1\[,EXT2,\.\.\.\] \[\<EXTENSION\_OPTIONS>\]\. Allowed values are \[c\]ulture\-\[i\]nvariant, \[f\]rom\-file, \[i\]gnore\-case, \[li\]st\-\[s\]eparator, \[l\]iteral, n\[e\]gative and timeout=\<NUM>\.

**`[-h|--help]`**

Show command line help\.

**`[-t|--highlight] <HIGHLIGHT>`**

Parts of the output to highlight\. Allowed values are \[n\]one, \[m\]atch, \[e\]mpty\-\[s\]plit and \[e\]mpty\.

**`[-i|--include-directory] <REGEX>`**

Regular expression for a directory name\. Syntax is \<PATTERN> \[\<PATTERN\_OPTIONS>\]\. Allowed values are compiled, \[c\]ulture\-\[i\]nvariant, \[e\]cma\-\[s\]cript, \[n\] explicit\-capture, \[f\]rom\-file, \[g\]roup=\<GROUP\_NAME>, \[i\]gnore\-case, \[x\] ignore\-pattern\-whitespace, \[li\]st, length=\<NUM>, \[li\]st\-\[s\]eparator, \[l\]iteral, \[m\]ultiline, n\[e\]gative, \[p\]art=\<NAME\_PART>, \[r\]ight\-to\-left, \[s\]ingleline, timeout=\<NUM>, \[w\]hole\-\[i\]nput, \[w\]hole\-\[l\]ine and \[w\]hole\-word\.

**`[--including-bom]`**

Delete byte order mark \(BOM\) when deleting file's content\.

**`[-m|--max-count] <NUMBER>`**

Stop renaming after specified number is reached\.

**`-n|--name <REGEX>`**

Regular expression for file or directory name\. Syntax is \<PATTERN> \[\<PATTERN\_OPTIONS>\]\. Allowed values are compiled, \[c\]ulture\-\[i\]nvariant, \[e\]cma\-\[s\]cript, \[n\] explicit\-capture, \[f\]rom\-file, \[g\]roup=\<GROUP\_NAME>, \[i\]gnore\-case, \[x\] ignore\-pattern\-whitespace, \[li\]st, length=\<NUM>, \[li\]st\-\[s\]eparator, \[l\]iteral, \[m\]ultiline, n\[e\]gative, \[p\]art=\<NAME\_PART>, \[r\]ight\-to\-left, \[s\]ingleline, timeout=\<NUM>, \[w\]hole\-\[i\]nput, \[w\]hole\-\[l\]ine and \[w\]hole\-word\.

**`[--no-recurse]`**

Do not search subdirectories\.

**`[-o|--output] <OUTPUT_OPTIONS>`**

Path to a file that should store results\. Syntax is \<PATH> \[\<OUTPUT\_OPTIONS>\]\. Allowed values are \[c\]ontent, \[p\]ath and \[e\]ncoding=\<ENCODING>\.

**`[--paths-from] <FILE_PATH>`**

Read the list of paths to search from a file\. Paths should be separated by newlines\.

**`[--progress]`**

Display dot \(\.\) for every hundredth searched file or directory\.

**`[-p|--properties] <FILE_PROPERTIES>`**

A filter for file properties\. Allowed values are \[c\]reation\-time=\<DATE>, \[m\]odified\-time=\<DATE> and \[s\]ize=\<NUM>\.

**`[-s|--sort] <SORT_OPTIONS>`**

Sort matched files and directories\. Allowed values are \[a\]scending, \[c\]reation\-time, \[d\]escending, max=\<NUM>, \[m\]odified\-time, \[n\]ame and \[s\]ize\.

**`[-v|--verbosity] <VERBOSITY>`**

The amount of information to display in the log\. Allowed values are \[q\]uiet, \[m\]inimal, \[n\]ormal, \[d\]etailed and \[diag\]nostic\.

**`[--file-log] <FILE_LOG>`**

Syntax is \<LOG\_PATH> \[\<LOG\_OPTIONS>\]\. Allowed values are \[v\]erbosity=\<VERBOSITY> and \[a\]ppend\.

## Samples

### Sample

Delete empty directories in a current directory.

#### Syntax

```
orang delete ^
 --name "^(?=)" ^
 --attributes directory empty
```

#### Short Syntax

```
orang delete ^
 -n "^(?=)" ^
 -a d e
```

### Sample

Delete content of "bin" or "obj" directories in a current directory.

#### Syntax

```
orang delete ^
 --name "bin|obj" whole-input ^
 --attributes directory ^
 --content-only
```

#### Short Syntax

```
orang delete ^
 -n "bin|obj" wi ^
 -a d ^
 --content-only
```

*\(Generated with [DotMarkdown](http://github.com/JosefPihrt/DotMarkdown)\)*