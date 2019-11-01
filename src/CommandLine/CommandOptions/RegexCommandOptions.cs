// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.CommandLine
{
    internal abstract class RegexCommandOptions : CommonRegexCommandOptions
    {
        internal RegexCommandOptions()
        {
        }

        public Filter Filter { get; internal set; }

        public string Input { get; internal set; }

        public int MaxCount { get; internal set; }

        public OutputDisplayFormat Format { get; internal set; }

        public ContentDisplayStyle ContentDisplayStyle => Format.ContentDisplayStyle;

        public string Separator => Format.Separator;
    }
}
