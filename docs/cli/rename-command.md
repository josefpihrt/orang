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
[-m|--max-count]          <NUM>
 -n|--name                <REGEX>
[   --no-recurse]
[-o|--output]             <OUTPUT_OPTIONS>
[   --paths-from]         <FILE_PATH>
[   --progress]
[-p|--properties]         <FILE_PROPERTIES>
[-r|--replacement]        <REPLACEMENT>
[-s|--sort]               <SORT_OPTIONS>
[-v|--verbosity]          <VERBOSITY>
```

## Arguments

**`<PATH>`**

Path to one or more files and/or directories that should be searched\.

## Options

**`[--ask]`**

Ask for a permission to rename file or directory\.

**`[-a|--attributes] <ATTRIBUTES>`**

File attributes that are required\. Allowed values are archive, compressed, d\[irectory\], e\[mpty\], encrypted, f\[ile\], h\[idden\], normal, offline, r\[ead\-only\], r\[eparse\]\-p\[oint\], s\[ystem\] and temporary\.

**`[--attributes-to-skip] <ATTRIBUTES>`**

File attributes that should be skipped\. Allowed values are archive, compressed, e\[mpty\], encrypted, h\[idden\], normal, offline, r\[ead\-only\], r\[eparse\]\-p\[oint\], s\[ystem\] and temporary\.

**`[-c|--content] <REGEX>`**

Regular expression for files' content\. Syntax is \<PATTERN> \[\<PATTERN\_OPTIONS>\]\. Allowed values are compiled, c\[ulture\-\]i\[nvariant\], e\[cma\-\]s\[cript\], n \[explicit\-capture\], f\[rom\-file\], g\[roup\]=\<GROUP\_NAME>, i\[gnore\-case\], x \[ignore\-pattern\-whitespace\], li\[st\], length=\<NUM>, l\[ist\-\]s\[eparator\], l\[iteral\], m\[ultiline\], ne\[gative\], r\[ight\-to\-left\], s\[ingleline\], timeout=\<NUM>, w\[hole\-\]i\[nput\], w\[hole\-\]l\[ine\] and w\[hole\-word\]\.

**`[-y|--display] <DISPLAY_OPTIONS>`**

Display of the results\. Allowed values are c\[reation\-\]t\[ime\], indent=\<INDENT>, m\[odified\-\]t\[ime\], p\[ath\]=\<PATH\_DISPLAY>, s\[ize\], separator=\<SEPARATOR> and summary\.

**`[-d|--dry-run]`**

Display which files or directories should be renamed but do not actually rename any file or directory\.

**`[--encoding] <ENCODING>`**

Encoding to use when a file does not contain byte order mark\. Default encoding is UTF\-8\.

**`[--evaluator] <EVALUATOR>`**

Path to the evaluator method to compute replacements\. The format is "LibraryPath,FullTypeName\.MethodName"\.

**`[-e|--extension] <EXTENSION_FILTER>`**

A filter for file extensions\. Syntax is EXT1\[,EXT2,\.\.\.\] \[\<EXTENSION\_OPTIONS>\]\. Allowed values are c\[ulture\-\]i\[nvariant\], f\[rom\-file\], i\[gnore\-case\], l\[ist\-\]s\[eparator\], l\[iteral\], ne\[gative\] and timeout=\<NUM>\.

**`[-h|--help]`**

Show command line help\.

**`[-t|--highlight] <HIGHLIGHT>`**

Parts of the output to highlight\. Allowed values are n\[one\], m\[atch\], r\[eplacement\], e\[mpty\-\]s\[plit\] and e\[mpty\]\.

**`[-i|--include-directory] <REGEX>`**

Regular expression for a directory name\. Syntax is \<PATTERN> \[\<PATTERN\_OPTIONS>\]\. Allowed values are compiled, c\[ulture\-\]i\[nvariant\], e\[cma\-\]s\[cript\], n \[explicit\-capture\], f\[rom\-file\], g\[roup\]=\<GROUP\_NAME>, i\[gnore\-case\], x \[ignore\-pattern\-whitespace\], li\[st\], length=\<NUM>, l\[ist\-\]s\[eparator\], l\[iteral\], m\[ultiline\], ne\[gative\], p\[art\]=\<NAME\_PART>, r\[ight\-to\-left\], s\[ingleline\], timeout=\<NUM>, w\[hole\-\]i\[nput\], w\[hole\-\]l\[ine\] and w\[hole\-word\]\.

**`[-m|--max-count] <NUM>`**

Stop deleting after specified number is reached\.

**`-n|--name <REGEX>`**

Regular expression for file or directory name\. Syntax is \<PATTERN> \[\<PATTERN\_OPTIONS>\]\. Allowed values are compiled, c\[ulture\-\]i\[nvariant\], e\[cma\-\]s\[cript\], n \[explicit\-capture\], f\[rom\-file\], i\[gnore\-case\], x \[ignore\-pattern\-whitespace\], li\[st\], length=\<NUM>, l\[ist\-\]s\[eparator\], l\[iteral\], m\[ultiline\], p\[art\]=\<NAME\_PART>, r\[ight\-to\-left\], s\[ingleline\], timeout=\<NUM>, w\[hole\-\]i\[nput\], w\[hole\-\]l\[ine\] and w\[hole\-word\]\.

**`[--no-recurse]`**

Do not search subdirectories\.

**`[-o|--output] <OUTPUT_OPTIONS>`**

Path to a file that should store output\. Syntax is \<PATH> \[\<OUTPUT\_OPTIONS>\]\. Allowed values are e\[ncoding\]=\<ENCODING>, v\[erbosity\]=\<VERBOSITY> and a\[ppend\]\.

**`[--paths-from] <FILE_PATH>`**

Read the list of paths to search from a file\. Paths should be separated by newlines\.

**`[--progress]`**

Display dot \(\.\) for every hundredth searched file or directory\.

**`[-p|--properties] <FILE_PROPERTIES>`**

A filter for file properties\. Allowed values are c\[reation\-\]t\[ime\], m\[odified\-\]t\[ime\] and s\[ize\]=\<NUM>\.

**`[-r|--replacement] <REPLACEMENT>`**

Replacement pattern\. Syntax is \<REPLACEMENT> \[\<REPLACEMENT\_OPTIONS>\]\. Allowed values are f\[rom\-file\], l\[iteral\] and c\[haracter\-\]e\[scapes\]\.

**`[-s|--sort] <SORT_OPTIONS>`**

Sort matched files and directories\. Allowed values are a\[scending\], c\[reation\-\]t\[ime\], d\[escending\], m\[ax\-count\]=\<NUM>, m\[odified\-\]t\[ime\], n\[ame\] and s\[ize\]\.

**`[-v|--verbosity] <VERBOSITY>`**

The amount of information to display in the log\. Allowed values are q\[uiet\], m\[inimal\], n\[ormal\], d\[etailed\] and di\[agnostic\]\.

## Use Redirected Input

Piped output from a previous command will be treated as a list of paths separated by newlines.

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