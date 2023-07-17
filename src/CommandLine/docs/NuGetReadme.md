# Orang.DotNet.Cli

Orang is a cross-platform command-line tool for:

* [searching](https://josefpihrt.github.io/docs/orang/cli/commands/find) files, directories and files' content,
* [replacing](https://josefpihrt.github.io/docs/orang/cli/commands/replace) files' content,
* [copying](https://josefpihrt.github.io/docs/orang/cli/commands/copy) files and directories,
* [moving](https://josefpihrt.github.io/docs/orang/cli/commands/move) files and directories,
* [renaming](https://josefpihrt.github.io/docs/orang/cli/commands/rename) files and directories,
* [deleting](https://josefpihrt.github.io/docs/orang/cli/commands/delete) files, directories or its content,
* [synchronizing](https://josefpihrt.github.io/docs/orang/cli/commands/sync) content of two directories,
* [spellchecking](https://josefpihrt.github.io/docs/orang/cli/commands/spellcheck) files' content,
* executing [Regex](https://docs.microsoft.com/cs-cz/dotnet/api/system.text.regularexpressions.regex?view=netcore-3.0) functions such as [match](https://josefpihrt.github.io/docs/orang/cli/commands/match) or [split](https://josefpihrt.github.io/docs/orang/cli/commands/split)

All these commands are powered with [.NET regular expression engine](https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expressions).

## Requirements

Orang requires [.NET Core Runtime](https://dotnet.microsoft.com/download/dotnet-core/current/runtime) 3.1 or higher.

## How to Install

To install Orang run:

```
dotnet tool install -g orang.dotnet.cli
```

To install non-alpha version run:

```
dotnet tool install -g orang.dotnet.cli --version <VERSION>
```

To update Orang run:

```
dotnet tool update -g orang.dotnet.cli
```

## How to Use

```
orang [command] [parameters]
```

## Basic Commands

* [find](https://josefpihrt.github.io/docs/orang/cli/commands/find)
* [replace](https://josefpihrt.github.io/docs/orang/cli/commands/replace)
* [rename](https://josefpihrt.github.io/docs/orang/cli/commands/rename)
* [delete](https://josefpihrt.github.io/docs/orang/cli/commands/delete)
* [spellcheck](https://josefpihrt.github.io/docs/orang/cli/commands/spellcheck)

For a list of all commands please see [Orang Command-Line Reference](https://josefpihrt.github.io/docs/orang/cli/commands)

## How to Learn

For a full list of commands, parameters and parameter values run:

```
orang help [command] [-v d]
```

For a full manual run:

```
orang help -m [-v d]
```

For a full list of .NET regular expressions syntax run:

```
orang list-patterns
```

## Features

- Single match can span over multiple lines
- Display match and replacement side-by-side
- Use C# code to compute replacements
- Load pattern from a file
