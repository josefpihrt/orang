# Orang.DotNet.Cli

Orang is a cross-platform command-line tool for:

* [searching](docs/cli/find-command.md) files, directories and files' content,
* [replacing](docs/cli/replace-command.md) files' content,
* [copying](docs/cli/copy-command.md) files and directories,
* [moving](docs/cli/move-command.md) files and directories,
* [renaming](docs/cli/rename-command.md) files and directories,
* [deleting](docs/cli/delete-command.md) files, directories or its content,
* [synchronizing](docs/cli/sync-command.md) content of two directories,
* [spellchecking](docs/cli/spellcheck-command.md) files' content,
* executing [Regex](https://docs.microsoft.com/cs-cz/dotnet/api/system.text.regularexpressions.regex?view=netcore-3.0) functions such as [match](docs/cli/match-command.md) or [split](docs/cli/split-command.md)

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

* [find](docs/cli/find-command.md)
* [replace](docs/cli/replace-command.md)
* [rename](docs/cli/rename-command.md)
* [delete](docs/cli/delete-command.md)
* [spellcheck](docs/cli/spellcheck-command.md)

For a list of all commands please see [Orang Command-Line Reference](docs/cli/README.md)

## How to Learn

For a full list of commands, parameters and parameter values run:

```
orang help [command] [-v d]
```

For a full [manual](docs/cli/manual.txt) run:

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
