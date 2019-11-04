﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using static Orang.CommandLine.ParseHelpers;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal abstract class RegexCommandLineOptions : CommonRegexCommandLineOptions
    {
        [Value(index: 0,
            HelpText = "Path to a file that should be analyzed.",
            MetaName = ArgumentMetaNames.Path)]
        public string Path { get; set; }

        [Option(shortName: OptionShortNames.Display, longName: OptionNames.Display,
            HelpText = "Display of the results.",
            MetaValue = MetaValues.DisplayOptions)]
        public IEnumerable<string> Display { get; set; }

        [Option(longName: OptionNames.Input,
            HelpText = "Text to search.",
            MetaValue = MetaValues.Input)]
        public string Input { get; set; }

        [Option(shortName: OptionShortNames.Output, longName: OptionNames.Output,
            HelpText = "Path to a file that should store results.",
            MetaValue = "<PATH>")]
        public string Output { get; set; }

        public bool TryParse(ref RegexCommandOptions options)
        {
            var baseOptions = (CommonRegexCommandOptions)options;

            if (!TryParse(ref baseOptions))
                return false;

            options = (RegexCommandOptions)baseOptions;

            string input = Input;

            if (!string.IsNullOrEmpty(Path))
            {
                if (input != null)
                {
                    WriteError($"Option '{OptionNames.GetHelpText(OptionNames.Input)}' and argument '{ArgumentMetaNames.Path}' cannot be set both at the same time.");
                    return false;
                }

                try
                {
                    input = File.ReadAllText(Path);
                }
                catch (Exception ex) when (ex is ArgumentException
                    || ex is IOException
                    || ex is UnauthorizedAccessException)
                {
                    WriteError(ex);
                    return false;
                }
            }
            else if (input == null)
            {
                WriteError("Specify either path to a file or text input.");
                return false;
            }

            string outputPath = null;

            if (Output != null
                && !TryEnsureFullPath(Output, out outputPath))
            {
                return false;
            }

            if (!TryParseDisplay(
                values: Display,
                optionName: OptionNames.Display,
                contentDisplayStyle: out ContentDisplayStyle contentDisplayStyle,
                pathDisplayStyle: out PathDisplayStyle _,
                defaultContentDisplayStyle: ContentDisplayStyle.Value,
                defaultPathDisplayStyle: 0,
                contentDisplayStyleProvider: OptionValueProviders.ContentDisplayStyleProvider_WithoutLineAndUnmatchedLines,
                pathDisplayStyleProvider: OptionValueProviders.PathDisplayStyleProvider))
            {
                return false;
            }

            options.Format = new OutputDisplayFormat(contentDisplayStyle: contentDisplayStyle);
            options.Input = input;
            options.OutputPath = outputPath;

            return true;
        }
    }
}
