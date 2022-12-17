// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Orang.FileSystem.Commands;

internal class ReplaceCommand : CommonFindCommand
{
    public ReplaceOptions ReplaceOptions { get; set; } = null!;

    public Replacer Replacer { get; set; } = null!;

    public Matcher ContentFilter { get; set; } = null!;

    public override OperationKind OperationKind => OperationKind.Replace;

    protected override void OnSearchStateCreating(SearchState search)
    {
        search.CanRecurseMatch = false;
    }

    protected override void ExecuteMatch(
        FileMatch fileMatch,
        string directoryPath)
    {
        TextWriter? textWriter = null;
        List<Capture>? captures = null;

        try
        {
            captures = ListCache<Capture>.GetInstance();

            GetCaptures(
                fileMatch.ContentMatch!,
                ContentFilter!.GroupNumber,
                predicate: ContentFilter.Predicate,
                captures: captures);

            if (!DryRun)
            {
                textWriter = new StreamWriter(fileMatch.Path, false, fileMatch.Encoding);

                WriteMatches(fileMatch.ContentText, captures, Replacer, textWriter);
            }

            fileMatch.Content = default;

            int fileMatchCount = captures.Count;
            int fileReplacementCount = fileMatchCount;
            Telemetry.MatchCount += fileMatchCount;
            Telemetry.ProcessedMatchCount += fileReplacementCount;

            if (fileReplacementCount > 0)
                Telemetry.ProcessedFileCount++;

            Log(fileMatch);
        }
        catch (Exception ex) when (ex is IOException
            || ex is UnauthorizedAccessException)
        {
            Log(fileMatch, ex);
        }
        finally
        {
            textWriter?.Dispose();

            if (captures is not null)
                ListCache<Capture>.Free(captures);
        }
    }

    private void WriteMatches(
        string input,
        IEnumerable<Capture> captures,
        Replacer replacer,
        TextWriter textWriter)
    {
        int index = 0;

        foreach (Capture capture in captures)
        {
            textWriter.Write(input.AsSpan(index, capture.Index - index));

            string result = replacer.Replace((Match)capture);

            textWriter.Write(result);

            index = capture.Index + capture.Length;
        }

        textWriter.Write(input.AsSpan(index, input.Length - index));
    }
}
