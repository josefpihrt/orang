// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.RegularExpressions;

namespace Orang.CommandLine
{
    internal sealed class CreatePatternCommandOptions : AbstractCommandOptions
    {
        internal CreatePatternCommandOptions()
        {
        }

        public string PatternOrPath { get; internal set; } = null!;

        public PatternOptions PatternOptions { get; internal set; }

        public string Separator { get; internal set; } = null!;

        protected override void WriteDiagnosticCore()
        {
            //TODO: 
            //DiagnosticWriter.WriteListPatternsCommand(this);
        }
    }
}
