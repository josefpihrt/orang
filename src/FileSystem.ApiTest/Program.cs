// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Orang;
using Orang.FileSystem;

namespace N;

public static class Program
{
    public static void Main(params string[] args)
    {
        foreach (FileMatch match in Operation.GetMatches(
            @"C:\code\datamole\ddp-kernel-device-provisioning\src",
            new FileSystemFilter() { Extension = new Filter("csproj") }))
        {
            Console.WriteLine(match.Path);
        }

        Console.WriteLine();

        IOperationResult result = Operation.RenameMatches(
            @"C:\code\datamole\ddp-kernel-device-provisioning\src",
            new FileSystemFilter()
            {
                Name = new Filter(Pattern.FromText("csproj", PatternCreationOptions.Equals), RegexOptions.IgnoreCase),
                Part = FileNamePart.Extension,
            },
            m => m.Value + "2",
            new RenameOptions()
            {
                DryRun = true,
                OperationProgress = new ConsoleOperationProgress(),
                //SearchProgress = new ConsoleSearchProgress()
            });

        Console.WriteLine(result.Telemetry.MatchingFileCount);
        Console.WriteLine(result.Telemetry.ProcessedFileCount);
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
            Console.WriteLine("  " + value.NewPath);
        }
    }
}
