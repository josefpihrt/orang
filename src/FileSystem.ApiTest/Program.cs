// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;
using System.Threading;
using Orang;
using Orang.FileSystem;

namespace N;

public static class Program
{
    public static void Main(params string[] args)
    {
        //foreach (FileMatch match in FileSystemOperation.GetMatches(
        //    @"C:\code\datamole\ddp-kernel-device-provisioning\src",
        //    new MatchOptions() { Extension = new Filter("csproj") }))
        //{
        //    Console.WriteLine(match.Path);
        //}

        //Console.WriteLine();

        IOperationResult result = FileSystemOperation.RenameMatches(
            @"C:\code\datamole\ddp-kernel-device-provisioning\src",
            new FileSystemFilter() { Name = new Filter("^csproj$"), Part = FileNamePart.Extension },
            f => "cspro",
            new RenameOptions() { DryRun = true, Progress = new ConsoleProgress() });
    }

    public class ConsoleProgress : IProgress<OperationProgress>
    {
        public void Report(OperationProgress value)
        {
            Console.WriteLine(value.FileMatch.Path);
            Console.WriteLine("  " + value.NewPath);
        }
    }
}
