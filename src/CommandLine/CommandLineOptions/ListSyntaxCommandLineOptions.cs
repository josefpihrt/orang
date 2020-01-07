// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using CommandLine;
using Orang.Syntax;

namespace Orang.CommandLine
{
    [Verb("list-syntax", HelpText = "Lists regular expression syntax.")]
    internal sealed class ListSyntaxCommandLineOptions : CommonListCommandLineOptions
    {
        [Option(shortName: OptionShortNames.Section, longName: OptionNames.Section,
            HelpText = "Syntax sections to filter.",
            MetaValue = MetaValues.SyntaxSections)]
        public IEnumerable<string> Section { get; set; }

        public bool TryParse(ListSyntaxCommandOptions options)
        {
            var baseOptions = (CommonListCommandOptions)options;

            if (!TryParse(baseOptions))
                return false;

            options = (ListSyntaxCommandOptions)baseOptions;

            if (!ParseHelpers.TryParseAsEnumValues(Section, OptionNames.Section, out ImmutableArray<SyntaxSection> sections, provider: OptionValueProviders.SyntaxSectionProvider))
                return false;

            options.Sections = sections;

            return true;
        }
    }
}
