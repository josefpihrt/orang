// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

#pragma warning disable RCS1223

namespace Orang.FileSystem;

public class DirectoryMatcher
{
    public Matcher? Name { get; set; }

    public FileNamePart NamePart { get; set; }

    public FileAttributes WithAttributes { get; set; }

    public FileAttributes WithoutAttributes { get; set; }

    public FileEmptyOption EmptyOption { get; set; }

    public Func<DirectoryInfo, bool>? MatchDirectoryInfo { get; set; }

    internal (FileMatch? FileMatch, Exception? Exception) Match(string path)
    {
        FileNameSpan span = FileNameSpan.FromDirectory(path, NamePart);
        Match? match = null;

        if (Name is not null)
        {
            match = Name.Match(span);

            if (match is null)
                return default;
        }

        DirectoryInfo? directoryInfo = null;

        if (WithAttributes != 0
            || EmptyOption != FileEmptyOption.None
            || MatchDirectoryInfo is not null)
        {
            try
            {
                directoryInfo = new DirectoryInfo(path);

                if (WithAttributes != 0
                    && (directoryInfo.Attributes & WithAttributes) != WithAttributes)
                {
                    return default;
                }

                if (MatchDirectoryInfo?.Invoke(directoryInfo) == false)
                    return default;

                if (EmptyOption == FileEmptyOption.Empty)
                {
                    if (!IsEmptyDirectory(path))
                        return default;
                }
                else if (EmptyOption == FileEmptyOption.NonEmpty)
                {
                    if (IsEmptyDirectory(path))
                        return default;
                }
            }
            catch (Exception ex) when (ex is IOException
                || ex is UnauthorizedAccessException)
            {
                return (null, ex);
            }
        }

        return (new FileMatch(span, match, directoryInfo, isDirectory: true), null);

        static bool IsEmptyDirectory(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }
    }
}
