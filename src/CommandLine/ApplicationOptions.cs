// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.CommandLine
{
    internal class ApplicationOptions
    {
        public static ApplicationOptions Default { get; } = new ApplicationOptions();

        public ApplicationOptions(string? contentIndent = null)
        {
            ContentIndent = contentIndent ?? "  ";
        }

        public string ContentIndent { get; }
    }
}
