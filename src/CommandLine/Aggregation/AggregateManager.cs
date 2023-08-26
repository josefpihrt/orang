// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Orang.CommandLine;

namespace Orang.Aggregation;

internal sealed class AggregateManager
{
    private readonly Logger _logger;

    private AggregateManager(
        ListResultStorage storage,
        List<StorageSection>? sections,
        FindCommandOptions options,
        Logger logger)
    {
        Storage = storage;
        Sections = sections;
        Options = options;
        _logger = logger;

        if (logger.ShouldWrite(Verbosity.Minimal))
        {
            PathWriter = new PathWriter(
                logger,
                pathColors: Colors.Matched_Path,
                relativePath: Options.DisplayRelativePath,
                indent: "  ");
        }
    }

    public ListResultStorage Storage { get; set; }

    public List<StorageSection>? Sections { get; set; }

    public FindCommandOptions Options { get; }

    public ModifyOptions ModifyOptions => Options.ModifyOptions;

    private PathWriter? PathWriter { get; }

    public static AggregateManager? TryCreate(FindCommandOptions Options, Logger logger)
    {
        ModifyOptions modifyOptions = Options.ModifyOptions;

        bool shouldCreate = modifyOptions.HasFunction(ModifyFunctions.Except_Intersect_Group)
            || (modifyOptions.Aggregate);

        if (!shouldCreate)
            return null;

        var storage = new ListResultStorage();

        List<StorageSection>? sections = null;

        if (modifyOptions.HasFunction(ModifyFunctions.Except_Intersect))
        {
            sections = new List<StorageSection>();
        }
        else if ((Options.ContentFilter is not null || Options.NameFilter is not null)
            && modifyOptions.HasFunction(ModifyFunctions.Group))
        {
            sections = new List<StorageSection>();
        }

        return new AggregateManager(storage, sections, Options, logger);
    }

    public void WriteAggregatedValues(CancellationToken cancellationToken)
    {
        int count = 0;

        IEnumerable<string> values = Storage.Values;

        if (Sections?.Count > 1
            && (ModifyOptions.HasFunction(ModifyFunctions.Except_Intersect)))
        {
            if (ModifyOptions.HasFunction(ModifyFunctions.Except))
            {
                values = ExceptOrIntersect(Storage.Values, (x, y, c) => x.Except(y, c));
            }
            else if (ModifyOptions.HasFunction(ModifyFunctions.Intersect))
            {
                values = ExceptOrIntersect(Storage.Values, (x, y, c) => x.Intersect(y, c));
            }
        }

        StringComparer comparer = ModifyOptions.StringComparer;
        Dictionary<string, SectionInfo>? valuesMap = null;

        if (Sections?.Count > 0
            && ModifyOptions.HasFunction(ModifyFunctions.Group | ModifyFunctions.Distinct))
        {
            if (Options.ContentFilter is not null)
            {
                valuesMap = GroupByValues(Storage.Values);
            }
            else
            {
                IEnumerable<IGrouping<string, (string First, StorageSection Second)>> grouping = Storage.Values
                    .Zip(Sections)
                    .GroupBy(f => f.First, comparer);

                if (ModifyOptions.HasFunction(ModifyFunctions.Group))
                {
                    valuesMap = grouping
                        .ToDictionary(f => f.Key, f => new SectionInfo(f.Select(f => f.Second).ToList()), comparer);
                }
                else if (ModifyOptions.HasFunction(ModifyFunctions.Distinct))
                {
                    valuesMap = grouping
                        .ToDictionary(f => f.Key, f => new SectionInfo(f.Count()), comparer);
                }
            }
        }
        else if (ModifyOptions.HasFunction(ModifyFunctions.Distinct))
        {
            valuesMap = values
                .GroupBy(f => f, comparer)
                .ToDictionary(f => f.Key, f => new SectionInfo(f.Count()), comparer);
        }

        if (valuesMap is not null)
        {
            if (ModifyOptions.SortProperty == ValueSortProperty.Count)
            {
                if (ModifyOptions.HasFunction(ModifyFunctions.SortAscending))
                {
                    valuesMap = valuesMap
                        .OrderBy(f => f.Value.Count)
                        .ToDictionary(f => f.Key, f => f.Value);
                }
                else if (ModifyOptions.HasFunction(ModifyFunctions.SortDescending))
                {
                    valuesMap = valuesMap
                        .OrderByDescending(f => f.Value.Count)
                        .ToDictionary(f => f.Key, f => f.Value);
                }
            }

            values = valuesMap.Select(f => f.Key);
        }

        ModifyFunctions filter = (ModifyOptions.SortProperty == ValueSortProperty.Count)
            ? ModifyFunctions.None
            : ModifyFunctions.Sort;

        values = values.Modify(ModifyOptions, filter: filter);

        using (IEnumerator<string> en = values.GetEnumerator())
        {
            if (en.MoveNext())
            {
                OutputSymbols symbols = OutputSymbols.Create(Options.HighlightOptions);

                ConsoleColors colors = ((Options.HighlightOptions & HighlightOptions.Match) != 0)
                    ? Colors.Match
                    : default;

                ConsoleColors boundaryColors = (Options.HighlightBoundary) ? Colors.MatchBoundary : default;
                var valueWriter = new ValueWriter(new ContentTextWriter(_logger, Verbosity.Minimal), includeEndingIndent: false);

                if (!Options.AggregateOnly)
                {
                    _logger.ConsoleOut.WriteLineIf(ShouldWriteLine(_logger.ConsoleOut.Verbosity));
                    _logger.Out?.WriteLineIf(ShouldWriteLine(_logger.Out.Verbosity));
                }

                do
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    valueWriter.Write(en.Current, symbols, colors, boundaryColors);

                    if (valuesMap is not null
                        && ModifyOptions.HasFunction(ModifyFunctions.Count))
                    {
                        int groupCount = valuesMap[en.Current].Count;

                        _logger.Write("  ", Colors.Message_OK, Verbosity.Minimal);
                        _logger.Write(groupCount.ToString("n0"), Colors.Message_OK, Verbosity.Minimal);
                    }

                    _logger.WriteLine(Verbosity.Minimal);

                    List<StorageSection>? sections = valuesMap?[en.Current].Sections;

                    if (sections is not null)
                        WriteGroup(sections, cancellationToken);

                    count++;
                }
                while (en.MoveNext());

                WriteSummary(count, valuesMap);
            }
        }

        bool ShouldWriteLine(Verbosity verbosity)
        {
            if (verbosity > Verbosity.Minimal)
                return true;

            return verbosity == Verbosity.Minimal
                && (Options.PathDisplayStyle != PathDisplayStyle.Omit || Options.IncludeSummary);
        }
    }

    private List<string> ExceptOrIntersect(
        List<string> values,
        Func<IEnumerable<string>, IEnumerable<string>, IEqualityComparer<string>, IEnumerable<string>> operation)
    {
        var list = new List<string>(GetRange(values, 0, Sections![0].Count));

        for (int i = 1; i < Sections.Count; i++)
        {
            if (list.Count == 0)
                break;

            IEnumerable<string> second = GetRange(
                values,
                Sections[i - 1].Count,
                Sections[i].Count);

            list = operation(list, second, ModifyOptions.StringComparer).ToList();
        }

        return list;
    }

    private Dictionary<string, SectionInfo> GroupByValues(List<string> values)
    {
        var sectionsValues = new List<(StorageSection section, IEnumerable<string> values)>()
        {
            (Sections![0], GetRange(values, 0, Sections[0].Count))
        };

        for (int i = 1; i < Sections.Count; i++)
        {
            IEnumerable<string> second = GetRange(
                values,
                Sections[i - 1].Count,
                Sections[i].Count);

            sectionsValues.Add((Sections[i], second));
        }

        IEnumerable<IGrouping<string, (StorageSection section, string value)>> grouping = sectionsValues
            .SelectMany(f => f.values.Select(value => (f.section, value)))
            .GroupBy(f => f.value, ModifyOptions.StringComparer);

        if (ModifyOptions.HasFunction(ModifyFunctions.Group))
        {
            return grouping.ToDictionary(
                f => f.Key,
                f => new SectionInfo(f.Select(g => g.section).ToList()),
                ModifyOptions.StringComparer);
        }
        else if (ModifyOptions.HasFunction(ModifyFunctions.Distinct))
        {
            return grouping.ToDictionary(
                f => f.Key,
                f => new SectionInfo(f.Count()),
                ModifyOptions.StringComparer);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    private static IEnumerable<string> GetRange(List<string> values, int start, int end)
    {
        for (int i = start; i < end; i++)
            yield return values[i];
    }

    private void WriteGroup(
        IEnumerable<StorageSection> sections,
        CancellationToken cancellationToken)
    {
        SortOptions? sortOptions = Options.SortOptions;

        if (sortOptions?.Descriptors.Any() == true)
        {
            List<SearchResult>? results = sections
                .Select(f => new SearchResult(f.FileMatch, f.BaseDirectoryPath))
                .ToList();

            PathDisplayStyle pathDisplayStyle = (Options.PathDisplayStyle == PathDisplayStyle.Relative)
                ? PathDisplayStyle.Relative
                : PathDisplayStyle.Full;

            sections = SortHelpers.SortResults(results, sortOptions, pathDisplayStyle)
                .Select(f => new StorageSection(f.FileMatch, f.BaseDirectoryPath, -1));
        }

        foreach (IGrouping<StorageSection, StorageSection>? grouping in sections
            .GroupBy(f => f, StorageSectionComparer.Path))
        {
            cancellationToken.ThrowIfCancellationRequested();

            PathWriter?.WritePath(grouping.Key.FileMatch, grouping.Key.BaseDirectoryPath);

            int pathCount = grouping.Count();

            if (pathCount > 1)
            {
                _logger.Write("  ", Colors.Message_OK, Verbosity.Minimal);
                _logger.Write(pathCount.ToString("n0"), Colors.Message_OK, Verbosity.Minimal);
            }

            _logger.WriteLine(Verbosity.Minimal);
        }
    }

    private void WriteSummary(int count, Dictionary<string, SectionInfo>? valuesMap)
    {
        LogWriter? first = _logger.ConsoleOut;
        LogWriter? second = _logger.Out;

        if (Options.IncludeCount
            && !Options.IncludeSummary)
        {
            if (second?.Verbosity == Verbosity.Quiet)
            {
                second.WriteLine(count.ToString("n0"));
                second = null;
            }

            if (first.Verbosity == Verbosity.Quiet)
            {
                first.WriteLine(count.ToString("n0"));
                first = null;

                if (second is null)
                    return;
            }
        }

        if (Options.IncludeSummary
            || _logger.ShouldWrite(Verbosity.Detailed))
        {
            if (first is null)
            {
                first = second;
                second = null;
            }

            using (var writer = new Logger(first!, second))
            {
                Verbosity verbosity = (Options.IncludeSummary)
                    ? Verbosity.Minimal
                    : Verbosity.Detailed;

                if (valuesMap is not null)
                    count = valuesMap.Sum(f => f.Value.Count);

                writer.WriteLine(verbosity);

                if (valuesMap is not null)
                {
                    writer.WriteValue(valuesMap.Count.ToString("n0"), "Groups", verbosity: verbosity);
                    writer.Write("  ", verbosity);
                }

                writer.WriteValue(count.ToString("n0"), "Values", verbosity: verbosity);
                writer.WriteLine(verbosity);
            }
        }
    }

    private readonly struct SectionInfo
    {
        public SectionInfo(List<StorageSection> sections)
        {
            Count = sections.Count;
            Sections = sections;
        }

        public SectionInfo(int count)
        {
            Count = count;
            Sections = null;
        }

        public int Count { get; }

        public List<StorageSection>? Sections { get; }
    }
}
