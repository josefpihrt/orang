## Exit Code

Code | Comment
--- | ---
0 | Match found
1 | No match found
2 | Error occurred or execution canceled

## Redirected/Piped Input

Redirected/piped input will be used either as a raw text or as a list of paths separated with newlines.

Command | Piped Input
--- | ---
[copy](cli/commands/copy) | list of paths
[delete](cli/commands/delete) | list of paths
[find](cli/commands/find) | text (default) or list of paths when `--pipe p[aths]` is specified
[move](cli/commands/move) | list of paths
[regex escape](cli/commands/regex-escape) | text
[regex match](cli/commands/regex-match) | text
[regex split](cli/commands/regex-split) | text
[rename](cli/commands/rename) | list of paths
[replace](cli/commands/replace) | text (default) or list of paths when `--pipe p[aths]` is specified
[sync](cli/commands/sync) | list of paths

## Multi-value Parameters

A lot of Orang parameters can have multiple values. Only shortcoming of this approach is that
a user cannot specify argument (usually path(s)) as a last value of a command
if the argument is preceded with multi-value parameter.

Following command is invalid because path `C:/Documents` is treated as a value of multi-value parameter `-c | --content`.
```
orang find -c "^abc" i m "C:/Documents"
```

To fix this problem you can either add parameter `--paths`
```
orang find -c "abc" i m --paths "C:/Documents"
```

or you can specify path right after the command name:

```
orang find "C:/Documents" -c "abc" i m
```

## Links

* [List of Option Values](cli/option-values)
* [How to's](cli/how-to)

## External Links

* [.NET Core Global Tools Overview](https://docs.microsoft.com/dotnet/core/tools/global-tools)
* [Create a .NET Core Global Tool Using the .NET Core CLI](https://docs.microsoft.com/dotnet/core/tools/global-tools-how-to-create)
* [.NET Core 2.1 Global Tools](https://natemcmaster.com/blog/2018/05/12/dotnet-global-tools/)
* [Windows CMD Shell](https://ss64.com/nt/syntax.html)
* [Parsing C++ Command-Line Arguments](https://docs.microsoft.com/cpp/cpp/parsing-cpp-command-line-arguments?view=vs-2019)
