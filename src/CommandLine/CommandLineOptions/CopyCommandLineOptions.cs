// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using CommandLine;
using Orang.CommandLine.Annotations;
using Orang.FileSystem;

namespace Orang.CommandLine;

[Verb("copy", HelpText = "Searches the file system for files and directories and copy them to a destination directory.")]
[CommandGroup("File System", 1)]
internal sealed class CopyCommandLineOptions : CommonCopyCommandLineOptions
{
    [Value(
        index: 0,
        HelpText = "Path to one or more source directories and optionally a target directory.",
        MetaName = ArgumentMetaNames.Path)]
    public override IEnumerable<string> Path { get; set; } = null!;

    [Option(
        longName: OptionNames.Ask,
        HelpText = "Ask for a permission to copy file or directory.")]
    public bool Ask { get; set; }

    [Option(
        longName: OptionNames.Conflict,
        HelpText = "Defines how to resolve conflict when a file/directory already exists.",
        MetaValue = MetaValues.ConflictResolution)]
    public string Conflict { get; set; } = null!;

    [Option(
        shortName: OptionShortNames.DryRun,
        longName: OptionNames.DryRun,
        HelpText = "Display which files/directories should be copied "
            + "but do not actually copy any file/directory.")]
    public bool DryRun { get; set; }

    [Option(
        longName: OptionNames.Flat,
        HelpText = "Copy files directly into target directory.")]
    public bool Flat { get; set; }

    [Option(
        longName: OptionNames.Target,
        HelpText = "A directory to copy files and directories to. It can be also specified as a last unnamed parameter.",
        MetaValue = MetaValues.DirectoryPath)]
    public string Target { get; set; } = null!;

    public bool TryParse(CopyCommandOptions options, ParseContext context)
    {
        var baseOptions = (CommonCopyCommandOptions)options;

        if (!TryParse(baseOptions, context))
            return false;

        options = (CopyCommandOptions)baseOptions;

        if (!context.TryParseAsEnumFlags(
            Compare,
            OptionNames.Compare,
            out FileCompareProperties compareProperties,
            FileCompareProperties.None,
            OptionValueProviders.FileCompareOptionsProvider))
        {
            return false;
        }

        if (!context.TryParseTargetDirectory(Target, out string? target, options, nameof(Target), OptionNames.Target))
            return false;

        if (!context.TryParseAsEnum(
            Conflict,
            OptionNames.Conflict,
            out ConflictResolution conflictResolution,
            defaultValue: ConflictResolution.Ask,
            provider: OptionValueProviders.ConflictResolutionProvider))
        {
            return false;
        }

        options.CompareProperties = compareProperties;
        options.DryRun = DryRun;
        options.Flat = Flat;
        options.ConflictResolution = conflictResolution;
        options.Target = target;
        options.AskMode = (Ask) ? AskMode.File : AskMode.None;

        return true;
    }
}
