// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

using System;

namespace Orang.FileSystem;

public sealed class SearchTelemetry
{
    internal SearchTelemetry()
    {
    }

    public int SearchedDirectoryCount { get; internal set; }

    public int FileCount { get; internal set; }

    public int DirectoryCount { get; internal set; }

    public int MatchingFileCount { get; internal set; }

    public int MatchingDirectoryCount { get; internal set; }

    internal int MatchingFileDirectoryCount => MatchingFileCount + MatchingDirectoryCount;

    public int ProcessedFileCount { get; internal set; }

    public int ProcessedDirectoryCount { get; internal set; }

    public int MatchCount { get; internal set; }

    public int ProcessedMatchCount { get; internal set; }

    internal void IncrementMatchingCount(bool isDirectory)
    {
        if (isDirectory)
        {
            MatchingDirectoryCount++;
        }
        else
        {
            MatchingFileCount++;
        }
    }

    internal void IncrementProcessedCount(bool isDirectory)
    {
        if (isDirectory)
        {
            ProcessedDirectoryCount++;
        }
        else
        {
            ProcessedFileCount++;
        }
    }

    internal int MatchingLineCount { get; set; }

    internal TimeSpan Elapsed { get; set; }

    internal long FilesTotalSize { get; set; }

    internal int UpdatedCount { get; set; }

    internal int AddedCount { get; set; }

    internal int RenamedCount { get; set; }

    internal int DeletedCount { get; set; }

    public SearchTelemetry Add(SearchTelemetry other)
    {
        SearchedDirectoryCount += other.SearchedDirectoryCount;
        FileCount += other.FileCount;
        DirectoryCount += other.DirectoryCount;
        MatchingFileCount += other.MatchingFileCount;
        MatchingDirectoryCount += other.MatchingDirectoryCount;
        ProcessedFileCount += other.ProcessedFileCount;
        ProcessedDirectoryCount += other.ProcessedDirectoryCount;
        MatchCount += other.MatchCount;
        ProcessedMatchCount += other.ProcessedMatchCount;
        MatchingLineCount += other.MatchingLineCount;
        Elapsed += other.Elapsed;
        FilesTotalSize += other.FilesTotalSize;
        UpdatedCount += other.UpdatedCount;
        AddedCount += other.AddedCount;
        RenamedCount += other.RenamedCount;
        DeletedCount += other.DeletedCount;

        return this;
    }
}
