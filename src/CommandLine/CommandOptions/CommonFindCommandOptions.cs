// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.CommandLine;

internal abstract class CommonFindCommandOptions : FileSystemCommandOptions
{
    internal CommonFindCommandOptions()
    {
    }

    public AskMode AskMode { get; internal set; }

    public int MaxTotalMatches { get; internal set; }

    public int MaxMatchesInFile { get; internal set; }
}
