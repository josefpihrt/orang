# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.6.0-beta] - 2023-10-19

### Added

- Add globbing functionality ([#74](https://github.com/josefpihrt/orang/pull/74)).
  - Add options `--include`, `--exclude`.
  - Make option `--include-directory` obsolete.

### Changed

- Update logo ([#72](https://github.com/josefpihrt/orang/pull/72)).
- Open help in browser on `orang help <COMMAND>` ([#76](https://github.com/josefpihrt/orang/pull/76)).
  - Deprecate options `--online` and `--filter`.
- Change traversal to depth-first ([#77](https://github.com/josefpihrt/orang/pull/77)).

### Removed

- Remove deprecated options ([#83](https://github.com/josefpihrt/orang/pull/83)).
  - Remove option `--display`
  - Remove option `--evaluator`

## [0.5.0] - 2023-09-26

### Added

- Add command `regex create` ([#45](https://github.com/josefpihrt/orang/pull/45)).
- Add parameter `--max-depth` for maximum directory depth ([#44](https://github.com/josefpihrt/orang/pull/44)).
  - Add property `Orang.FileSystem.DirectoryMatcher.MaxDepth`

### Changed

- Bump Roslyn to 4.7.0 ([#64](https://github.com/josefpihrt/orang/pull/64)).
- Change target framework to .NET 6 ([#66](https://github.com/josefpihrt/orang/pull/66)).
- Update logo ([#70](https://github.com/josefpihrt/orang/pull/70)).

### Fixed

- Fix exit code of command `spellcheck` ([#63](https://github.com/josefpihrt/orang/pull/63)).

## [0.4.0] - 2023-08-17

## [0.4.0-beta.2] - 2023-07-29

### Added

- Fix validation of group name ([#58](https://github.com/josefpihrt/orang/pull/58)).

## [0.4.0-beta] - 2023-07-28

### Added

- Add [.NET API](https://josefpihrt.github.io/docs/orang/ref) ([#51](https://github.com/josefpihrt/orang/pull/51)).

### Changed

- Migrate documentation to [Docusaurus](https://josefpihrt.github.io/docs/orang) ([#48](https://github.com/josefpihrt/orang/pull/48)).

### Fixed

- Check correctly if name changed during renaming ([#54](https://github.com/josefpihrt/orang/pull/54)).

## [0.3.2] - 2022-06-26

### Added

- Add support for Source Link ([#42](https://github.com/josefpihrt/orang/pull/42)).
- Add CI/CD pipeline ([#41](https://github.com/josefpihrt/orang/pull/41)).
- Add NuGet readme file ([#38](https://github.com/josefpihrt/orang/pull/38)).

### Changed

- Migrate script to PowerShell ([#40](https://github.com/josefpihrt/orang/pull/40)).
- Replace ruleset with editorconfig ([#39](https://github.com/josefpihrt/orang/pull/39)).
- Format changelog according to 'Keep a Changelog' ([#37](https://github.com/josefpihrt/orang/pull/37)).
- Rename `master` to `main`.

### Fixed

- Specify max matching files with `--max-count` correctly ([commit](https://github.com/josefpihrt/orang/commit/a6b2b3379a170741f4ae47747220a9ae5e8fcf08)).

-----
<!-- Content below does not adhere to 'Keep a Changelog' format -->

## 0.3.1 (2022-03-29)

## 0.3.0-rc (2021-11-14)

* Change shortcut for `--attributes-to-skip` from `-b` to `-A`
* Empty name pattern matches entire name (or its part): `--name ""`
* Empty content pattern matches entire file's content: `--content ""`
* Matching lines/values are not indented by default
* Add allowed values for option `--modify`
  * `count`
  * `sort-by=count`
* Add option `--max-matches-in-file`
* Add option `--max-matching-files`
* Add option `--max-word-length` (command `spellcheck`)
* Add option `-P,--no-path` (a shortcut for `--path-mode omit`)
* Add option `-C,--no-content` (a shortcut for `--content-mode omit`)
* Deprecate option `--display` and replace it with:
  * `--after-context`
  * `--before-context`
  * `--content-mode`
  * `--context`
  * `--count`
  * `--line-number`
  * `--path-mode`
  * `-u,--summary`
  * `--align-columns`
* Deprecate command `escape` and replace it with `regex escape`
* Deprecate command `list-patterns` and replace it with `regex list`
* Deprecate command `match` and replace it with `regex match`
* Deprecate command `split` and replace it with `regex split`

## 0.2.2 (2021-07-21)

* Add command `sync` ([commit](https://github.com/JosefPihrt/Roslynator/commit/81b2f412f81edb4f205195226777f0a7f565a350))
* Target directory can be also specified as a last unnamed parameter (applies to `copy` and `move` commands) ([commit](https://github.com/JosefPihrt/Roslynator/commit/fe08b08a567d2da0756cb3a3a2524ea593367847))

## 0.2.1 (2021-06-08)

* Add command `spellcheck` ([commit](https://github.com/JosefPihrt/Roslynator/commit/6d815be953b0d01b9f777d4a6323cb26cc944c33))
* Add option `--interactive` (applies to `rename` and `replace` commands) ([commit](https://github.com/JosefPihrt/Roslynator/commit/79f51787c6422087ecd408d97641c39b64770e47))
* Add value `culture-invariant` to option `--sort` ([commit](https://github.com/JosefPihrt/Roslynator/commit/3bf8db68ad480d96b43992f6d143c6668ea58638))

## 0.2.0 (2021-01-30)

## 0.2.0-rc (2021-01-04)

* Update target framework to .netcoreapp3.1 (LTS)
* Update Roslyn reference to 3.8.0
* Disallow empty regex pattern
* Copy/move directory including its content
* Add option `--split` (applies to `find` command)
* Add option `--pipe p[aths]` to use piped input as a list of paths  (applies to `find` and `replace` commands)
* Add option value `--modify group` to group paths by matched value  (applies to `find` command)
* Add option value `--display c[ontent]=o[mit]` to not display content of a file (applies to `find` and `replace` commands)
* Add option `-o|--online` to open help in a default browser

### Breaking Changes

* Redirected/piped input is used by default as a text not as a list of paths (applies to `find` and `replace` commands)
* Filter for `list-patterns` command uses regex search instead of simple text search and search is case-insensitive by default
* Filter for `help` command is case-insensitive by default
* Option `--ask` has no value (applies to `copy` and `move`)
* Change option value `new-line` to `newline` and remove shortcut `nl`
* Change shortcut `ao` to `o` (`aggregate-only`)
* Change shortcut `t` to `co` (`context`)
* Change shortcut `ta` to `a` (`context-after`)
* Change shortcut `tb` to `b` (`context-before`)

## 0.1.2 (2020-12-12)

* Enable 'except' for more than two files

## 0.1.1 (2020-10-26)

* Use C# code to compute replacements ([issue](https://github.com/josefpihrt/orang/issues/4))
  * `--replace <CSHARP_EXPRESSION> cs[harp]`
  * `--replace <CSHARP_CODE_FILE_PATH> cs[harp] f[rom-file]`
* Deprecate option `--evaluator` and replace it with `--replace "DllPath,FullTypeName.MethodName" from-dll`
* Add option `--display no-align` to disable alignment of columns
* Add option `--paths`
  * Use this option when you need to specify path(s) at a different than first position

## 0.1.0 (2020-09-01)

* Add shortcut 'b' for option '--attributes-to-skip'
* Add shortcut 'tb' for option 'context-before'
* Add shortcut 'ta' for option 'context-after'

## 0.1.0-rc2 (2020-06-09)

* Change target framework to 'netcoreapp3.0'
* Enable escape sequences in the input (applies to 'replace' command)
* Rename option `--conflict-resolution rename` to `--conflict-resolution suffix` (applies to 'copy', 'move' and 'rename' commands)
* Add option `--conflict-resolution` to 'rename' command
* Verify if new file name contains invalid characters (applies to 'rename' command)
* Add option `-f|--filter` (applies to 'help' command)
* Change option `-m|--max-count`
  * `--max-count <NUM>`: Stop searching after `<NUM>` matching files and/or directories.
  * `--max-count m[atches]=<NUM>`: Stop searching in each file after `<NUM>` matches.
* Add option to recognize escape sequences in the input string (applies to 'match' and 'split' commands)
* Add option to display lines before and after matching line.
  * Add option `--display context=<NUM>` or `-y t=<NUM>`
  * Add option `--display context-before=<NUM>`
  * Add option `--display context-after=<NUM>`
* Rename command 'list-syntax' to 'list-patterns'
* Replace option `-v|--values` with `-v|--verbosity d[etailed]` (applies to 'help' command)
* Add ability to separate matching lines with newline: `--display separator=\n`

## 0.1.0-rc (2020-02-09)

* Remove shortcut 't' from option 'trim-line'
* Add shortcut '-i' for option '--input' (applies to 'match' and 'split' command)
* Add ability to list syntax that matches specified character (applies to 'list-syntax' command)
* Add ability to list syntax that matches specified character (applies to 'list-syntax' command)
* Add 'copy' and 'move' commands
* Add option '--modify' (applies to 'find', 'rename' and 'replace' commands)
* Add pattern options 'starts-with' and 'ends-with'
* Rename pattern option 'whole-input' to 'equals'
* Make extension filter case-insensitive by default
* Display summary when explicitly required even if verbosity is set to 'quiet'
* Count processed files/directories in dry run but display it in gray colors
* Change display of renamed paths
* Require '--name' option only when no path is specified (i.e. current directory is used) (applies to 'delete' command)
* Compare paths based on current OS
* Calculate directory size
* Add option to display only match: `--display path=match`

## 0.1.0-beta (2019-12-28)

* Initial release
