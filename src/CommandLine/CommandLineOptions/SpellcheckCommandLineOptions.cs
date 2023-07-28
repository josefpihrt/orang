// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using CommandLine;
using Orang.CommandLine.Annotations;
using Orang.Spelling;

namespace Orang.CommandLine;

[Verb("spellcheck", HelpText = "Searches the files' content for potential misspellings and typos.")]
[CommandGroup("File System", 1)]
#if DEBUG
[OptionValueProvider(nameof(Word), OptionValueProviderNames.PatternOptions_Word)]
#endif
internal sealed class SpellcheckCommandLineOptions : CommonReplaceCommandLineOptions
{
    [Option(
        longName: OptionNames.CaseSensitive,
        HelpText = "Specifies case-sensitive matching.")]
    public bool CaseSensitive { get; set; }

    [Option(
        shortName: OptionShortNames.Content,
        longName: OptionNames.Content,
        HelpText = "Regular expression for files' content.",
        MetaValue = MetaValues.Regex)]
    public override IEnumerable<string> Content { get; set; } = null!;

    [Option(
        longName: OptionNames.MaxWordLength,
        Default = int.MaxValue,
        HelpText = "Specifies maximal word length to be checked.")]
    public int MaxWordLength { get; set; }

    [Option(
        longName: OptionNames.MinWordLength,
        Default = 3,
        HelpText = "Specifies minimal word length to be checked. Default value is 3.")]
    public int MinWordLength { get; set; }

    [Option(
        longName: OptionNames.Words,
        Required = true,
        HelpText = "Specified path to file and/or directory that contains list of allowed words.",
        MetaValue = MetaValues.Path)]
    public IEnumerable<string> Words { get; set; } = null!;
#if DEBUG
    [Option(
        longName: OptionNames.Word)]
    public IEnumerable<string> Word { get; set; } = null!;
#endif
    public bool TryParse(SpellcheckCommandOptions options, ParseContext context)
    {
        var baseOptions = (CommonReplaceCommandOptions)options;

        if (!TryParse(baseOptions, context))
            return false;

        options = (SpellcheckCommandOptions)baseOptions;

        Regex? wordRegex = null;

        if (!context.TryEnsureFullPath(Words, out ImmutableArray<string> wordListPaths))
            return false;
#if DEBUG
        if (Word.Any())
        {
            if (!context.TryParseFilter(
                Word,
                OptionNames.Word,
                OptionValueProviders.PatternOptions_Word_Provider,
                out Matcher? wordFilter))
            {
                return false;
            }

            wordRegex = wordFilter!.Regex;
        }
#endif
        WordListLoaderResult result = WordListLoader.Load(
            wordListPaths,
            minWordLength: MinWordLength,
            maxWordLength: MaxWordLength,
            (CaseSensitive) ? WordListLoadOptions.None : WordListLoadOptions.IgnoreCase);

        var data = new SpellingData(result.List, result.CaseSensitiveList, result.FixList);

        var spellcheckerOptions = new SpellcheckerOptions(
            SplitMode.CaseAndHyphen,
            MinWordLength,
            MaxWordLength);

        var spellchecker = new Spellchecker(data, wordRegex, spellcheckerOptions);

        options.Replacer = new SpellcheckState(spellchecker, data);

        return true;
    }

    protected override Matcher? GetDefaultContentFilter()
    {
        return Matcher.EntireInput;
    }
}
