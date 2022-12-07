// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Orang;
using Orang.FileSystem;
using Orang.FileSystem.Fluent;

namespace N;

public static class Program
{
    public static void Main(params string[] args)
    {
        IOperationResult result = SearchBuilder.Create()
            .DirectoryName(Pattern.From("bin", "obj", PatternOptions.Equals))
            .SkipDirectory(Pattern.From(".git", ".vs", PatternOptions.Equals))
            .Delete("c:/code/datamole/ddp-kernel-device-provisioning")
            .ContentOnly()
            .DryRun()
            .LogOperation(o => Console.WriteLine(o.Path))
            .Execute(CancellationToken.None);

        Console.WriteLine(result.Telemetry.DirectoryCount);
        Console.WriteLine(result.Telemetry.MatchingDirectoryCount);

        var search = new Search(
            new DirectoryMatcher()
            {
                Name = new Matcher(Pattern.From(new[] { "bin", "obj" }, PatternOptions.Equals)),
            },
            new SearchOptions()
            {
                SearchDirectory = new DirectoryMatcher()
                {
                    Name = new Matcher(Pattern.From(new[] { ".git", ".vs" }, PatternOptions.Equals), isNegative: true)
                }
            });

        result = search.Delete(
            @"C:\code\datamole\ddp-kernel-device-provisioning",
            new DeleteOptions()
            {
                ContentOnly = true,
                DryRun = true,
                LogOperation = o => Console.WriteLine(o.Path),
            },
            CancellationToken.None);

        Console.WriteLine(result.Telemetry.DirectoryCount);
        Console.WriteLine(result.Telemetry.MatchingDirectoryCount);
    }

    public class ConsoleSearchProgress : IProgress<SearchProgress>
    {
        public void Report(SearchProgress value)
        {
            ConsoleColor tmp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(value.Path);
            Console.ForegroundColor = tmp;
        }
    }

    public class ConsoleOperationProgress : IProgress<OperationProgress>
    {
        public void Report(OperationProgress value)
        {
            Console.WriteLine(value.FileMatch.Path);
        }
    }
}
