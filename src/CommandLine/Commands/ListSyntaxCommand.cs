// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;
using Orang.Syntax;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal class ListSyntaxCommand : AbstractCommand<ListSyntaxCommandOptions>
    {
        public ListSyntaxCommand(ListSyntaxCommandOptions options) : base(options)
        {
        }

        protected override CommandResult ExecuteCore(CancellationToken cancellationToken = default)
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("Resources", "RegexSyntax.xml"));

            IEnumerable<SyntaxItem> items = XDocument.Load(path)
                .Root
                .Elements()
                .Select(f => new SyntaxItem(
                    f.Element("Text").Value,
                    (SyntaxSection)Enum.Parse(typeof(SyntaxSection), f.Element("Category").Value),
                    f.Element("Description").Value));

            string filter = Options.Filter;

            ImmutableArray<SyntaxSection> sections = Options.Sections;

            if (!string.IsNullOrEmpty(filter))
            {
                items = items.Where(f => f.Text.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                    || f.Description.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (sections.Any())
            {
                items = items.Join(sections, f => f.Section, f => f, (f, _) => f);
            }
            else
            {
                items = items.Where(f => f.Section != SyntaxSection.GeneralCategories && f.Section != SyntaxSection.NamedBlocks);
            }

            List<SyntaxItem> list;
            if (sections.Any())
            {
                list = items.GroupBy(f => f.Section).OrderBy(f => sections.IndexOf(f.Key)).SelectMany(f => f).ToList();
            }
            else
            {
                list = items.GroupBy(f => f.Section).OrderBy(f => f.Key).SelectMany(f => f).ToList();
            }

            if (list.Count > 0)
            {
                int width = list.Max(f => f.Text.Length - f.Text.Count(ch => ch == '%'));

                foreach (IGrouping<SyntaxSection, SyntaxItem> grouping in list.GroupBy(f => f.Section))
                {
                    WriteLine();
                    WriteLine(TextHelpers.SplitCamelCase(grouping.Key.ToString()).ToUpper());

                    foreach (SyntaxItem item in grouping)
                    {
                        Write(" ");

                        string text = item.Text;
                        int length = text.Length;
                        int prevIndex = 0;

                        for (int i = 0; i < length; i++)
                        {
                            if (text[i] == '%')
                            {
                                Write(text, prevIndex, i - prevIndex, Colors.Syntax);

                                int j = i + 1;

                                while (j < length
                                    && text[j] != '%')
                                {
                                    j++;
                                }

                                Write(text, i + 1, j - i - 1);

                                i = j;
                                prevIndex = j + 1;
                            }
                        }

                        Write(text, prevIndex, length - prevIndex, Colors.Syntax);
                        Write(' ', width - text.Length + text.Count(ch => ch == '%'));
                        Write(" ");
                        WriteLine(item.Description);
                    }
                }
            }
            else
            {
                WriteLine($"No syntax matches filter '{filter}'");
            }

            return CommandResult.Success;
        }
    }
}
