---
sidebar_position: 2
sidebar_label: How to's
---

# How to's

### How to exclude directory from a search

Sample: Exclude directories that starts with dot (.) from a search.

```
orang find --include-directory "^\." negative
```
```
orang find -i "^\." e
```

### How to pass option value that starts with dash (-)

The [library](https://github.com/commandlineparser/commandline) that is used to parse command line argument does not allow to pass option value that starts with dash (-).
This problem is discussed in [this](https://github.com/commandlineparser/commandline/issues/300) issue.
As a workaround is it recommended to either escape dash character or to use `--` syntax.

```
orang find --content "\-" --replacement -- "-"
```
```
orang find -c "\-" -r -- "-"
```

