// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine;
using Orang.CommandLine.Annotations;
using Orang.FileSystem;

namespace Orang.CommandLine;

[OptionValueProvider(nameof(AttributesToSkip), OptionValueProviderNames.FileSystemAttributesToSkip)]
internal abstract class FileSystemCommandLineOptions : CommonRegexCommandLineOptions
{
    private FileSystemAttributes FileSystemAttributes { get; set; }

    protected PipeMode? PipeMode { get; set; }

    [AdditionalDescription(" For further information about the syntax see [reference documentation]("
        + "https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.filesystemglobbing.matcher?view=dotnet-plat-ext-7.0#remarks"
        + ").")]
    [Option(
        longName: OptionNames.Include,
        HelpText = "Space separated list of glob patterns to include files and/or folders.",
        MetaValue = "<GLOB>")]
    public IEnumerable<string> Include { get; set; } = null!;

    [AdditionalDescription(" For further information about the syntax see [reference documentation]("
        + "https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.filesystemglobbing.matcher?view=dotnet-plat-ext-7.0#remarks"
        + ").")]
    [Option(
        longName: OptionNames.Exclude,
        HelpText = "Space separated list of glob patterns to exclude files and/or folders.",
        MetaValue = "<GLOB>")]
    public IEnumerable<string> Exclude { get; set; } = null!;

    [HideFromConsoleHelp]
    [Option(
        longName: OptionNames.AfterContext,
        HelpText = "Number of lines to show after matching line.",
        MetaValue = MetaValues.Num,
        Default = -1)]
    public int AfterContext { get; set; }

    [HideFromConsoleHelp]
    [Option(
        longName: OptionNames.AlignColumns,
        HelpText = "Align columns.")]
    public bool AlignColumns { get; set; }

    [HideFromConsoleHelp]
    [Option(
        longName: OptionNames.BeforeContext,
        HelpText = "Number of lines to show before matching line.",
        MetaValue = MetaValues.Num,
        Default = -1)]
    public int BeforeContext { get; set; }

    [Option(
        longName: OptionNames.PathMode,
        HelpText = "Defines which part of a path should be included in the results.",
        MetaValue = MetaValues.PathMode)]
    public string PathMode { get; set; } = null!;

    [Option(
        longName: OptionNames.Count,
        HelpText = "Show number of matches in a file.")]
    public bool Count { get; set; }

    [Option(
        longName: OptionNames.Context,
        HelpText = "Number of lines to show before and after matching line.",
        MetaValue = MetaValues.Num,
        Default = -1)]
    public int Context { get; set; }

    [Option(
#if DEBUG
        shortName: OptionShortNames.LineNumber,
#endif
        longName: OptionNames.LineNumber,
        HelpText = "Include line number.")]
    public bool LineNumber { get; set; }

    [Option(
        longName: OptionNames.MaxDepth,
        HelpText = "Maximum directory depth.",
        MetaValue = MetaValues.Num,
        Default = int.MaxValue)]
    public int MaxDepth { get; set; }

    [HideFromConsoleHelp]
    [Option(
        shortName: OptionShortNames.NoContent,
        longName: OptionNames.NoContent,
        HelpText = "A shortcut for '--" + OptionNames.ContentMode + " omit'.")]
    public bool NoContent { get; set; }

    [HideFromConsoleHelp]
    [Option(
        shortName: OptionShortNames.NoPath,
        longName: OptionNames.NoPath,
        HelpText = "A shortcut for '--" + OptionNames.PathMode + " omit'.")]
    public bool NoPath { get; set; }

    [Value(
        index: 0,
        HelpText = "Path to one or more files and/or directories that should be searched.",
        MetaName = ArgumentMetaNames.Path)]
    public virtual IEnumerable<string> Path { get; set; } = null!;

    [Option(
        shortName: OptionShortNames.Attributes,
        longName: OptionNames.Attributes,
        HelpText = "File attributes that are required.",
        MetaValue = MetaValues.Attributes)]
    public IEnumerable<string> Attributes { get; set; } = null!;

    [Option(
        shortName: OptionShortNames.AttributesToSkip,
        longName: OptionNames.AttributesToSkip,
        HelpText = "File attributes that should be skipped.",
        MetaValue = MetaValues.Attributes)]
    public IEnumerable<string> AttributesToSkip { get; set; } = null!;

    [Option(
        longName: OptionNames.Encoding,
        HelpText = "Encoding to use when a file does not contain BOM. Default encoding is UTF-8.",
        MetaValue = MetaValues.Encoding)]
    public string Encoding { get; set; } = null!;

    [Option(
        shortName: OptionShortNames.Extension,
        longName: OptionNames.Extension,
        HelpText = "A filter for file extensions (case-insensitive by default).",
        MetaValue = MetaValues.ExtensionFilter)]
    public IEnumerable<string> Extension { get; set; } = null!;

    [HideFromConsoleHelp]
    [Option(
        shortName: OptionShortNames.IncludeDirectory,
        longName: OptionNames.IncludeDirectory,
        HelpText = "[deprecated] Regular expression for a directory name.",
        MetaValue = MetaValues.Regex)]
    public IEnumerable<string> IncludeDirectory { get; set; } = null!;

    [Option(
        longName: OptionNames.NoRecurse,
        HelpText = "Do not search subdirectories.")]
    public bool NoRecurse { get; set; }

    [Option(
        longName: OptionNames.Paths,
        HelpText = "Path to one or more files and/or directories that should be searched.",
        MetaValue = MetaValues.Path)]
    public IEnumerable<string> Paths { get; set; } = null!;

    [Option(
        longName: OptionNames.PathsFrom,
        HelpText = "Read the list of paths to search from a file. Paths should be separated by newlines.",
        MetaValue = MetaValues.FilePath)]
    public string PathsFrom { get; set; } = null!;

    [Option(
        longName: OptionNames.Progress,
        HelpText = "Display dot (.) for every hundredth searched file or directory.")]
    public bool Progress { get; set; }

    [Option(
        shortName: OptionShortNames.Properties,
        longName: OptionNames.Properties,
        HelpText = "Display file's properties and optionally filter by that properties.",
        MetaValue = MetaValues.FileProperties)]
    public IEnumerable<string> Properties { get; set; } = null!;

    [Option(
        shortName: OptionShortNames.Sort,
        longName: OptionNames.Sort,
        HelpText = "Sort matched files and directories.",
        MetaValue = MetaValues.SortOptions)]
    public IEnumerable<string> Sort { get; set; } = null!;

    public bool TryParse(FileSystemCommandOptions options, ParseContext context)
    {
        var baseOptions = (CommonRegexCommandOptions)options;

        if (!TryParse(baseOptions, context))
            return false;

        options = (FileSystemCommandOptions)baseOptions;

        if (!TryParsePaths(out ImmutableArray<PathInfo> paths, context))
            return false;

        if (!context.TryParseAsEnumFlags(
            Attributes,
            OptionNames.Attributes,
            out FileSystemAttributes attributes,
            provider: OptionValueProviders.FileSystemAttributesProvider))
        {
            return false;
        }

        if (!context.TryParseAsEnumFlags(
            AttributesToSkip,
            OptionNames.AttributesToSkip,
            out FileSystemAttributes attributesToSkip,
            provider: OptionValueProviders.FileSystemAttributesToSkipProvider))
        {
            return false;
        }

        if (!context.TryParseEncoding(Encoding, out Encoding defaultEncoding, Text.EncodingHelpers.UTF8NoBom))
            return false;

        if (!context.TryParseSortOptions(Sort, OptionNames.Sort, out SortOptions? sortOptions))
            return false;

        if (IncludeDirectory.Any())
        {
            context.WriteWarning($"Option '{OptionNames.GetHelpText(OptionNames.IncludeDirectory)}' has been deprecated "
                + "and will be removed in future versions. "
                + $"Use options '{OptionNames.GetHelpText(OptionNames.Include)}/{OptionNames.GetHelpText(OptionNames.Exclude)}' instead.");
        }

        if (!context.TryParseFilter(
            IncludeDirectory,
            OptionNames.IncludeDirectory,
            OptionValueProviders.PatternOptionsProvider,
            out Matcher? directoryFilter,
            out FileNamePart directoryNamePart,
            allowNull: true,
            namePartProvider: OptionValueProviders.NamePartKindProvider_WithoutExtension))
        {
            return false;
        }

        if (!context.TryParseFilter(
            Extension,
            OptionNames.Extension,
            OptionValueProviders.ExtensionOptionsProvider,
            out Matcher? extensionFilter,
            allowNull: true,
            defaultNamePart: FileNamePart.Extension,
            includedPatternOptions: PatternOptions.List | PatternOptions.Equals | PatternOptions.IgnoreCase))
        {
            return false;
        }

        if (!context.TryParseFileProperties(
            Properties,
            OptionNames.Properties,
            out bool creationTime,
            out bool modifiedTime,
            out bool size,
            out FilterPredicate<DateTime>? creationTimePredicate,
            out FilterPredicate<DateTime>? modifiedTimePredicate,
            out FilterPredicate<long>? sizePredicate))
        {
            return false;
        }

        if ((attributes & FileSystemAttributes.Empty) != 0)
        {
            if ((attributesToSkip & FileSystemAttributes.Empty) != 0)
            {
                string helpValue = OptionValueProviders.FileSystemAttributesProvider
                    .GetValue(nameof(FileSystemAttributes.Empty))
                    .HelpValue;

                context.WriteError($"Value '{helpValue}' cannot be specified both for "
                    + $"'{OptionNames.GetHelpText(OptionNames.Attributes)}' and "
                    + $"'{OptionNames.GetHelpText(OptionNames.AttributesToSkip)}'.");

                return false;
            }

            options.EmptyOption = FileEmptyOption.Empty;
        }
        else if ((attributesToSkip & FileSystemAttributes.Empty) != 0)
        {
            options.EmptyOption = FileEmptyOption.NonEmpty;
        }

        options.Paths = paths;
        options.DirectoryFilter = directoryFilter;
        options.DirectoryNamePart = directoryNamePart;
        options.ExtensionFilter = extensionFilter;
        options.Attributes = GetFileAttributes(attributes);
        options.AttributesToSkip = GetFileAttributes(attributesToSkip);
        options.RecurseSubdirectories = !NoRecurse;
        options.Progress = Progress;
        options.DefaultEncoding = defaultEncoding;
        options.SortOptions = sortOptions;
        options.CreationTimePredicate = creationTimePredicate;
        options.ModifiedTimePredicate = modifiedTimePredicate;
        options.SizePredicate = sizePredicate;
        options.AlignColumns = AlignColumns;
        options.MaxDirectoryDepth = MaxDepth;

        options.FilePropertyOptions = new FilePropertyOptions(
            includeCreationTime: creationTime,
            includeModifiedTime: modifiedTime,
            includeSize: size,
            creationTimePredicate: creationTimePredicate?.Predicate,
            modifiedTimePredicate: modifiedTimePredicate?.Predicate,
            sizePredicate: sizePredicate?.Predicate
            );

        FileSystemAttributes = attributes;

        options.GlobFilter = GlobMatcher.TryCreate(Include, Exclude);

        return true;
    }

    protected virtual bool TryParsePaths(out ImmutableArray<PathInfo> paths, ParseContext context)
    {
        paths = ImmutableArray<PathInfo>.Empty;

        if (Path.Any()
            && !TryEnsureFullPath(Path, PathOrigin.Argument, context, out paths))
        {
            return false;
        }

        if (Paths.Any())
        {
            if (!TryEnsureFullPath(Paths, PathOrigin.Option, context, out ImmutableArray<PathInfo> paths2))
                return false;

            paths = paths.AddRange(paths2);
        }

        ImmutableArray<PathInfo> pathsFromFile = ImmutableArray<PathInfo>.Empty;

        if (PathsFrom is not null)
        {
            if (!FileSystemUtilities.TryReadAllText(PathsFrom, out string? content, ex => context.WriteError(ex)))
                return false;

            IEnumerable<string> lines = TextHelpers.ReadLines(content).Where(f => !string.IsNullOrWhiteSpace(f));

            if (!TryEnsureFullPath(lines, PathOrigin.File, context, out pathsFromFile))
                return false;

            paths = paths.AddRange(pathsFromFile);
        }

        if (Console.IsInputRedirected
            && PipeMode == CommandLine.PipeMode.Paths)
        {
            if (!TryEnsureFullPath(
                ConsoleHelpers.ReadRedirectedInputAsLines().Where(f => !string.IsNullOrEmpty(f)),
                PathOrigin.RedirectedInput,
                context,
                out ImmutableArray<PathInfo> pathsFromInput))
            {
                return false;
            }

            paths = paths.AddRange(pathsFromInput);
        }

        if (paths.IsEmpty)
            paths = ImmutableArray.Create(new PathInfo(Environment.CurrentDirectory, PathOrigin.CurrentDirectory));

        return true;
    }

    private static bool TryEnsureFullPath(
        IEnumerable<string> paths,
        PathOrigin kind,
        ParseContext context,
        out ImmutableArray<PathInfo> fullPaths)
    {
        ImmutableArray<PathInfo>.Builder builder = ImmutableArray.CreateBuilder<PathInfo>();

        foreach (string path in paths)
        {
            if (!context.TryEnsureFullPath(path, out string? fullPath))
            {
                fullPaths = default;
                return false;
            }

            builder.Add(new PathInfo(fullPath, kind));
        }

        fullPaths = builder.ToImmutableArray();
        return true;
    }

    protected static FileAttributes GetFileAttributes(FileSystemAttributes attributes)
    {
        FileAttributes fileAttributes = 0;

        if ((attributes & FileSystemAttributes.ReadOnly) != 0)
            fileAttributes |= FileAttributes.ReadOnly;

        if ((attributes & FileSystemAttributes.Hidden) != 0)
            fileAttributes |= FileAttributes.Hidden;

        if ((attributes & FileSystemAttributes.System) != 0)
            fileAttributes |= FileAttributes.System;

        if ((attributes & FileSystemAttributes.Archive) != 0)
            fileAttributes |= FileAttributes.Archive;

        if ((attributes & FileSystemAttributes.Device) != 0)
            fileAttributes |= FileAttributes.Device;

        if ((attributes & FileSystemAttributes.Normal) != 0)
            fileAttributes |= FileAttributes.Normal;

        if ((attributes & FileSystemAttributes.Temporary) != 0)
            fileAttributes |= FileAttributes.Temporary;

        if ((attributes & FileSystemAttributes.SparseFile) != 0)
            fileAttributes |= FileAttributes.SparseFile;

        if ((attributes & FileSystemAttributes.ReparsePoint) != 0)
            fileAttributes |= FileAttributes.ReparsePoint;

        if ((attributes & FileSystemAttributes.Compressed) != 0)
            fileAttributes |= FileAttributes.Compressed;

        if ((attributes & FileSystemAttributes.Offline) != 0)
            fileAttributes |= FileAttributes.Offline;

        if ((attributes & FileSystemAttributes.NotContentIndexed) != 0)
            fileAttributes |= FileAttributes.NotContentIndexed;

        if ((attributes & FileSystemAttributes.Encrypted) != 0)
            fileAttributes |= FileAttributes.Encrypted;

        if ((attributes & FileSystemAttributes.IntegrityStream) != 0)
            fileAttributes |= FileAttributes.IntegrityStream;

        if ((attributes & FileSystemAttributes.NoScrubData) != 0)
            fileAttributes |= FileAttributes.NoScrubData;

        return fileAttributes;
    }

    protected SearchTarget GetSearchTarget()
    {
        if ((FileSystemAttributes & FileSystemAttributes.File) != 0)
        {
            return ((FileSystemAttributes & FileSystemAttributes.Directory) != 0)
                ? SearchTarget.All
                : SearchTarget.Files;
        }

        if ((FileSystemAttributes & FileSystemAttributes.Directory) != 0)
            return SearchTarget.Directories;

        return SearchTarget.Files;
    }
}
