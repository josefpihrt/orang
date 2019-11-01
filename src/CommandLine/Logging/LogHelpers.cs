// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Orang.FileSystem;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal static class LogHelpers
    {
        public static void WriteFileError(Exception ex, string path = null, string basePath = null, ConsoleColors colors = default, string indent = null, Verbosity verbosity = Verbosity.Normal)
        {
            if (colors.IsDefault)
                colors = Colors.Message_Warning;

            string message = ex.Message;

            WriteLine($"{indent}ERROR: {message}", colors, verbosity);

            if (!string.IsNullOrEmpty(path)
                && !string.IsNullOrEmpty(message)
                && !message.Contains(path, StringComparison.OrdinalIgnoreCase))
            {
                Write($"{indent}PATH: ", colors, verbosity);
                WritePath(path, basePath, colors: colors, verbosity: verbosity);
                WriteLine();
            }
#if DEBUG
            WriteLine($"{indent}STACK TRACE:");
            WriteLine(ex.StackTrace);
#endif
        }

        public static void WriteSearchedFilesAndDirectories(SearchTelemetry telemetry, SearchTarget searchTarget)
        {
            if (!ShouldLog(Verbosity.Minimal))
                return;

            WriteLine(Verbosity.Minimal);

            if (searchTarget != SearchTarget.Directories)
            {
                WriteCount("Files searched", telemetry.FileCount, verbosity: Verbosity.Minimal);
            }

            if (searchTarget != SearchTarget.Files)
            {
                if (searchTarget != SearchTarget.Directories)
                    Write("  ", Verbosity.Minimal);

                WriteCount("Directories searched", telemetry.DirectoryCount, verbosity: Verbosity.Minimal);
            }

            if (telemetry.Elapsed != default)
                Write($"  Elapsed Time: {telemetry.Elapsed:mm\\:ss\\.ff}", Verbosity.Minimal);

            WriteLine(Verbosity.Minimal);
        }

        public static void WriteProcessedFilesAndDirectories(
            SearchTelemetry telemetry,
            SearchTarget searchTarget,
            string processedFilesTitle,
            string processedDirectoriesTitle,
            Verbosity verbosity = Verbosity.Minimal)
        {
            if (!ShouldLog(verbosity))
                return;

            WriteLine(verbosity);

            const string filesTitle = "Matching files";
            const string directoriesTitle = "Matching directories";

            bool files = searchTarget != SearchTarget.Directories;
            bool directories = searchTarget != SearchTarget.Files;

            string matchingFileCount = telemetry.MatchingFileCount.ToString("n0");
            string matchingDirectoryCount = telemetry.MatchingDirectoryCount.ToString("n0");
            string processedFileCount = telemetry.ProcessedFileCount.ToString("n0");
            string processedDirectoryCount = telemetry.ProcessedDirectoryCount.ToString("n0");

            int width1 = Math.Max(filesTitle.Length, processedFilesTitle.Length);
            int width2 = Math.Max(matchingFileCount.Length, processedFileCount.Length);
            int width3 = Math.Max(directoriesTitle.Length, processedDirectoriesTitle.Length);
            int width4 = Math.Max(matchingDirectoryCount.Length, processedDirectoryCount.Length);

            if (files)
                WriteCount(filesTitle, matchingFileCount, width1, width2, Colors.Message_OK);

            if (directories)
            {
                if (files)
                    Write("  ", Colors.Message_Change, verbosity);

                WriteCount(directoriesTitle, matchingDirectoryCount, width3, width4, Colors.Message_OK);
            }

            WriteLine(verbosity);

            if (files)
                WriteCount(processedFilesTitle, processedFileCount, width1, width2, Colors.Message_Change);

            if (directories)
            {
                if (files)
                    Write("  ", Colors.Message_Change, verbosity);

                WriteCount(processedDirectoriesTitle, processedDirectoryCount, width3, width4, Colors.Message_Change);
            }

            WriteLine(verbosity);
        }

        public static void WriteGroups(GroupDefinitionCollection groupDefinitions, Dictionary<int, ConsoleColors> colors = null)
        {
            if (!ShouldLog(Verbosity.Normal))
                return;

            if (groupDefinitions.Count == 1
                && groupDefinitions[0].Number == 0)
            {
                return;
            }

            WriteLine(Verbosity.Normal);

            WriteLine("Groups:", Verbosity.Normal);

            int maxWidth = groupDefinitions.Max(f => f.Number).GetDigitCount();

            foreach (GroupDefinition groupDefinition in groupDefinitions.OrderBy(f => f.Number))
            {
                if (groupDefinition.Number == 0)
                    continue;

                ConsoleColors groupColors = default;

                colors?.TryGetValue(groupDefinition.Number, out groupColors);

                Write(" ", Verbosity.Normal);

                string indexText = groupDefinition.Number.ToString();

                Write(' ', maxWidth - indexText.Length, Verbosity.Normal);
                Write(indexText, groupColors, Verbosity.Normal);

                if (indexText != groupDefinition.Name)
                {
                    Write(" ", Verbosity.Normal);
                    Write(groupDefinition.Name, groupColors, Verbosity.Normal);
                }

                WriteLine(Verbosity.Normal);
            }
        }

        public static int WriteCount(string name, int count, in ConsoleColors colors = default, Verbosity verbosity = Verbosity.Quiet)
        {
            return WriteCount(name, count.ToString("n0"), colors, verbosity);
        }

        public static int WriteCount(string name, string count, in ConsoleColors colors = default, Verbosity verbosity = Verbosity.Quiet)
        {
            return WriteCount(name, count, name.Length, count.Length, colors, verbosity);
        }

        internal static int WriteCount(string name, string count, int nameWidth, int countWidth, in ConsoleColors colors, Verbosity verbosity = Verbosity.Quiet)
        {
            if (name.Length > 0)
            {
                Write(name, colors, verbosity);
                Write(": ", colors, verbosity);
            }

            Write(' ', nameWidth - name.Length, verbosity);
            Write(' ', countWidth - count.Length, verbosity);
            Write(count, colors, verbosity);

            return nameWidth + countWidth + 2;
        }

        public static void WritePath(
            in FileSystemFinderResult result,
            string basePath,
            in ConsoleColors colors = default,
            in ConsoleColors matchColors = default,
            string indent = null,
            Verbosity verbosity = Verbosity.Quiet)
        {
            WritePathImpl(
                path: result.Path,
                basePath: basePath,
                colors: colors,
                matchIndex: result.Index,
                matchLength: result.Length,
                matchColors: matchColors,
                indent: indent,
                verbosity: verbosity);
        }

        public static void WritePath(
            string path,
            string basePath = null,
            in ConsoleColors colors = default,
            string indent = null,
            Verbosity verbosity = Verbosity.Quiet)
        {
            WritePathImpl(
                path: path,
                basePath: basePath,
                colors: colors,
                matchColors: default,
                indent: indent,
                verbosity: verbosity);
        }

        private static void WritePathImpl(
            string path,
            string basePath,
            in ConsoleColors colors = default,
            int matchIndex = -1,
            int matchLength = -1,
            in ConsoleColors matchColors = default,
            string indent = null,
            Verbosity verbosity = Verbosity.Quiet)
        {
            if (!ShouldLog(verbosity))
                return;

            Write(indent, verbosity);

            int startIndex = 0;

            if (string.Equals(path, basePath, StringComparison.OrdinalIgnoreCase))
            {
                Debug.Assert(matchIndex == -1);
                Write(".", colors, verbosity);
                return;
            }

            if (basePath != null
                && path.Length > basePath.Length
                && path.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            {
                startIndex = basePath.Length;

                if (FileSystemHelpers.IsDirectorySeparator(path[startIndex]))
                    startIndex++;
            }

            if (matchIndex >= 0
                && !matchColors.IsDefault)
            {
                if (matchIndex < startIndex)
                    startIndex = 0;

                Write(path, startIndex, matchIndex - startIndex, colors: colors, verbosity);
                Write(path, matchIndex, matchLength, matchColors, verbosity);
                Write(path, matchIndex + matchLength, path.Length - matchIndex - matchLength, colors: colors, verbosity);
            }
            else
            {
                Write(path, startIndex, path.Length - startIndex, colors: colors, verbosity);
            }
        }
    }
}
