// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using Markdig;
using Markdig.Extensions.CustomContainers;
using Markdig.Extensions.Tables;
using Markdig.Helpers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Orang.CommandLine;

internal static class MarkdownProcessor
{
    private static readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    public static IEnumerable<CaptureSlim> ProcessText(string text)
    {
        MarkdownDocument document = Markdown.Parse(text, _pipeline);

        foreach (MarkdownObject item in document.Descendants())
        {
            switch (item)
            {
                case CodeInline code:
                    {
                        yield return new CaptureSlim(code.Content, code.Span.Start + code.DelimiterCount, code.Span.Length);
                        break;
                    }
                case LiteralInline literal:
                    {
                        string value = literal.Content.ToString();
                        SourceSpan span = literal.Span;
                        int offset = (literal.IsFirstCharacterEscaped) ? 1 : 0;

                        yield return new CaptureSlim(value, span.Start + offset, span.Length + offset);
                        break;
                    }
                case LinkInline link:
                    {
                        string? label = link.Label;
                        string? title = link.Title;

                        if (!string.IsNullOrEmpty(label))
                            yield return new CaptureSlim(label, link.LabelSpan.Start, link.LabelSpan.Length);

                        if (!string.IsNullOrEmpty(title))
                            yield return new CaptureSlim(title, link.TitleSpan.Start, link.TitleSpan.Length);

                        break;
                    }
                case LinkReferenceDefinition linkReferenceDef:
                    {
                        string? label = linkReferenceDef.Label;
                        string? title = linkReferenceDef.Title;

                        if (!string.IsNullOrEmpty(label))
                            yield return new CaptureSlim(label, linkReferenceDef.LabelSpan.Start, linkReferenceDef.LabelSpan.Length);

                        if (!string.IsNullOrEmpty(title))
                            yield return new CaptureSlim(title, linkReferenceDef.TitleSpan.Start, linkReferenceDef.TitleSpan.Length);

                        break;
                    }
                case CodeBlock codeBlock:
                    {
                        foreach (StringLine line in codeBlock.Lines.Lines)
                        {
                            StringSlice slice = line.Slice;
                            yield return new CaptureSlim(slice.ToString(), slice.Start, slice.Length);
                        }

                        break;
                    }
                case ContainerInline: // EmphasisInline, DelimiterInline, EmphasisDelimiterInline, LinkDelimiterInline
                case AutolinkInline:
                case HtmlEntityInline:
                case LineBreakInline:
                case HtmlInline:
                case HeadingBlock:
                case ListBlock:
                case ListItemBlock:
                case ParagraphBlock:
                case ThematicBreakBlock:
                case LinkReferenceDefinitionGroup:
                case Table:
                case TableRow:
                case TableCell:
                case QuoteBlock:
                case CustomContainer:
                case HtmlBlock:
                    {
                        break;
                    }
                default:
                    {
                        Debug.Fail(item.GetType().FullName);
                        break;
                    }
            }
        }
    }
}
