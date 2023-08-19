// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using CommandLine;
using Orang.CommandLine.Annotations;
using Orang.FileSystem;

namespace Orang.CommandLine;

[Verb("sync", HelpText = "Synchronizes content of two directories.")]
[CommandGroup("File System", 1)]
internal sealed class SyncCommandLineOptions : CommonCopyCommandLineOptions
{
    [Value(
        index: 0,
        HelpText = "Path to a first directory to be synchronized and optionally a second directory.",
        MetaName = ArgumentMetaNames.Path)]
    public override IEnumerable<string> Path { get; set; } = null!;

    [Option(
        longName: OptionNames.Ask,
        HelpText = "Ask for a permission to synchronize file or directory.")]
    public bool Ask { get; set; }

    [Option(
        longName: OptionNames.Conflict,
        HelpText = "Action to choose if a file or directory exists in one directory and it is missing in the second directory.",
        MetaValue = MetaValues.SyncConflictResolution)]
    public string Conflict { get; set; } = null!;
#if DEBUG
    [Option(
        longName: OptionNames.DetectRename,
        HelpText = "Detect if the file was only renamed.")]
    public bool DetectRename { get; set; }
#endif
    [Option(
        shortName: OptionShortNames.DryRun,
        longName: OptionNames.DryRun,
        HelpText = "Display which files or directories should be copied/deleted "
            + "but do not actually copy/delete any file or directory.")]
    public bool DryRun { get; set; }

    [Option(
        longName: OptionNames.Second,
        HelpText = "A directory to be synchronized with a first directory. It can be also specified as a last unnamed parameter.",
        MetaValue = MetaValues.DirectoryPath)]
    public string Second { get; set; } = null!;

    public bool TryParse(SyncCommandOptions options, ParseContext context)
    {
        var baseOptions = (CommonCopyCommandOptions)options;

        if (!TryParse(baseOptions, context))
            return false;

        options = (SyncCommandOptions)baseOptions;

        if (!context.TryParseTargetDirectory(Second, out string? second, options, nameof(Second), OptionNames.Second))
            return false;

        if (!context.TryParseAsEnumFlags(
            Compare,
            OptionNames.Compare,
            out FileCompareProperties compareProperties,
            FileCompareProperties.Attributes
                | FileCompareProperties.Content
                | FileCompareProperties.ModifiedTime
                | FileCompareProperties.Size,
            OptionValueProviders.FileCompareOptionsProvider))
        {
            return false;
        }

        if (!context.TryParseAsEnum(
            Conflict,
            OptionNames.Conflict,
            out SyncConflictResolution conflictResolution,
            defaultValue: SyncConflictResolution.FirstWins,
            provider: OptionValueProviders.SyncConflictResolutionProvider))
        {
            return false;
        }

        options.SearchTarget = SearchTarget.All;
        options.CompareProperties = compareProperties;
        options.DryRun = DryRun;
        options.Target = second;
        options.ConflictResolution = conflictResolution;
        options.AskMode = (Ask) ? AskMode.File : AskMode.None;
#if DEBUG
        options.DetectRename = DetectRename;
#endif
        return true;
    }
}
