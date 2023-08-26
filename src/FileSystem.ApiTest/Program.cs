// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Orang;
using Orang.FileSystem;
using Orang.FileSystem.Fluent;

namespace N;

public static class Program
{
    public static void Main()
    {
        const string directoryPath = "";

        IOperationResult result = new SearchBuilder()
            .MatchDirectory(d => d
                .Name(Pattern.Any("bin", "obj", PatternOptions.Equals))
                .NonEmpty())
            .SkipDirectory(Pattern.Any(".git", ".vs", PatternOptions.Equals | PatternOptions.Literal))
            .Delete(d => d
                .ContentOnly()
                .DryRun()
                .LogOperation(o => Console.WriteLine(o.Path)))
            .Run(directoryPath, CancellationToken.None);

        Console.WriteLine(result.Telemetry.MatchingDirectoryCount);

        var search = new Search(
            new DirectoryMatcher()
            {
                Name = new Matcher(@"\A(bin|obj)\z"),
                EmptyOption = FileEmptyOption.NonEmpty,
            },
            new SearchOptions()
            {
                SearchDirectory = new DirectoryMatcher()
                {
                    Name = new Matcher(@"\A(\.git|\.vs)\z", invert: true),
                }
            });

        result = search.Delete(
            directoryPath,
            new DeleteOptions()
            {
                ContentOnly = true,
                DryRun = true,
                LogOperation = o => Console.WriteLine(o.Path),
            },
            CancellationToken.None);

        Console.WriteLine(result.Telemetry.MatchingDirectoryCount);
    }
}
