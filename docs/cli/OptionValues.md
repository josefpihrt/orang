# List of Option Values

## &lt;ASK\_MODE&gt;

| Value | Shortcut | Description                             |
| ----- | -------- | --------------------------------------- |
| file  | f        | Ask for confirmation after each file\.  |
| value | v        | Ask for confirmation after each value\. |

## &lt;ATTRIBUTES&gt;

| Value                 | Shortcut | Description |
| --------------------- | -------- | ----------- |
| archive               |          |             |
| compressed            |          |             |
| directory             | d        |             |
| empty                 | e        |             |
| encrypted             |          |             |
| file                  | f        |             |
| hidden                | h        |             |
| integrity\-stream     |          |             |
| normal                |          |             |
| no\-scrub\-data       |          |             |
| not\-content\-indexed |          |             |
| offline               |          |             |
| read\-only            | r        |             |
| reparse\-point        | rp       |             |
| sparse\-file          |          |             |
| system                | s        |             |
| temporary             |          |             |

## &lt;COMPARE\_OPTIONS&gt;

| Value          | Shortcut | Description                             |
| -------------- | -------- | --------------------------------------- |
| none           | n        | Compare files only by name\.            |
| attributes     | a        | Compare file attributes\.               |
| content        | c        | Compare file content\.                  |
| modified\-time | mt       | Compare time a file was last modified\. |
| size           | s        | Compare file size\.                     |

## &lt;CONFLICT\_RESOLUTION&gt;

| Value     | Shortcut | Description                                       |
| --------- | -------- | ------------------------------------------------- |
| ask       | a        | Ask when a file or already exists\.               |
| overwrite | o        | Overwrite a file when it already exists\.         |
| suffix    |          | Add suffix to file name if it already exists\.    |
| skip      | s        | Do not copy or move a file if it already exists\. |

## &lt;CONTENT\_MODE&gt;

| Value            | Shortcut | Description                                                                              |
| ---------------- | -------- | ---------------------------------------------------------------------------------------- |
| all\-lines       | a        | Display all lines\.                                                                      |
| line             | l        | Display entire line containing the matched value\.                                       |
| unmatched\-lines | u        | Display all lines that do not contain the matched value\.                                |
| value            | v        | Display just the matched value\.                                                         |
| value\-detail    | d        | Display matched value and include information about each value such as index or length\. |
| omit             | o        | Do not display file's content\.                                                          |

## &lt;EXTENSION\_OPTIONS&gt;

| Value                             | Shortcut | Description                                                                                                             |
| --------------------------------- | -------- | ----------------------------------------------------------------------------------------------------------------------- |
| case\-sensitive                   | cs       | Use case\-sensitive matching\.                                                                                          |
| culture\-invariant                | ci       | Ignore cultural differences between languages\.                                                                         |
| from\-file                        | f        | Load pattern from a file\.                                                                                              |
| list\-separator=&lt;SEPARATOR&gt; | ls       | String that separate each value in a list\. Default value is comma \(,\) or newline if the list is loaded from a file\. |
| literal                           | l        | Pattern should be treated as a literal expression and not as a regular expression\.                                     |
| negative                          | ne       | Search succeeds if the regular expression does not match\.                                                              |
| timeout=&lt;NUM&gt;               |          | Match time\-out interval in seconds\.                                                                                   |

## &lt;FILE\_PROPERTIES&gt;

| Value                       | Shortcut | Description                                                                                                   |
| --------------------------- | -------- | ------------------------------------------------------------------------------------------------------------- |
| creation\-time=&lt;DATE&gt; | ct       | Show file's creation time and optionally define condition \(See 'Expression syntax' for other expressions\)\. |
| modified\-time=&lt;DATE&gt; | mt       | Show file's modified time and optionally define condition \(See 'Expression syntax' for other expressions\)\. |
| size=&lt;NUM&gt;            | s        | Show file's size and optionally define condition \(See 'Expression syntax' for other expressions\)\.          |

## &lt;HIGHLIGHT&gt;

| Value              | Shortcut | Description                                         |
| ------------------ | -------- | --------------------------------------------------- |
| none               | n        | No highlighting\.                                   |
| match              | m        | Highlight match value\.                             |
| replacement        | r        | Highlight replacement value\.                       |
| split              | s        | Highlight split value\.                             |
| empty\-match       | em       | Highlight match value that is empty string\.        |
| empty\-replacement | er       | Highlight replacement value that is empty string\.  |
| empty\-split       | es       | Highlight split value that is empty string\.        |
| empty              | e        | Highlight value that is empty string\.              |
| boundary           | b        | Highlight start and end of the value\.              |
| tab                | t        | Highlight tab character\.                           |
| carriage\-return   | cr       | Highlight carriage return character\.               |
| linefeed           | lf       | Highlight linefeed character\.                      |
| newline            |          | Highlight carriage return and linefeed characters\. |
| space              |          | Highlight space character\.                         |

## &lt;INPUT\_OPTIONS&gt;

| Value  | Shortcut | Description                                                                    |
| ------ | -------- | ------------------------------------------------------------------------------ |
| escape | e        | Interpret literals \\a, \\b, \\f, \\n, \\r, \\t and \\v as character escapes\. |

## &lt;MODIFY\_OPTIONS&gt;

| Value                                            | Shortcut | Description                                                    |
| ------------------------------------------------ | -------- | -------------------------------------------------------------- |
| aggregate                                        | ag       | Display list of all values at the end of search\.              |
| aggregate\-only                                  | o        | Display only list of all values at the end of search\.         |
| ascending                                        | a        | Sort values in an ascending order\.                            |
| culture\-invariant                               | ci       | Ignore cultural differences between languages\.                |
| descending                                       | d        | Sort values in a descending order\.                            |
| distinct                                         | di       | Return distinct values\.                                       |
| except                                           |          | Return values from first file except values from second file\. |
| intersect                                        |          | Return values that were found in all files\.                   |
| group                                            |          | Group matching files by matched values\.                       |
| count                                            |          | Show number of values in a group\.                             |
| ignore\-case                                     | i        | Use case\-insensitive matching\.                               |
| remove\-empty                                    | re       | Remove values that are empty strings\.                         |
| remove\-white\-space                             | rw       | Remove values that are empty or consist of white\-space\.      |
| sort\-by=[&lt;SORT_PROPERTY&gt;](#sort_property) |          |                                                                |
| to\-lower                                        | tl       | Convert value to lowercase\.                                   |
| to\-upper                                        | tu       | Convert value to uppercase\.                                   |
| trim                                             | t        | Trim leading and trailing white\-space\.                       |
| trim\-end                                        | te       | Trim trailing white\-space\.                                   |
| trim\-start                                      | ts       | Trim leading white\-space\.                                    |

## &lt;NAME\_PART&gt;

| Value                    | Shortcut | Description                             |
| ------------------------ | -------- | --------------------------------------- |
| extension                | e        | Search in file extension\.              |
| full\-name               | f        | Search in full path\.                   |
| name                     | n        | Search in file name and its extension\. |
| name\-without\-extension | w        | Search in file name without extension\. |

## &lt;OUTPUT\_OPTIONS&gt;

| Value                                     | Shortcut | Description                                                         |
| ----------------------------------------- | -------- | ------------------------------------------------------------------- |
| encoding=&lt;ENCODING&gt;                 | e        |                                                                     |
| verbosity=[&lt;VERBOSITY&gt;](#verbosity) | v        |                                                                     |
| append                                    | a        | If the file exists output will be appended to the end of the file\. |

## &lt;PATH\_MODE&gt;

| Value    | Shortcut | Description                                     |
| -------- | -------- | ----------------------------------------------- |
| full     | f        | Display full path\.                             |
| relative | r        | Display path relatively to the base directory\. |
| match    | m        | Display only match\.                            |
| omit     | o        | Do not display path\.                           |

## &lt;PATTERN\_OPTIONS&gt;

| Value                                | Shortcut | Description                                                                                                                                            |
| ------------------------------------ | -------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| compiled                             |          | Compile the regular expression to an assembly\.                                                                                                        |
| culture\-invariant                   | ci       | Ignore cultural differences between languages\.                                                                                                        |
| ecma\-script                         | es       | Enable ECMAScript\-compliant behavior for the expression\.                                                                                             |
| ends\-with                           | ew       | Pattern should match from the end of the input string\.                                                                                                |
| equals                               | e        | Pattern should match whole input string\.                                                                                                              |
| explicit\-capture                    | n        | Do not capture unnamed groups\.                                                                                                                        |
| from\-file                           | f        | Load pattern from a file\.                                                                                                                             |
| group=&lt;GROUP\_NAME&gt;            | g        |                                                                                                                                                        |
| ignore\-case                         | i        | Use case\-insensitive matching\.                                                                                                                       |
| ignore\-pattern\-whitespace          | x        | Exclude unescaped white\-space from the pattern and enable comments after a number sign \(\#\)\.                                                       |
| list                                 | li       | Interpret pattern as a list of patterns any of which is to be matched\. Separator is either comma \(,\) or newline if the list is loaded from a file\. |
| length=&lt;NUM&gt;                   |          | Include matches whose length matches the expression \(See 'Expression syntax' for other expressions\)\.                                                |
| list\-separator=&lt;SEPARATOR&gt;    | ls       | String that separate each value in a list\. Default value is comma \(,\) or newline if the list is loaded from a file\.                                |
| literal                              | l        | Pattern should be treated as a literal expression and not as a regular expression\.                                                                    |
| multiline                            | m        | ^ and $ match the beginning and end of each line \(instead of the beginning and end of the input string\)\.                                            |
| negative                             | ne       | Search succeeds if the regular expression does not match\.                                                                                             |
| part=[&lt;NAME_PART&gt;](#name_part) | p        | The part of a file or a directory name that should be matched\.                                                                                        |
| right\-to\-left                      | r        | Specifies that the search will be from right to left\.                                                                                                 |
| singleline                           | s        | The period \(\.\) matches every character \(instead of every character except \\n\)\.                                                                  |
| starts\-with                         | sw       | Pattern should match from the start of the input string\.                                                                                              |
| timeout=&lt;NUM&gt;                  |          | Match time\-out interval in seconds\.                                                                                                                  |
| whole\-line                          | wl       | Pattern should match whole line\.                                                                                                                      |
| whole\-word                          | w        | Pattern should match whole word\.                                                                                                                      |

## &lt;PIPE\_MODE&gt;

| Value | Shortcut | Description                                                       |
| ----- | -------- | ----------------------------------------------------------------- |
| text  | t        | Use redirected input as a text to be searched\.                   |
| paths | p        | Use redirected input as a list of paths separated with newlines\. |

## &lt;REGEX\_OPTIONS&gt;

| Value                       | Shortcut | Description                                                                                                 |
| --------------------------- | -------- | ----------------------------------------------------------------------------------------------------------- |
| compiled                    | c        | Compile the regular expression to an assembly\.                                                             |
| culture\-invariant          | ci       | Ignore cultural differences between languages\.                                                             |
| ecma\-script                | es       | Enable ECMAScript\-compliant behavior for the expression\.                                                  |
| explicit\-capture           | n        | Do not capture unnamed groups\.                                                                             |
| ignore\-case                | i        | Use case\-insensitive matching\.                                                                            |
| ignore\-pattern\-whitespace | x        | Exclude unescaped white\-space from the pattern and enable comments after a number sign \(\#\)\.            |
| multiline                   | m        | ^ and $ match the beginning and end of each line \(instead of the beginning and end of the input string\)\. |
| right\-to\-left             | r        | Specifies that the search will be from right to left\.                                                      |
| singleline                  | s        | The period \(\.\) matches every character \(instead of every character except \\n\)\.                       |

## &lt;REPLACE\_MODIFY&gt;

| Value              | Shortcut | Description                                     |
| ------------------ | -------- | ----------------------------------------------- |
| culture\-invariant | ci       | Ignore cultural differences between languages\. |
| to\-lower          | tl       | Convert value to lowercase\.                    |
| to\-upper          | tu       | Convert value to uppercase\.                    |
| trim               | t        | Trim leading and trailing white\-space\.        |
| trim\-end          | te       | Trim trailing white\-space\.                    |
| trim\-start        | ts       | Trim leading white\-space\.                     |

## &lt;REPLACEMENT\_OPTIONS&gt;

| Value      | Shortcut | Description                                                                                                                                                                                                                                                                                                                                                     |
| ---------- | -------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| from\-file | f        | Load text from a file whose path is specified in &lt;REPLACEMENT&gt; value\.                                                                                                                                                                                                                                                                                    |
| literal    | l        | Replacement should be treated as a literal expression and not as a replacement expression\.                                                                                                                                                                                                                                                                     |
| escape     | e        | Interpret literals \\a, \\b, \\f, \\n, \\r, \\t and \\v as character escapes\.                                                                                                                                                                                                                                                                                  |
| csharp     | cs       | &lt;REPLACEMENT&gt; is either expression\-body of a method with signature 'string M\(Match match\)' or a path to a code file that contains public method with signature 'string M\(Match match\)'\. Imported namespaces \(when inline expression is specified\): System System\.Collections\.Generic System\.Linq System\.Text System\.Text\.RegularExpressions |
| from\-dll  |          | &lt;REPLACEMENT&gt; is a path to a method in DLL file\. The format is 'DllPath,FullTypeName\.MethodName'\.                                                                                                                                                                                                                                                      |

## &lt;SORT\_OPTIONS&gt;

| Value                  | Shortcut | Description                                     |
| ---------------------- | -------- | ----------------------------------------------- |
| ascending              | a        | Sort items in ascending order\.                 |
| creation\-time         | ct       | Sort items by creation time\.                   |
| culture\-invariant     | ci       | Ignore cultural differences between languages\. |
| descending             | d        | Sort items in descending order\.                |
| max\-count=&lt;NUM&gt; | m        | Show only &lt;NUM&gt; items\.                   |
| modified\-time         | mt       | Sort items by last modified time\.              |
| name                   | n        | Sort items by full name\.                       |
| size                   | s        | Sort items by size\.                            |

## &lt;SORT\_PROPERTY&gt;

| Value  | Shortcut | Description                     |
| ------ | -------- | ------------------------------- |
| length | l        | Sort values by value's length\. |
| count  | c        | Sort values by group's count\.  |

## &lt;SYNC\_CONFLICT\_RESOLUTION&gt;

| Value        | Shortcut | Description |
| ------------ | -------- | ----------- |
| ask          | a        |             |
| first\-wins  | f        |             |
| second\-wins | s        |             |

## &lt;SYNTAX\_SECTIONS&gt;

| Value                     | Shortcut | Description |
| ------------------------- | -------- | ----------- |
| alternation\-constructs   | ac       |             |
| anchors                   | a        |             |
| backreference\-constructs | b        |             |
| character\-classes        | c        |             |
| character\-escapes        | ce       |             |
| general\-categories       | gc       |             |
| grouping\-constructs      | g        |             |
| miscellaneous             | m        |             |
| named\-blocks             | n        |             |
| options                   | o        |             |
| regex\-options            | ro       |             |
| quantifiers               | q        |             |
| substitutions             | s        |             |
| all                       |          |             |

## &lt;VERBOSITY&gt;

| Value      | Shortcut | Description |
| ---------- | -------- | ----------- |
| quiet      | q        |             |
| minimal    | m        |             |
| normal     | n        |             |
| detailed   | d        |             |
| diagnostic | di       |             |

## Expression Syntax

| Expression            | Description                                    |
| --------------------- | ---------------------------------------------- |
| `x=n`                 |
| `x<n`                 |
| `x>n`                 |
| `x<=n`                |
| `x>=n`                |
| `x=<min;max>`         | Inclusive interval                             |
| `x=(min;max)`         | Exclusive interval                             |
| `x=-d\|[d.]hh:mm[:ss]` | x is greater than actual date \- &lt;VALUE&gt; |

*\(Generated with [DotMarkdown](http://github.com/JosefPihrt/DotMarkdown)\)*