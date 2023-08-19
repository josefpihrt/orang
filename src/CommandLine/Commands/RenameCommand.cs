// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using Orang.FileSystem;
using Orang.Text.RegularExpressions;

namespace Orang.CommandLine;

internal class RenameCommand : DeleteOrRenameCommand<RenameCommandOptions>
{
    public RenameCommand(RenameCommandOptions options, Logger logger) : base(options, logger)
    {
        Debug.Assert(options.NameFilter?.Invert == false);

        if (_logger.ShouldWrite(Verbosity.Minimal))
        {
            PathWriter = new PathWriter(
                _logger,
                pathColors: Colors.Matched_Path,
                matchColors: (Options.HighlightMatch) ? Colors.Match : default,
                Options.DisplayRelativePath);
        }
    }

    public ConflictResolution ConflictResolution
    {
        get { return Options.ConflictResolution; }
        protected set { Options.ConflictResolution = value; }
    }

    public override bool CanUseResults => false;

    private PathWriter? PathWriter { get; }

    protected override void OnSearchCreating(SearchState search)
    {
        search.SupportsEnumeration = Options.DryRun;

        base.OnSearchCreating(search);
    }

    protected override void ExecuteMatchCore(
        FileMatch fileMatch,
        SearchContext context,
        string? baseDirectoryPath,
        ColumnWidths? columnWidths)
    {
        string indent = GetPathIndent(baseDirectoryPath);

        ReplaceResult result = fileMatch.GetReplaceResult(
            Options.Replacer,
            count: Options.MaxMatchesInFile,
            predicate: NameFilter!.Predicate,
            cancellationToken: context.CancellationToken);

        string newValue = result.NewName;

        if (string.IsNullOrWhiteSpace(newValue))
        {
            _logger.WriteLine($"{indent}New file name cannot be empty or contains only white-space.", Colors.Message_Warning);
            return;
        }

        string path = fileMatch.Path;

        bool changed = string.Compare(
            path,
            fileMatch.NameSpan.Start,
            newValue,
            0,
            Math.Max(fileMatch.NameSpan.Length, newValue.Length),
            StringComparison.Ordinal) != 0;

        bool isInvalidName = changed
            && FileSystemUtilities.ContainsInvalidFileNameChars(newValue);

        string newPath = path.Substring(0, fileMatch.NameSpan.Start)
            + newValue
            + path.Substring(fileMatch.NameSpan.Start + fileMatch.NameSpan.Length);

        if (Options.Interactive
            || (!Options.OmitPath && changed))
        {
            ConsoleColors replaceColors = default;

            if (Options.HighlightReplacement)
                replaceColors = (isInvalidName) ? Colors.InvalidReplacement : Colors.Replacement;

            PathWriter?.WritePath(
                fileMatch,
                result.Items,
                replaceColors,
                baseDirectoryPath,
                indent);

            WriteProperties(context, fileMatch, columnWidths);
            _logger.WriteFilePathEnd(result.Count, result.MaxReason, Options.IncludeCount);
        }

        ListCache<ReplaceItem>.Free(result.Items);

        if (Options.Interactive)
        {
            string newName2 = ConsoleHelpers.ReadUserInput(newValue, indent + "New name: ");

            newPath = newPath.Substring(0, newPath.Length - newValue.Length) + newName2;

            changed = !string.Equals(path, newPath, StringComparison.Ordinal);

            if (changed)
            {
                isInvalidName = FileSystemUtilities.ContainsInvalidFileNameChars(newName2);

                if (isInvalidName)
                {
                    _logger.WriteLine($"{indent}New file name contains invalid character(s).", Colors.Message_Warning);
                    return;
                }
            }
        }

        if (!changed)
            return;

        if (isInvalidName)
        {
            _logger.WriteWarning($"{indent}New file name contains invalid character(s).", verbosity: Verbosity.Detailed);
            return;
        }

        bool fileExists = File.Exists(newPath);
        bool directoryExists = !fileExists && Directory.Exists(newPath);

        if (fileExists
            || directoryExists)
        {
            if (ConflictResolution == ConflictResolution.Skip)
                return;
        }

        string question;
        if (fileExists)
        {
            question = "Rename (and delete existing file)?";
        }
        else if (directoryExists)
        {
            question = "Rename (and delete existing directory)?";
        }
        else
        {
            question = "Rename?";
        }

        if ((fileExists || directoryExists)
            && ConflictResolution == ConflictResolution.Ask
            && !Options.DryRun)
        {
            if (!AskToOverwrite(context, question, indent))
                return;
        }
        else if (Options.AskMode == AskMode.File)
        {
            if (!AskToExecute(context, question, indent))
                return;
        }

        var renamed = false;

        try
        {
            if (!Options.DryRun)
            {
                if (fileExists)
                {
                    File.Delete(newPath);
                }
                else if (directoryExists)
                {
                    Directory.Delete(newPath, recursive: true);
                }

                if (fileMatch.IsDirectory)
                {
                    Directory.Move(path, newPath);
                }
                else if (Options.KeepOriginal)
                {
                    File.Copy(path, newPath);
                }
                else
                {
                    File.Move(path, newPath);
                }

                renamed = true;
            }

            if (fileMatch.IsDirectory)
            {
                context.Telemetry.ProcessedDirectoryCount++;
            }
            else
            {
                context.Telemetry.ProcessedFileCount++;
            }
        }
        catch (Exception ex) when (ex is IOException
            || ex is UnauthorizedAccessException)
        {
            _logger.WriteFileError(ex, indent: indent);
        }

        if (fileMatch.IsDirectory
            && renamed)
        {
            OnDirectoryChanged(new DirectoryChangedEventArgs(path, newPath));
        }
    }

    protected override void WriteSummary(SearchTelemetry telemetry, Verbosity verbosity)
    {
        _logger.WriteSearchedFilesAndDirectories(telemetry, Options.SearchTarget, verbosity);
        _logger.WriteProcessedFilesAndDirectories(
            telemetry,
            Options.SearchTarget,
            "Renamed files",
            "Renamed directories",
            Options.DryRun,
            verbosity);
    }

    private bool AskToOverwrite(SearchContext context, string question, string indent)
    {
        DialogResult result = ConsoleHelpers.Ask(question, indent);

        switch (result)
        {
            case DialogResult.Yes:
                {
                    return true;
                }
            case DialogResult.YesToAll:
                {
                    ConflictResolution = ConflictResolution.Overwrite;
                    return true;
                }
            case DialogResult.No:
            case DialogResult.None:
                {
                    return false;
                }
            case DialogResult.NoToAll:
                {
                    ConflictResolution = ConflictResolution.Skip;
                    return false;
                }
            case DialogResult.Cancel:
                {
                    context.TerminationReason = TerminationReason.Canceled;
                    return false;
                }
            default:
                {
                    throw new InvalidOperationException($"Unknown enum value '{result}'.");
                }
        }
    }
}
