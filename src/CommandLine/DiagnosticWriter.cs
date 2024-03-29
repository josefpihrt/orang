﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CommandLine;
using Orang.FileSystem;

namespace Orang.CommandLine;

internal class DiagnosticWriter
{
    private readonly Logger _logger;

    public DiagnosticWriter(Logger logger)
    {
        _logger = logger;
        ValueWriter = new ValueWriter(new ContentTextWriter(_logger, Verbosity.Diagnostic), includeEndingIndent: false);
    }

    public Verbosity Verbosity { get; } = Verbosity.Diagnostic;

    private ConsoleColors ValueColors { get; } = new(ConsoleColor.Cyan);

    private ConsoleColors NullValueColors { get; } = new(ConsoleColor.DarkGray);

    private ValueWriter ValueWriter { get; }

    private OutputSymbols Symbols_Character { get; } = OutputSymbols.Create(HighlightOptions.Character);

    private OutputSymbols Symbols_Newline { get; } = OutputSymbols.Create(HighlightOptions.Newline);

    internal void WriteParameters(AbstractCommandLineOptions commandLineOptions)
    {
        Type type = commandLineOptions.GetType();

        var values = new List<(int index, object? value)>();
        var options = new List<(string name, object? value)>();

        foreach (PropertyInfo propertyInfo in type.GetRuntimeProperties())
        {
            OptionAttribute? optionAttribute = propertyInfo.GetCustomAttribute<OptionAttribute>();

            if (optionAttribute is not null)
            {
                object? value = propertyInfo.GetValue(commandLineOptions);

                options.Add((optionAttribute.LongName, value));
            }
            else
            {
                ValueAttribute? valueAttribute = propertyInfo.GetCustomAttribute<ValueAttribute>();

                if (valueAttribute is not null)
                {
                    object? value = propertyInfo.GetValue(commandLineOptions);

                    values.Add((valueAttribute.Index, value));
                }
            }
        }

        foreach ((int index, object? value) in values.OrderBy(f => f.index))
            WriteParameterValue(index.ToString(), value);

        foreach ((string name, object? value) in options.OrderBy(f => f.name))
            WriteParameterValue(name, value);
    }

    private void WriteParameterValue(string name, object? value)
    {
        WriteName(name);

        if (value is null)
        {
            WriteNullValue();
        }
        else if (value is bool boolValue)
        {
            if (boolValue)
            {
                WriteIndent();
                Write("true");
            }
            else
            {
                WriteNullValue();
            }
        }
        else if (value is int intValue)
        {
            WriteIndent();
            Write(intValue.ToString());
        }
        else if (value is string stringValue)
        {
            if (stringValue.Length == 0)
            {
                WriteNullValue();
            }
            else
            {
                WriteIndent();
                Write(stringValue);
            }
        }
        else if (value is IEnumerable<string> values)
        {
            using (IEnumerator<string> en = values.GetEnumerator())
            {
                if (en.MoveNext())
                {
                    WriteIndent();

                    while (true)
                    {
                        Write(en.Current);

                        if (en.MoveNext())
                        {
                            _logger.Write(", ", Verbosity);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    WriteNullValue();
                }
            }
        }
        else
        {
            Debug.Fail(value.GetType().ToString());
        }

        WriteLine();
    }

    internal void WriteRegexCreateCommand(RegexCreateCommandOptions options)
    {
        WriteOption("input", options.Input);
        WriteOption("pattern options", options.PatternOptions);
        WriteOption("separator", options.Separator);
    }

    internal void WriteCopyCommand(CopyCommandOptions options)
    {
        WriteOption("align columns", options.AlignColumns);
#if DEBUG
        WriteOption("allowed time diff", options.AllowedTimeDiff);
#endif
        WriteOption("ask", options.AskMode);
        WriteOption("attributes", options.Attributes);
        WriteOption("attributes to skip", options.AttributesToSkip);
        WriteOption("compare", options.CompareProperties);
        WriteOption("conflict resolution", options.ConflictResolution);
        WriteFilter("content filter", options.ContentFilter);
#if DEBUG
        WriteOption("content indent", options.Format.Indent);
#endif
        WriteOption("content mode", options.Format.ContentDisplayStyle);
#if DEBUG
        WriteOption("content separator", options.Format.Separator);
#endif
        WriteContext("context", options.Format.LineContext);
        WriteOption("count", options.Format.Includes(DisplayParts.Count));
        WriteEncoding("default encoding", options.DefaultEncoding);
        WriteFilter("directory filter", options.DirectoryFilter, options.DirectoryNamePart);
        WriteOption("dry run", options.DryRun);
        WriteOption("empty", options.EmptyOption);
        WriteFilter("extension filter", options.ExtensionFilter);
        WriteFilePropertyFilter(
            "file properties",
            options.SizePredicate,
            options.CreationTimePredicate,
            options.ModifiedTimePredicate);
        WriteOption("flat", options.Flat);
        WriteOption("highlight options", options.HighlightOptions);
        WriteOption("line number", options.Format.Includes(LineDisplayOptions.IncludeLineNumber));
        WriteOption("max matching files", options.MaxMatchingFiles);
        WriteOption("max matches in file", options.MaxMatchesInFile);
        WriteFilter("name filter", options.NameFilter, options.NamePart);
#if DEBUG
        WriteOption("no compare attributes", options.NoCompareAttributes);
#endif
        WriteOption("path mode", options.Format.PathDisplayStyle);
        WritePaths("paths", options.Paths);
        WriteOption("progress", options.Progress);
        WriteProperties(options);
        WriteOption("recurse subdirectories", options.RecurseSubdirectories);
        WriteOption("search target", options.SearchTarget);
        WriteSortOptions("sort", options.SortOptions);
        WriteOption("summary", options.Format.Includes(DisplayParts.Summary));
        WriteOption("target", options.Target);
    }

    internal void WriteDeleteCommand(DeleteCommandOptions options)
    {
        WriteOption("align columns", options.AlignColumns);
        WriteOption("ask", options.AskMode == AskMode.File);
        WriteOption("attributes", options.Attributes);
        WriteOption("attributes to skip", options.AttributesToSkip);
        WriteFilter("content filter", options.ContentFilter);
#if DEBUG
        WriteOption("content indent", options.Format.Indent);
#endif
        WriteOption("content mode", options.Format.ContentDisplayStyle);
        WriteOption("content only", options.ContentOnly);
#if DEBUG
        WriteOption("content separator", options.Format.Separator);
#endif
        WriteContext("context", options.Format.LineContext);
        WriteOption("count", options.Format.Includes(DisplayParts.Count));
        WriteEncoding("default encoding", options.DefaultEncoding);
        WriteFilter("directory filter", options.DirectoryFilter, options.DirectoryNamePart);
        WriteOption("dry run", options.DryRun);
        WriteOption("empty", options.EmptyOption);
        WriteFilter("extension filter", options.ExtensionFilter);
        WriteFilePropertyFilter(
            "file properties",
            options.SizePredicate,
            options.CreationTimePredicate,
            options.ModifiedTimePredicate);
        WriteOption("highlight options", options.HighlightOptions);
        WriteOption("including bom", options.IncludingBom);
        WriteOption("line number", options.Format.Includes(LineDisplayOptions.IncludeLineNumber));
        WriteOption("max matching files", options.MaxMatchingFiles);
        WriteFilter("name filter", options.NameFilter, options.NamePart);
        WriteOption("path mode", options.Format.PathDisplayStyle);
        WritePaths("paths", options.Paths);
        WriteOption("progress", options.Progress);
        WriteProperties(options);
        WriteOption("recurse subdirectories", options.RecurseSubdirectories);
        WriteOption("search target", options.SearchTarget);
        WriteSortOptions("sort", options.SortOptions);
        WriteOption("summary", options.Format.Includes(DisplayParts.Summary));
    }

    internal void WriteFindCommand(FindCommandOptions options)
    {
        WriteOption("align columns", options.AlignColumns);
        WriteOption("ask", options.AskMode);
        WriteOption("attributes", options.Attributes);
        WriteOption("attributes to skip", options.AttributesToSkip);
        WriteFilter("content filter", options.ContentFilter);
#if DEBUG
        WriteOption("content indent", options.Format.Indent);
#endif
        WriteOption("content mode", options.Format.ContentDisplayStyle);
#if DEBUG
        WriteOption("content separator", options.Format.Separator);
#endif
        WriteContext("context", options.Format.LineContext);
        WriteOption("count", options.Format.Includes(DisplayParts.Count));
        WriteEncoding("default encoding", options.DefaultEncoding);
        WriteFilter("directory filter", options.DirectoryFilter, options.DirectoryNamePart);
        WriteOption("empty", options.EmptyOption);
        WriteFilter("extension filter", options.ExtensionFilter);
        WriteFilePropertyFilter(
            "file properties",
            options.SizePredicate,
            options.CreationTimePredicate,
            options.ModifiedTimePredicate);
        WriteFunctions("function", options.ModifyOptions?.Functions);
        WriteOption("highlight options", options.HighlightOptions);
        WriteInput(options.Input);
        WriteOption("line number", options.Format.Includes(LineDisplayOptions.IncludeLineNumber));
        WriteOption("max matching files", options.MaxMatchingFiles);
        WriteOption("max matches in file", options.MaxMatchesInFile);
        WriteFilter("name filter", options.NameFilter, options.NamePart);
        WriteOption("path mode", options.Format.PathDisplayStyle);
        WritePaths("paths", options.Paths);
        WriteOption("progress", options.Progress);
        WriteProperties(options);
        WriteOption("recurse subdirectories", options.RecurseSubdirectories);
        WriteOption("search target", options.SearchTarget);
        WriteOption("split", options.Split);
        WriteSortOptions("sort", options.SortOptions);
        WriteOption("summary", options.Format.Includes(DisplayParts.Summary));
    }

    internal void WriteHelpCommand(HelpCommandOptions options)
    {
        WriteOption("command", string.Join(' ', options.Command));
        WriteOption("manual", options.Manual);
    }

    internal void WriteMoveCommand(MoveCommandOptions options)
    {
        WriteOption("align columns", options.AlignColumns);
#if DEBUG
        WriteOption("allowed time diff", options.AllowedTimeDiff);
#endif
        WriteOption("ask", options.AskMode);
        WriteOption("attributes", options.Attributes);
        WriteOption("attributes to skip", options.AttributesToSkip);
        WriteOption("compare", options.CompareProperties);
        WriteOption("conflict resolution", options.ConflictResolution);
        WriteFilter("content filter", options.ContentFilter);
#if DEBUG
        WriteOption("content indent", options.Format.Indent);
#endif
        WriteOption("content mode", options.Format.ContentDisplayStyle);
#if DEBUG
        WriteOption("content separator", options.Format.Separator);
#endif
        WriteContext("context", options.Format.LineContext);
        WriteOption("count", options.Format.Includes(DisplayParts.Count));
        WriteEncoding("default encoding", options.DefaultEncoding);
        WriteFilter("directory filter", options.DirectoryFilter, options.DirectoryNamePart);
        WriteOption("dry run", options.DryRun);
        WriteOption("empty", options.EmptyOption);
        WriteFilter("extension filter", options.ExtensionFilter);
        WriteFilePropertyFilter(
            "file properties",
            options.SizePredicate,
            options.CreationTimePredicate,
            options.ModifiedTimePredicate);
        WriteOption("flat", options.Flat);
        WriteOption("highlight options", options.HighlightOptions);
        WriteOption("line number", options.Format.Includes(LineDisplayOptions.IncludeLineNumber));
        WriteOption("max matching files", options.MaxMatchingFiles);
        WriteOption("max matches in file", options.MaxMatchesInFile);
        WriteFilter("name filter", options.NameFilter, options.NamePart);
#if DEBUG
        WriteOption("no compare attributes", options.NoCompareAttributes);
#endif
        WriteOption("path mode", options.Format.PathDisplayStyle);
        WritePaths("paths", options.Paths);
        WriteOption("progress", options.Progress);
        WriteProperties(options);
        WriteOption("recurse subdirectories", options.RecurseSubdirectories);
        WriteOption("search target", options.SearchTarget);
        WriteSortOptions("sort", options.SortOptions);
        WriteOption("summary", options.Format.Includes(DisplayParts.Summary));
        WriteOption("target", options.Target);
    }

    internal void WriteRegexEscapeCommand(RegexEscapeCommandOptions options)
    {
        WriteOption("char group", options.InCharGroup);
        WriteInput(options.Input);
        WriteOption("replacement", options.Replacement);
    }

    internal void WriteRegexMatchCommand(RegexMatchCommandOptions options)
    {
#if DEBUG
        WriteOption("content indent", options.Format.Indent);
#endif
        WriteOption("content mode", options.Format.ContentDisplayStyle);
#if DEBUG
        WriteOption("content separator", options.Format.Separator);
#endif
        WriteFilter("filter", options.Matcher);
        WriteOption("highlight options", options.HighlightOptions);
        WriteInput(options.Input);
        WriteOption("max count", options.MaxCount);
        WriteOption("summary", options.Format.Includes(DisplayParts.Summary));
    }

    internal void WriteRegexListCommand(RegexListCommandOptions options)
    {
        WriteOption("char", options.Value);
        WriteOption("char group", options.InCharGroup);
        WriteFilter("filter", options.Matcher);
        WriteOption("regex options", options.RegexOptions);
        WriteOption("sections", options.Sections);
    }

    internal void WriteRegexSplitCommand(RegexSplitCommandOptions options)
    {
#if DEBUG
        WriteOption("content indent", options.Format.Indent);
#endif
        WriteOption("content mode", options.Format.ContentDisplayStyle);
#if DEBUG
        WriteOption("content separator", options.Format.Separator);
#endif
        WriteFilter("filter", options.Matcher);
        WriteOption("highlight options", options.HighlightOptions);
        WriteInput(options.Input);
        WriteOption("max count", options.MaxCount);
        WriteOption("omit groups", options.OmitGroups);
        WriteOption("summary", options.Format.Includes(DisplayParts.Summary));
    }

    internal void WriteRenameCommand(RenameCommandOptions options)
    {
        WriteOption("align columns", options.AlignColumns);
        WriteOption("ask", options.AskMode == AskMode.File);
        WriteOption("attributes", options.Attributes);
        WriteOption("attributes to skip", options.AttributesToSkip);
        WriteOption("conflict resolution", options.ConflictResolution);
        WriteFilter("content filter", options.ContentFilter);
#if DEBUG
        WriteOption("content indent", options.Format.Indent);
#endif
        WriteOption("content mode", options.Format.ContentDisplayStyle);
#if DEBUG
        WriteOption("content separator", options.Format.Separator);
#endif
        WriteContext("context", options.Format.LineContext);
        WriteOption("count", options.Format.Includes(DisplayParts.Count));
        WriteEncoding("default encoding", options.DefaultEncoding);
        WriteFilter("directory filter", options.DirectoryFilter, options.DirectoryNamePart);
        WriteOption("dry run", options.DryRun);
        WriteOption("empty", options.EmptyOption);
        WriteEvaluator("evaluator", options.Replacer.MatchEvaluator);
        WriteFilter("extension filter", options.ExtensionFilter);
        WriteFilePropertyFilter(
            "file properties",
            options.SizePredicate,
            options.CreationTimePredicate,
            options.ModifiedTimePredicate);
        WriteOption("highlight options", options.HighlightOptions);
        WriteOption("interactive", options.Interactive);
        WriteOption("line number", options.Format.Includes(LineDisplayOptions.IncludeLineNumber));
        WriteOption("max matching files", options.MaxMatchingFiles);
        WriteReplaceModify("modify", options.Replacer);
        WriteFilter("name filter", options.NameFilter, options.NamePart);
        WriteOption("path mode", options.Format.PathDisplayStyle);
        WritePaths("paths", options.Paths);
        WriteOption("progress", options.Progress);
        WriteProperties(options);
        WriteOption("recurse subdirectories", options.RecurseSubdirectories);
        WriteOption("replacement", options.Replacer.Replacement);
        WriteOption("search target", options.SearchTarget);
        WriteSortOptions("sort", options.SortOptions);
        WriteOption("summary", options.Format.Includes(DisplayParts.Summary));
    }

    internal void WriteReplaceCommand(ReplaceCommandOptions options)
    {
        var replacer = (Replacer)options.Replacer;

        WriteOption("align columns", options.AlignColumns);
        WriteOption("ask", options.AskMode);
        WriteOption("attributes", options.Attributes);
        WriteOption("attributes to skip", options.AttributesToSkip);
        WriteFilter("content filter", options.ContentFilter);
#if DEBUG
        WriteOption("content indent", options.Format.Indent);
#endif
        WriteOption("content mode", options.Format.ContentDisplayStyle);
#if DEBUG
        WriteOption("content separator", options.Format.Separator);
#endif
        WriteContext("context", options.Format.LineContext);
        WriteOption("count", options.Format.Includes(DisplayParts.Count));
        WriteEncoding("default encoding", options.DefaultEncoding);
        WriteFilter("directory filter", options.DirectoryFilter, options.DirectoryNamePart);
        WriteOption("dry run", options.DryRun);
        WriteOption("empty", options.EmptyOption);
        WriteEvaluator("evaluator", replacer.MatchEvaluator);
        WriteFilter("extension filter", options.ExtensionFilter);
        WriteFilePropertyFilter(
            "file properties",
            options.SizePredicate,
            options.CreationTimePredicate,
            options.ModifiedTimePredicate);
        WriteOption("highlight options", options.HighlightOptions);
        WriteInput(options.Input);
        WriteOption("interactive", options.Interactive);
        WriteOption("line number", options.Format.Includes(LineDisplayOptions.IncludeLineNumber));
        WriteOption("max matching files", options.MaxMatchingFiles);
        WriteOption("max matches in file", options.MaxMatchesInFile);
        WriteReplaceModify("modify", replacer);
        WriteFilter("name filter", options.NameFilter, options.NamePart);
        WriteOption("path mode", options.Format.PathDisplayStyle);
        WritePaths("paths", options.Paths);
        WriteOption("progress", options.Progress);
        WriteProperties(options);
        WriteOption("recurse subdirectories", options.RecurseSubdirectories);
        WriteOption("replacement", replacer.Replacement);
        WriteOption("search target", options.SearchTarget);
        WriteSortOptions("sort", options.SortOptions);
        WriteOption("summary", options.Format.Includes(DisplayParts.Summary));
    }

    internal void WriteSpellcheckCommand(SpellcheckCommandOptions options)
    {
        var spellcheckState = (SpellcheckState)options.Replacer;

        WriteOption("align columns", options.AlignColumns);
        WriteOption("ask", options.AskMode);
        WriteOption("attributes", options.Attributes);
        WriteOption("attributes to skip", options.AttributesToSkip);
        WriteFilter("content filter", options.ContentFilter);
#if DEBUG
        WriteOption("content indent", options.Format.Indent);
#endif
        WriteOption("content mode", options.Format.ContentDisplayStyle);
#if DEBUG
        WriteOption("content separator", options.Format.Separator);
#endif
        WriteContext("context", options.Format.LineContext);
        WriteOption("count", options.Format.Includes(DisplayParts.Count));
        WriteEncoding("default encoding", options.DefaultEncoding);
        WriteFilter("directory filter", options.DirectoryFilter, options.DirectoryNamePart);
        WriteOption("dry run", options.DryRun);
        WriteOption("empty", options.EmptyOption);
        WriteFilter("extension filter", options.ExtensionFilter);
        WriteFilePropertyFilter(
            "file properties",
            options.SizePredicate,
            options.CreationTimePredicate,
            options.ModifiedTimePredicate);
        WriteOption("fixes", spellcheckState.Data.Fixes.Items.Sum(f => f.Value.Count));
        WriteOption("highlight options", options.HighlightOptions);
        WriteInput(options.Input);
        WriteOption("interactive", options.Interactive);
        WriteOption("line number", options.Format.Includes(LineDisplayOptions.IncludeLineNumber));
        WriteOption("max matching files", options.MaxMatchingFiles);
        WriteOption("max matches in file", options.MaxMatchesInFile);
        WriteOption("min word length", spellcheckState.Spellchecker.Options.MinWordLength);
        WriteOption("max word length", spellcheckState.Spellchecker.Options.MaxWordLength);
        WriteFilter("name filter", options.NameFilter, options.NamePart);
        WriteOption("path mode", options.Format.PathDisplayStyle);
        WritePaths("paths", options.Paths);
        WriteOption("progress", options.Progress);
        WriteProperties(options);
        WriteOption("recurse subdirectories", options.RecurseSubdirectories);
        WriteOption("search target", options.SearchTarget);
        WriteSortOptions("sort", options.SortOptions);
        WriteOption("split mode", spellcheckState.Spellchecker.Options.SplitMode);
        WriteOption("summary", options.Format.Includes(DisplayParts.Summary));
        WriteOption("words", spellcheckState.Data.WordList.Words.Count + spellcheckState.Data.CaseSensitiveWordList.Words.Count);
    }

    internal void WriteSyncCommand(SyncCommandOptions options)
    {
        WriteOption("align columns", options.AlignColumns);
#if DEBUG
        WriteOption("allowed time diff", options.AllowedTimeDiff);
#endif
        WriteOption("ask", options.AskMode);
        WriteOption("attributes", options.Attributes);
        WriteOption("attributes to skip", options.AttributesToSkip);
        WriteOption("compare", options.CompareProperties);
        WriteOption("conflict resolution", options.ConflictResolution);
        WriteFilter("content filter", options.ContentFilter);
#if DEBUG
        WriteOption("content indent", options.Format.Indent);
#endif
        WriteOption("content mode", options.Format.ContentDisplayStyle);
#if DEBUG
        WriteOption("content separator", options.Format.Separator);
#endif
        WriteContext("context", options.Format.LineContext);
        WriteOption("count", options.Format.Includes(DisplayParts.Count));
        WriteEncoding("default encoding", options.DefaultEncoding);
#if DEBUG
        WriteOption("detect rename", options.DetectRename);
#endif
        WriteFilter("directory filter", options.DirectoryFilter, options.DirectoryNamePart);
        WriteOption("dry run", options.DryRun);
        WriteOption("empty", options.EmptyOption);
        WriteFilter("extension filter", options.ExtensionFilter);
        WriteFilePropertyFilter(
            "file properties",
            options.SizePredicate,
            options.CreationTimePredicate,
            options.ModifiedTimePredicate);
        WriteOption("flat", options.Flat);
        WriteOption("highlight options", options.HighlightOptions);
        WriteOption("line number", options.Format.Includes(LineDisplayOptions.IncludeLineNumber));
        WriteOption("max matching files", options.MaxMatchingFiles);
        WriteOption("max matches in file", options.MaxMatchesInFile);
        WriteFilter("name filter", options.NameFilter, options.NamePart);
        WriteOption("no compare attributes", options.NoCompareAttributes);
        WriteOption("path mode", options.Format.PathDisplayStyle);
        WritePaths("paths", options.Paths);
        WriteOption("progress", options.Progress);
        WriteProperties(options);
        WriteOption("recurse subdirectories", options.RecurseSubdirectories);
        WriteOption("search target", options.SearchTarget);
        WriteSortOptions("sort", options.SortOptions);
        WriteOption("second", options.Target);
        WriteOption("summary", options.Format.Includes(DisplayParts.Summary));
    }

    private void WriteInput(string? value)
    {
        WriteName("input");

        const int maxLength = 10000;
        int remainingLength = 0;

        if (value?.Length > maxLength)
        {
            remainingLength = value.Length - maxLength;
            value = value.Remove(maxLength);
        }

        WriteValue(value);

        if (remainingLength > 0)
            _logger.Write($"<truncated> {remainingLength:n0} remaining characters", NullValueColors);

        WriteLine();
    }

    private void WriteOption(string name, int value)
    {
        WriteName(name);
        WriteValue(value.ToString());
        WriteLine();
    }

    private void WriteOption(string name, bool value)
    {
        WriteName(name);
        WriteValue(value);
        WriteLine();
    }

    private void WriteOption(string name, char? value)
    {
        WriteName(name);

        if (value is null)
        {
            WriteNullValue();
        }
        else
        {
            WriteValue(value.Value.ToString());
        }

        WriteLine();
    }

    private void WriteOption(string name, string? value, bool replaceAllSymbols = false)
    {
        WriteName(name);
        WriteValue(value, replaceAllSymbols: replaceAllSymbols);
        WriteLine();
    }

    private void WriteOption<TEnum>(string name, TEnum value) where TEnum : Enum
    {
        WriteName(name);
        WriteValue(value.ToString());
        WriteLine();
    }

    private void WriteOption(string name, object? value)
    {
        WriteName(name);
        WriteValue(value?.ToString());
        WriteLine();
    }

    private void WriteOption<TEnum>(string name, ImmutableArray<TEnum> values) where TEnum : Enum
    {
        WriteName(name);

        if (values.IsEmpty)
        {
            WriteNullValue();
        }
        else
        {
            WriteValue(string.Join(", ", values));
        }

        WriteLine();
    }

#if DEBUG
    private void WriteOption(string name, TimeSpan value)
    {
        WriteName(name);
        WriteValue(value.ToString());
        WriteLine();
    }
#endif

    private void WriteEncoding(string name, Encoding encoding)
    {
        WriteName(name);
        WriteValue(encoding.EncodingName);
        WriteLine();
    }

    private void WriteFilter(string name, Matcher? matcher, FileNamePart? part = null)
    {
        WriteName(name);

        if (matcher is null)
        {
            WriteNullValue();
            WriteLine();
            return;
        }

        WriteRegex(matcher.Regex);

        WriteIndent();
        WriteName("negative");
        WriteIndent();
        WriteLine(matcher.Invert.ToString().ToLowerInvariant());
        WriteIndent();
        WriteName("group");

        if (string.IsNullOrEmpty(matcher.GroupName))
        {
            WriteNullValue();
            WriteLine();
        }
        else
        {
            WriteIndent();
            WriteLine(matcher.GroupName);
        }

        if (part is not null)
        {
            WriteIndent();
            WriteName("part");
            WriteIndent();
            WriteLine(part.ToString());
        }
    }

    private void WriteRegex(Regex regex)
    {
        WriteLine();
        WriteIndent();
        WriteName("pattern");
        WriteIndent();
        WriteLine(regex.ToString());
        WriteIndent();
        WriteName("options");
        WriteIndent();
        WriteLine(regex.Options.ToString());
    }

    private void WriteFilePropertyFilter(
        string name,
        FilterPredicate<long>? sizePredicate,
        FilterPredicate<DateTime>? creationTimePredicate,
        FilterPredicate<DateTime>? modifiedTimePredicate)
    {
        WriteName(name);

        if (sizePredicate is null
            && creationTimePredicate is null
            && modifiedTimePredicate is null)
        {
            WriteNullValue();
            WriteLine();
        }
        else
        {
            WriteLine();
            WriteIndent();
            WriteFilterPredicate("creation time", creationTimePredicate);
            WriteFilterPredicate("modified time", modifiedTimePredicate);
            WriteFilterPredicate("size", sizePredicate);
        }
    }

    private void WriteFilterPredicate<T>(string name, FilterPredicate<T>? filterPredicate)
    {
        if (filterPredicate is null)
            return;

        WriteIndent();
        WriteName(name);
        WriteValue(filterPredicate.Expression.Kind.ToString());
        WriteIndent();
        WriteValue(filterPredicate.Expression.Text);
        WriteLine();
    }

    private void WriteContext(string name, LineContext lineContext)
    {
        if (lineContext.Before == lineContext.After)
        {
            WriteOption(name, lineContext.Before);
        }
        else
        {
            WriteName(name);
            WriteLine();
            WriteIndent();
            WriteOption("context before", lineContext.Before);
            WriteIndent();
            WriteOption("context after", lineContext.After);
        }
    }

    private void WriteProperties(FileSystemCommandOptions options)
    {
        WriteName("properties");

        FilePropertyOptions filePropertyOptions = options.FilePropertyOptions;

        if (!filePropertyOptions.IncludeCreationTime
            && !filePropertyOptions.IncludeModifiedTime
            && !filePropertyOptions.IncludeSize
            && filePropertyOptions.CreationTimePredicate is null
            && filePropertyOptions.ModifiedTimePredicate is null
            && filePropertyOptions.SizePredicate is null)
        {
            WriteNullValue();
            WriteLine();
            return;
        }

        WriteLine();
        WriteIndent();
        WriteOption("creation time", filePropertyOptions.IncludeCreationTime);
        WriteIndent();
        WriteOption("creation time condition", options.CreationTimePredicate?.Expression.Text);
        WriteIndent();
        WriteOption("modified time", filePropertyOptions.IncludeModifiedTime);
        WriteIndent();
        WriteOption("modified time condition", options.ModifiedTimePredicate?.Expression.Text);
        WriteIndent();
        WriteOption("size", filePropertyOptions.IncludeSize);
        WriteIndent();
        WriteOption("size condition", options.SizePredicate?.Expression.Text);
    }

    private void WriteSortOptions(string name, SortOptions? options)
    {
        WriteName(name);

        if (options is null)
        {
            WriteNullValue();
            WriteLine();
            return;
        }

        WriteLine();
        WriteIndent();
        WriteOption("max count", options.MaxCount);

        foreach (SortDescriptor descriptor in options.Descriptors)
        {
            WriteIndent();
            WriteName("property");
            WriteValue(descriptor.Property.ToString());
            WriteIndent();
            WriteName("direction");
            WriteValue(descriptor.Direction.ToString());
            WriteLine();
        }
    }

    private void WritePaths(string name, ImmutableArray<PathInfo> paths)
    {
        WriteName(name);

        if (paths.Length > 1)
            WriteLine();

        foreach (PathInfo path in paths)
        {
            WriteValue($"[{path.Origin}] {path.Path}");
            WriteLine();
        }
    }

    private void WriteEvaluator(string name, MatchEvaluator? matchEvaluator)
    {
        WriteName(name);

        if (matchEvaluator is not null
            && !object.ReferenceEquals(matchEvaluator.Method.DeclaringType!.Assembly, typeof(Program).Assembly))
        {
            WriteLine();
            WriteIndent();
            WriteOption("type", matchEvaluator.Method.DeclaringType.AssemblyQualifiedName);
            WriteIndent();
            WriteOption("method", matchEvaluator.Method.Name);
        }
        else
        {
            WriteNullValue();
            WriteLine();
        }
    }

    private void WriteReplaceModify(string name, Replacer replacer)
    {
        WriteName(name);

        if (replacer.Functions != ReplaceFunctions.None)
        {
            if (replacer.CultureInvariant)
            {
                WriteValue(nameof(replacer.CultureInvariant) + ", " + replacer.Functions.ToString());
            }
            else
            {
                WriteValue(replacer.Functions.ToString());
            }
        }
        else if (replacer.CultureInvariant)
        {
            WriteValue(nameof(replacer.CultureInvariant));
        }
        else
        {
            WriteNullValue();
        }

        WriteLine();
    }

    private void WriteFunctions(string name, ModifyFunctions? functions)
    {
        WriteName(name);

        if (functions is not null
            && functions != ModifyFunctions.None)
        {
            WriteValue(functions.ToString());
        }
        else
        {
            WriteNullValue();
        }
    }

    private void WriteValue(bool value)
    {
        WriteIndent();
        Write((value) ? "true" : "false");
    }

    private void WriteValue(string? value, bool replaceAllSymbols = false)
    {
        if (value is null)
        {
            WriteNullValue();
        }
        else
        {
            WriteIndent();

            if (replaceAllSymbols)
            {
                ValueWriter.Write(value, Symbols_Character, ValueColors);
            }
            else
            {
                ValueWriter.Write(value, Symbols_Newline, ValueColors);
            }
        }
    }

    private void WriteIndent()
    {
        Write("  ");
    }

    private void WriteName(string name)
    {
        _logger.Write(name, Verbosity);
        _logger.Write(":", Verbosity);
    }

    private void WriteNullValue()
    {
        WriteIndent();
        _logger.Write("<none>", NullValueColors, Verbosity);
    }

    private void Write(string value)
    {
        _logger.Write(value, ValueColors, Verbosity);
    }

    private void WriteLine()
    {
        _logger.WriteLine(Verbosity);
    }

    private void WriteLine(string? value)
    {
        _logger.WriteLine(value, ValueColors, Verbosity);
    }
}
