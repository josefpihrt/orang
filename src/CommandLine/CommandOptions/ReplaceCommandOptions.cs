// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.CommandLine
{
    internal sealed class ReplaceCommandOptions : FindCommandOptions
    {
        internal ReplaceCommandOptions()
        {
        }

        public string Input { get; internal set; }

        public bool DryRun { get; internal set; }

        public ReplaceOptions ReplaceOptions { get; internal set; }
    }
}
