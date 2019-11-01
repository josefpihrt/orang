// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.RegularExpressions;

namespace Orang.CommandLine
{
    internal class RenameCommandOptions : CommonFindCommandOptions
    {
        internal RenameCommandOptions()
        {
        }

        public string Replacement { get; internal set; }

        public bool Ask { get; internal set; }

        public bool DryRun { get; internal set; }

        public MatchEvaluator MatchEvaluator { get; internal set; }
    }
}
