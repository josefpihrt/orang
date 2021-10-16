// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.CommandLine
{
    internal sealed class RegexEscapeCommandOptions : AbstractCommandOptions
    {
        internal RegexEscapeCommandOptions()
        {
        }

        public string Input { get; internal set; } = null!;

        public bool InCharGroup { get; internal set; }

        public bool Replacement { get; internal set; }

        protected override void WriteDiagnosticCore(DiagnosticWriter writer)
        {
            writer.WriteRegexEscapeCommand(this);
        }
    }
}
