// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.CommandLine.Help;

public class HelpWriterOptions
{
    public static HelpWriterOptions Default { get; } = new();

    public HelpWriterOptions(
        string indent = "  ",
        Matcher? matcher = null)
    {
        Indent = indent;
        Matcher = matcher;
    }

    public string Indent { get; }

    public Matcher? Matcher { get; }
}
