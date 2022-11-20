// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.CommandLine;

internal class ApplicationOptions
{
    public static ApplicationOptions Default { get; } = new();

    public ApplicationOptions(
        string? contentIndent = null,
        string? contentSeparator = null,
        string? dateFormat = null,
        string? sizeFormat = null,
        int progressCount = 100)
    {
        ContentIndent = contentIndent ?? "";
        ContentSeparator = contentSeparator;
        DateFormat = dateFormat ?? "yyyy-MM-dd HH:mm:ss";
        SizeFormat = sizeFormat ?? "n0";
        ProgressCount = progressCount;
    }

    public string ContentIndent { get; }

    public string? ContentSeparator { get; }

    public string DateFormat { get; }

    public string SizeFormat { get; }

    public int ProgressCount { get; }
}
