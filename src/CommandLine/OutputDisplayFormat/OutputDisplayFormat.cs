// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.CommandLine;

internal class OutputDisplayFormat
{
    public OutputDisplayFormat(
        ContentDisplayStyle contentDisplayStyle,
        PathDisplayStyle pathDisplayStyle = PathDisplayStyle.Full,
        LineDisplayOptions lineOptions = LineDisplayOptions.None,
        LineContext lineContext = default,
        DisplayParts displayParts = DisplayParts.None,
        string? indent = null,
        string? separator = null)
    {
        ContentDisplayStyle = contentDisplayStyle;
        PathDisplayStyle = pathDisplayStyle;
        LineOptions = lineOptions;
        LineContext = lineContext;
        DisplayParts = displayParts;
        Indent = indent ?? ApplicationOptions.Default.ContentIndent;
        Separator = separator ?? ApplicationOptions.Default.ContentSeparator;
    }

    public ContentDisplayStyle ContentDisplayStyle { get; }

    public PathDisplayStyle PathDisplayStyle { get; }

    public LineDisplayOptions LineOptions { get; }

    public LineContext LineContext { get; }

    public DisplayParts DisplayParts { get; }

    public string Indent { get; }

    public string? Separator { get; }

    public bool Includes(LineDisplayOptions options) => (LineOptions & options) == options;

    public bool Includes(DisplayParts parts) => (DisplayParts & parts) == parts;
}
