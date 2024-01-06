// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Orang.FileSystem;
using Orang.Text.RegularExpressions;

namespace Orang;

internal static class CommandLineExtensions
{
    public static void WriteDeprecatedWarning(
        this Logger logger,
        string message,
        ConsoleColors colors = default,
        Verbosity verbosity = Verbosity.Minimal)
    {
        logger.ConsoleOut.ErrorWriter.WriteLine(message, (colors.IsDefault) ? Colors.Message_Warning : colors, verbosity);
    }

    public static void WriteWarning(
        this Logger logger,
        string message,
        ConsoleColors colors = default,
        Verbosity verbosity = Verbosity.Minimal)
    {
        if (colors.IsDefault)
            colors = Colors.Message_Warning;

        logger.WriteLine(message, colors, verbosity);
    }

    public static void WriteFilePathEnd(this Logger logger, int count, MaxReason maxReason, bool includeCount)
    {
        Verbosity verbosity = logger.ConsoleOut.Verbosity;

        if (count >= 0)
        {
            if (verbosity >= Verbosity.Detailed
                || includeCount)
            {
                verbosity = (includeCount) ? Verbosity.Minimal : Verbosity.Detailed;

                logger.ConsoleOut.Write("  ", Colors.Message_OK, verbosity);
                logger.ConsoleOut.Write(count.ToString("n0"), Colors.Message_OK, verbosity);
                logger.ConsoleOut.WriteIf(maxReason == MaxReason.CountExceedsMax, "+", Colors.Message_OK, verbosity);
            }
        }

        logger.ConsoleOut.WriteLine(Verbosity.Minimal);

        if (logger.Out is not null)
        {
            verbosity = logger.Out.Verbosity;

            if (verbosity >= Verbosity.Detailed
                || includeCount)
            {
                verbosity = (includeCount) ? Verbosity.Minimal : Verbosity.Detailed;

                logger.Out.Write("  ", verbosity);
                logger.Out.Write(count.ToString("n0"), verbosity);
                logger.Out.WriteIf(maxReason == MaxReason.CountExceedsMax, "+", verbosity);
            }

            logger.Out.WriteLine(Verbosity.Minimal);
        }
    }

    public static void WriteFileError(
        this Logger logger,
        Exception ex,
        string? path = null,
        string? indent = null,
        Verbosity verbosity = Verbosity.Normal)
    {
        string message = ex.Message;

        logger.WriteLine($"{indent}ERROR: {message}", Colors.Message_Warning, verbosity);

        if (!string.IsNullOrEmpty(path)
            && !string.IsNullOrEmpty(message)
            && !message.Contains(path, FileSystemUtilities.Comparison))
        {
            logger.WriteLine($"{indent}PATH: {path}", Colors.Message_Warning, verbosity);
        }
#if DEBUG
        logger.WriteLine($"{indent}STACK TRACE:", verbosity);
        logger.WriteLine(ex.StackTrace, verbosity);
#endif
    }

    public static void WriteSearchedFilesAndDirectories(
        this Logger logger,
        SearchTelemetry telemetry,
        SearchTarget searchTarget,
        Verbosity verbosity = Verbosity.Detailed)
    {
        if (!logger.ShouldWrite(verbosity))
            return;

        logger.WriteLine(verbosity);

        if (searchTarget != SearchTarget.Directories)
        {
            logger.WriteCount("Files searched", telemetry.FileCount, verbosity: verbosity);
        }

        if (searchTarget != SearchTarget.Files)
        {
            if (searchTarget != SearchTarget.Directories)
                logger.Write("  ", verbosity);

            logger.WriteCount("Directories searched", telemetry.DirectoryCount, verbosity: verbosity);
        }

        if (telemetry.FilesTotalSize > 0)
        {
            if (searchTarget == SearchTarget.Files)
            {
                logger.WriteCount("  Files total size", telemetry.FilesTotalSize, verbosity: verbosity);
            }
            else if (searchTarget == SearchTarget.Directories)
            {
                logger.WriteCount("  Directories total size", telemetry.FilesTotalSize, verbosity: verbosity);
            }
            else if (searchTarget == SearchTarget.All)
            {
                logger.WriteCount("  Total size", telemetry.FilesTotalSize, verbosity: verbosity);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        if (telemetry.Elapsed != default)
            logger.Write($"  Elapsed time: {telemetry.Elapsed:mm\\:ss\\.ff}", verbosity);

        logger.WriteLine(verbosity);
    }

    public static void WriteProcessedFilesAndDirectories(
        this Logger logger,
        SearchTelemetry telemetry,
        SearchTarget searchTarget,
        string processedFilesTitle,
        string processedDirectoriesTitle,
        bool dryRun,
        Verbosity verbosity = Verbosity.Detailed)
    {
        if (!logger.ShouldWrite(verbosity))
            return;

        logger.WriteLine(verbosity);

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

        ConsoleColors colors = Colors.Message_OK;

        if (files)
            WriteCount(logger, filesTitle, matchingFileCount, width1, width2, colors, verbosity);

        if (directories)
        {
            if (files)
                logger.Write("  ", colors, verbosity);

            WriteCount(logger, directoriesTitle, matchingDirectoryCount, width3, width4, colors, verbosity);
        }

        logger.WriteLine(verbosity);

        colors = (dryRun) ? Colors.Message_DryRun : Colors.Message_Change;

        if (files)
            WriteCount(logger, processedFilesTitle, processedFileCount, width1, width2, colors, verbosity);

        if (directories)
        {
            if (files)
                logger.Write("  ", colors, verbosity);

            WriteCount(logger, processedDirectoriesTitle, processedDirectoryCount, width3, width4, colors, verbosity);
        }

        logger.WriteLine(verbosity);
    }

    public static void WriteGroups(
        this Logger logger,
        GroupDefinitionCollection groupDefinitions,
        Dictionary<int, ConsoleColors>? colors = null,
        Verbosity verbosity = Verbosity.Detailed)
    {
        if (!logger.ShouldWrite(verbosity))
            return;

        if (groupDefinitions.Count == 1
            && groupDefinitions[0].Number == 0)
        {
            return;
        }

        logger.WriteLine(verbosity);

        logger.WriteLine("Groups:", verbosity);

        int maxWidth = groupDefinitions.Max(f => f.Number).GetDigitCount();

        foreach (GroupDefinition groupDefinition in groupDefinitions.OrderBy(f => f.Number))
        {
            if (groupDefinition.Number == 0)
                continue;

            ConsoleColors groupColors = default;

            colors?.TryGetValue(groupDefinition.Number, out groupColors);

            logger.Write(" ", verbosity);

            string indexText = groupDefinition.Number.ToString();

            logger.Write(' ', maxWidth - indexText.Length, verbosity);
            logger.Write(indexText, groupColors, verbosity);

            if (indexText != groupDefinition.Name)
            {
                logger.Write(" ", verbosity);
                logger.Write(groupDefinition.Name, groupColors, verbosity);
            }

            logger.WriteLine(verbosity);
        }
    }

    public static int WriteCount(
        this Logger logger,
        string name,
        int count,
        in ConsoleColors colors = default,
        Verbosity verbosity = Verbosity.Quiet)
    {
        return WriteCount(logger, name, count.ToString("n0"), colors, verbosity);
    }

    public static int WriteCount(
        this Logger logger,
        string name,
        long count,
        in ConsoleColors colors = default,
        Verbosity verbosity = Verbosity.Quiet)
    {
        return WriteCount(logger, name, count.ToString("n0"), colors, verbosity);
    }

    public static int WriteCount(
        this Logger logger,
        string name,
        string count,
        in ConsoleColors colors = default,
        Verbosity verbosity = Verbosity.Quiet)
    {
        return WriteCount(logger, name, count, name.Length, count.Length, colors, verbosity);
    }

    internal static int WriteCount(
        this Logger logger,
        string name,
        string count,
        int nameWidth,
        int countWidth,
        in ConsoleColors colors,
        Verbosity verbosity = Verbosity.Quiet)
    {
        if (name.Length > 0)
        {
            logger.Write(name, colors, verbosity);
            logger.Write(": ", colors, verbosity);
        }

        logger.Write(' ', nameWidth - name.Length, verbosity);
        logger.Write(' ', countWidth - count.Length, verbosity);
        logger.Write(count, colors, verbosity);

        return nameWidth + countWidth + 2;
    }

    internal static void WriteValue(
        this Logger logger,
        string value,
        string? label = null,
        int valueWidth = -1,
        int labelWidth = -1,
        ConsoleColors colors = default,
        Verbosity verbosity = Verbosity.Quiet)
    {
        int labelLength = label?.Length ?? 0;

        if (labelLength > 0)
        {
            logger.Write(label, colors, verbosity);
            logger.Write(": ", colors, verbosity);
        }

        int labelPadding = labelWidth - labelLength;

        if (labelPadding > 0)
            logger.Write(' ', labelPadding, verbosity);

        int valuePadding = valueWidth - value.Length;

        if (valuePadding > 0)
            logger.Write(' ', valuePadding, verbosity);

        logger.Write(value, colors, verbosity);
    }

    public static void WriteError(
        this Logger logger,
        Exception exception,
        string? message = null)
    {
        logger.WriteError(message ?? exception.Message);

        const Verbosity verbosity =
#if DEBUG
            Verbosity.Quiet;
#else
            Verbosity.Diagnostic;
#endif
        if (logger.ConsoleOut.Verbosity >= verbosity)
            logger.ConsoleOut.ErrorWriter.WriteLine(exception.ToString());

        if (logger.Out?.Verbosity >= verbosity)
            logger.Out.ErrorWriter.WriteLine(exception.ToString());
    }

    public static void WriteError(
        this Logger logger,
        string message)
    {
        logger.ConsoleOut.ErrorWriter.WriteLine(message);
        logger.Out?.ErrorWriter.WriteLine(message);
    }

    public static IEnumerable<string> Modify(
        this IEnumerable<string> values,
        ModifyOptions options,
        ModifyFunctions? filter = null)
    {
        return options.Modify(values, filter);
    }

    public static OperationCanceledException? GetOperationCanceledException(this AggregateException aggregateException)
    {
        OperationCanceledException? operationCanceledException = null;

        foreach (Exception ex in aggregateException.InnerExceptions)
        {
            if (ex is OperationCanceledException operationCanceledException2)
            {
                if (operationCanceledException is null)
                    operationCanceledException = operationCanceledException2;
            }
            else if (ex is AggregateException aggregateException2)
            {
                foreach (Exception ex2 in aggregateException2.InnerExceptions)
                {
                    if (ex2 is OperationCanceledException operationCanceledException3)
                    {
                        if (operationCanceledException is null)
                            operationCanceledException = operationCanceledException3;
                    }
                    else
                    {
                        return null;
                    }
                }

                return operationCanceledException;
            }
            else
            {
                return null;
            }
        }

        return null;
    }
}
