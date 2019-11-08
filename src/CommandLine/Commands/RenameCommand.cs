// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Orang.FileSystem;
using static Orang.CommandLine.LogHelpers;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal class RenameCommand : CommonFindCommand<RenameCommandOptions>, INotifyDirectoryChanged
    {
        public RenameCommand(RenameCommandOptions options) : base(options)
        {
        }

        public event EventHandler<DirectoryChangedEventArgs> DirectoryChanged;

        protected override void ExecuteDirectory(string directoryPath, SearchContext context, FileSystemFinderProgressReporter progress)
        {
            SearchTelemetry telemetry = context.Telemetry;
            string basePath = (Options.PathDisplayStyle == PathDisplayStyle.Full) ? null : directoryPath;
            string indent = (Options.PathDisplayStyle == PathDisplayStyle.Relative) ? Options.Indent : "";

            foreach (FileSystemFinderResult result in FileSystemHelpers.Find(directoryPath, Options, progress, notifyDirectoryChanged: this, canEnumerate: Options.DryRun, context.CancellationToken))
            {
                string path = result.Path;

                if (result.IsDirectory)
                {
                    telemetry.MatchingDirectoryCount++;
                }
                else
                {
                    if (Options.ContentFilter != null)
                    {
                        string input = ReadFile(result.Path, path, Options.DefaultEncoding, progress, indent);

                        if (input == null)
                            continue;

                        if (!Options.ContentFilter.IsMatch(input))
                            continue;
                    }

                    telemetry.MatchingFileCount++;
                }

                NamePart part = result.Part;

                Debug.Assert(path.StartsWith(directoryPath, StringComparison.OrdinalIgnoreCase), $"{directoryPath}\r\n{path}");

                EndProgress(progress);

                WritePath(result, basePath, colors: Colors.Matched_Path, matchColors: (Options.HighlightMatch) ? Colors.Match : default, indent: indent);
                WriteLine();

                Match match = Options.NameFilter.Regex.Match(path.Substring(part.Index, part.Length));

                string replacement = (Options.MatchEvaluator != null)
                    ? Options.MatchEvaluator(match)
                    : match.Result(Options.Replacement);

                int index = part.Index + match.Index;
                int endIndex = index + match.Length;

                string newPath = path.Remove(index) + replacement + path.Substring(endIndex);

                int fileNameIndex = part.GetFileNameIndex();

                int indentCount = fileNameIndex;

                if (Options.PathDisplayStyle == PathDisplayStyle.Relative
                    && path.StartsWith(directoryPath, StringComparison.OrdinalIgnoreCase))
                {
                    indentCount -= directoryPath.Length;

                    if (fileNameIndex > 0
                        && FileSystemHelpers.IsDirectorySeparator(path[fileNameIndex - 1]))
                    {
                        indentCount--;
                    }
                }

                Write(' ', indentCount + indent.Length);
                Write(path, fileNameIndex, index - fileNameIndex);
                Write(replacement, (Options.HighlightReplacement) ? Colors.Replacement : default);
                Write(path, endIndex, path.Length - endIndex);

                if (string.Equals(path, newPath, StringComparison.Ordinal))
                {
                    WriteLine(" NO CHANGE", Colors.Message_Warning);
                    continue;
                }

                WriteLine();

                bool success = false;

                if (!Options.DryRun
                    && ShouldRename())
                {
                    try
                    {
                        if (result.IsDirectory)
                        {
                            Directory.Move(path, newPath);
                        }
                        else
                        {
                            File.Move(path, newPath);
                        }

                        if (result.IsDirectory)
                        {
                            telemetry.ProcessedDirectoryCount++;
                        }
                        else
                        {
                            telemetry.ProcessedFileCount++;
                        }

                        success = true;
                    }
                    catch (Exception ex) when (ex is IOException
                        || ex is UnauthorizedAccessException)
                    {
                        WriteFileError(ex, indent: indent);
                    }
                }

                if (Options.DryRun
                    || success)
                {
                    context.Output?.WriteLine(newPath);
                }

                if (result.IsDirectory
                    && success)
                {
                    OnDirectoryChanged(new DirectoryChangedEventArgs(path, newPath));
                }

                if (context.State == SearchState.Canceled)
                    break;

                if (Options.MaxMatchingFiles == telemetry.MatchingFileCount + telemetry.MatchingDirectoryCount)
                {
                    context.State = SearchState.MaxReached;
                    break;
                }
            }

            telemetry.SearchedDirectoryCount = progress.SearchedDirectoryCount;
            telemetry.FileCount = progress.FileCount;
            telemetry.DirectoryCount = progress.DirectoryCount;

            bool ShouldRename()
            {
                try
                {
                    return ConsoleHelpers.QuestionIf(Options.Ask, "Rename?", indent);
                }
                catch (OperationCanceledException)
                {
                    context.State = SearchState.Canceled;
                    return false;
                }
            }
        }

        protected virtual void OnDirectoryChanged(DirectoryChangedEventArgs e)
        {
            DirectoryChanged?.Invoke(this, e);
        }

        protected override void WriteSummary(SearchTelemetry telemetry)
        {
            WriteSearchedFilesAndDirectories(telemetry, Options.SearchTarget);
            WriteProcessedFilesAndDirectories(telemetry, Options.SearchTarget, "Renamed files", "Renamed directories");
        }
    }
}
