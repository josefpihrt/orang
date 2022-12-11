// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Orang.Text;

#pragma warning disable RCS1223

namespace Orang.FileSystem;

public class FileMatcher
{
    public Matcher? Name { get; set; }

    public FileNamePart NamePart { get; set; }

    public Matcher? Extension { get; set; }

    public Matcher? Content { get; set; }

    public FileAttributes Attributes { get; set; }

    public FileAttributes AttributesToSkip { get; set; }

    public FileEmptyOption EmptyOption { get; set; }

    public Func<FileInfo, bool>? Predicate { get; set; }

    internal bool IsMatch(string path)
    {
        return Match(path).FileMatch is not null;
    }

    internal (FileMatch? FileMatch, Exception? Exception) Match(string path, Encoding? defaultEncoding = null)
    {
        if (Extension?.IsMatch(FileNameSpan.FromFile(path, FileNamePart.Extension)) == false)
            return default;

        FileNameSpan span = FileNameSpan.FromFile(path, NamePart);
        Match? match = null;

        if (Name is not null)
        {
            match = Name.Match(span);

            if (match is null)
                return default;
        }

        FileEmptyOption emptyOption = EmptyOption;
        var isEmpty = false;
        FileInfo? fileInfo = null;

        if (Attributes != 0
            || emptyOption != FileEmptyOption.None
            || Predicate is not null)
        {
            try
            {
                fileInfo = new FileInfo(path);

                if (Attributes != 0
                    && (fileInfo.Attributes & Attributes) != Attributes)
                {
                    return default;
                }

                if (Predicate?.Invoke(fileInfo) == false)
                    return default;

                if (emptyOption != FileEmptyOption.None)
                {
                    if (fileInfo.Length == 0)
                    {
                        if (emptyOption == FileEmptyOption.NonEmpty)
                            return default;

                        isEmpty = true;
                    }
                    else if (fileInfo.Length > 4)
                    {
                        if (emptyOption == FileEmptyOption.Empty)
                            return default;

                        emptyOption = FileEmptyOption.None;
                    }
                }
            }
            catch (Exception ex) when (ex is IOException
                || ex is UnauthorizedAccessException)
            {
                return (null, ex);
            }
        }

        if (Content is not null
            || emptyOption != FileEmptyOption.None)
        {
            FileContent fileContent = default;

            if (isEmpty)
            {
                fileContent = new FileContent("", defaultEncoding ?? EncodingHelpers.UTF8NoBom, hasBom: false);
            }
            else
            {
                try
                {
                    using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        Encoding? bomEncoding = FileSystemUtilities.DetectEncoding(stream);

                        if (emptyOption != FileEmptyOption.None)
                        {
                            if (bomEncoding?.Preamble.Length == stream.Length)
                            {
                                if (emptyOption == FileEmptyOption.NonEmpty)
                                    return default;
                            }
                            else if (emptyOption == FileEmptyOption.Empty)
                            {
                                return default;
                            }
                        }

                        if (Content is not null)
                        {
                            using (var reader = new StreamReader(
                                stream: stream,
                                encoding: bomEncoding ?? defaultEncoding ?? EncodingHelpers.UTF8NoBom,
                                detectEncodingFromByteOrderMarks: bomEncoding is null))
                            {
                                string content = reader.ReadToEnd();

                                fileContent = new FileContent(
                                    content,
                                    reader.CurrentEncoding,
                                    hasBom: bomEncoding is not null);
                            }
                        }
                    }
                }
                catch (Exception ex) when (ex is IOException
                    || ex is UnauthorizedAccessException)
                {
                    return (null, ex);
                }
            }

            if (Content is not null)
            {
                Match? contentMatch = Content.Match(fileContent.Text);

                if (contentMatch is null)
                    return default;

                return (new FileMatch(span, match, fileContent, contentMatch, fileInfo), null);
            }
        }

        return (new FileMatch(span, match, fileInfo), null);
    }
}
