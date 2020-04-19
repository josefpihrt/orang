// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Orang
{
    internal static class HelpProvider
    {
        public static ImmutableArray<CommandShortHelp> GetCommandShortHelp(IEnumerable<Command> commands, Filter filter = null)
        {
            if (!commands.Any())
                return ImmutableArray<CommandShortHelp>.Empty;

            int width = commands.Max(f => f.Name.Length) + 1;

            ImmutableArray<CommandShortHelp>.Builder builder = ImmutableArray.CreateBuilder<CommandShortHelp>();

            foreach (Command command in commands)
            {
                StringBuilder sb = StringBuilderCache.GetInstance();

                sb.Append(command.Name);
                sb.AppendSpaces(width - command.Name.Length);
                sb.Append(command.Description);

                string text = StringBuilderCache.GetStringAndFree(sb);

                if (filter?.IsMatch(text) != false)
                    builder.Add(new CommandShortHelp(command, text));
            }

            return builder.ToImmutableArray();
        }

        public static ImmutableArray<ArgumentHelp> GetArgumentsHelp(IEnumerable<CommandArgument> arguments, Filter filter = null)
        {
            int width = CalculateArgumentsWidths(arguments);

            ImmutableArray<ArgumentHelp>.Builder builder = ImmutableArray.CreateBuilder<ArgumentHelp>();

            bool anyIsOptional = arguments.Any(f => !f.IsRequired);

            foreach (CommandArgument argument in arguments)
            {
                StringBuilder sb = StringBuilderCache.GetInstance();

                if (!argument.IsRequired)
                {
                    sb.Append("[");
                }
                else if (anyIsOptional)
                {
                    sb.Append(" ");
                }

                sb.Append(argument.Name);

                if (!argument.IsRequired)
                    sb.Append("]");

                if (!string.IsNullOrEmpty(argument.Description))
                {
                    sb.AppendSpaces(width - argument.Name.Length);
                    sb.Append(argument.Description);
                }

                builder.Add(new ArgumentHelp(argument, StringBuilderCache.GetStringAndFree(sb)));
            }

            return (filter != null)
                ? builder.Where(f => filter.IsMatch(f.Text)).ToImmutableArray()
                : builder.ToImmutableArray();
        }

        public static ImmutableArray<OptionHelp> GetOptionsHelp(IEnumerable<CommandOption> options, Filter filter = null)
        {
            (int width1, int width2) = CalculateOptionsWidths(options);

            bool anyIsOptional = options.Any(f => !f.IsRequired);
            bool anyHasShortName = options.Any(f => !string.IsNullOrEmpty(f.ShortName));

            ImmutableArray<OptionHelp>.Builder builder = ImmutableArray.CreateBuilder<OptionHelp>();

            foreach (CommandOption option in options)
            {
                StringBuilder sb = StringBuilderCache.GetInstance();

                if (!option.IsRequired)
                {
                    sb.Append("[");
                }
                else if (anyIsOptional)
                {
                    sb.Append(" ");
                }

                if (!string.IsNullOrEmpty(option.ShortName))
                {
                    sb.Append("-");
                    sb.Append(option.ShortName);
                    sb.Append(", ");
                }
                else if (anyHasShortName)
                {
                    sb.Append(' ', 4);
                }

                sb.Append("--");
                sb.Append(option.Name);

                if (!option.IsRequired)
                    sb.Append("]");

                sb.AppendSpaces(width1 - sb.Length);

                if (!string.IsNullOrEmpty(option.MetaValue))
                {
                    sb.Append(option.MetaValue);
                    sb.AppendSpaces(width2 - option.MetaValue.Length);
                }
                else
                {
                    sb.AppendSpaces(width2);
                }

                sb.Append(" ");
                sb.Append(option.Description);

                builder.Add(new OptionHelp(option, StringBuilderCache.GetStringAndFree(sb)));
            }

            return (filter != null)
                ? builder.Where(f => filter.IsMatch(f.Text)).ToImmutableArray()
                : builder.ToImmutableArray();
        }

        public static ImmutableArray<OptionValuesHelp> GetOptionValuesHelp(
            IEnumerable<CommandOption> options,
            IEnumerable<OptionValueProvider> providers,
            Filter filter = null)
        {
            providers = OptionValueProvider.GetProviders(options, providers).ToImmutableArray();

            IEnumerable<OptionValue> allOptionValues = providers.SelectMany(p => p.Values.Where(v => !v.Hidden));

            ImmutableArray<OptionValuesHelp>.Builder builder = ImmutableArray.CreateBuilder<OptionValuesHelp>();

            (int width1, int width2) = CalculateOptionValuesWidths(allOptionValues);

            foreach (OptionValueProvider provider in providers)
            {
                IEnumerable<OptionValue> optionValues = provider.Values.Where(f => !f.Hidden);
                ImmutableArray<OptionValueHelp> values = GetOptionValueHelp(optionValues, width1, width2);

                builder.Add(new OptionValuesHelp(provider.Name, values));
            }

            return (filter != null)
                ? FilterOptionValues(builder, filter)
                : builder.ToImmutableArray();
        }

        public static ImmutableArray<OptionValueHelp> GetOptionValueHelp(IEnumerable<OptionValue> optionValues, int width1, int width2)
        {
            ImmutableArray<OptionValueHelp>.Builder builder = ImmutableArray.CreateBuilder<OptionValueHelp>();

            foreach (OptionValue optionValue in optionValues)
            {
                StringBuilder sb = StringBuilderCache.GetInstance();

                string value = GetValue(optionValue);

                sb.Append(value);
                sb.AppendSpaces(width1 - value.Length);

                string shortValue = GetShortValue(optionValue);

                if (string.IsNullOrEmpty(shortValue))
                    shortValue = "-";

                sb.Append(shortValue);

                string description = optionValue.Description;

                if (!string.IsNullOrEmpty(description))
                {
                    sb.AppendSpaces(width2 - shortValue.Length);
                    sb.Append(description);
                }

                builder.Add(new OptionValueHelp(optionValue, StringBuilderCache.GetStringAndFree(sb)));
            }

            return builder.ToImmutableArray();

            static string GetValue(OptionValue value)
            {
                return value switch
                {
                    SimpleOptionValue enumOptionValue => enumOptionValue.Value,
                    KeyValuePairOptionValue keyOptionValue => $"{keyOptionValue.Key}={keyOptionValue.Value}",
                    _ => throw new InvalidOperationException(),
                };
            }

            static string GetShortValue(OptionValue value)
            {
                return value switch
                {
                    SimpleOptionValue enumOptionValue => enumOptionValue.ShortValue,
                    KeyValuePairOptionValue keyOptionValue => keyOptionValue.ShortKey,
                    _ => throw new InvalidOperationException(),
                };
            }
        }

        public static ImmutableArray<OptionValuesHelp> FilterOptionValues(IEnumerable<OptionValuesHelp> values, Filter filter)
        {
            ImmutableArray<OptionValuesHelp>.Builder builder = ImmutableArray.CreateBuilder<OptionValuesHelp>();

            foreach (OptionValuesHelp optionValues in values)
            {
                if (filter.IsMatch(optionValues.MetaValue))
                {
                    builder.Add(optionValues);
                }
                else
                {
                    ImmutableArray<OptionValueHelp> values2 = optionValues.Values
                        .Where(f => filter.IsMatch(f.Text))
                        .ToImmutableArray();

                    if (values2.Any())
                        builder.Add(new OptionValuesHelp(optionValues.MetaValue, values2));
                }
            }

            return builder.ToImmutableArray();
        }

        public static ImmutableArray<string> GetExpressionsLines(IEnumerable<OptionValuesHelp> values, bool includeDate = true)
        {
            return (values.SelectMany(f => f.Values).Any(f => f.Value.CanContainExpression))
                ? GetExpressionsLines(includeDate: includeDate)
                : ImmutableArray<string>.Empty;
        }

        public static string GetExpressionsText(string indent, bool includeDate = true)
        {
            ImmutableArray<string> expressions = GetExpressionsLines(includeDate: includeDate);

            return indent + string.Join(Environment.NewLine + indent, expressions);
        }

        public static ImmutableArray<string> GetExpressionsLines(bool includeDate = true)
        {
            ImmutableArray<string>.Builder builder = ImmutableArray.CreateBuilder<string>();

            builder.Add("x=n");
            builder.Add("x<n");
            builder.Add("x>n");
            builder.Add("x<=n");
            builder.Add("x>=n");
            builder.Add("x=<min;max>          Inclusive interval");
            builder.Add("x=(min;max)          Exclusive interval");

            if (includeDate)
                builder.Add("x=-d|[d.]hh:mm[:ss]  x is greater than actual date - <VALUE>");

            return builder.ToImmutableArray();
        }

        public static int CalculateArgumentsWidths(IEnumerable<CommandArgument> arguments)
        {
            return (arguments.Any())
                ? arguments.Max(f => f.Name.Length + ((f.IsRequired) ? 0 : 2)) + 1
                : default;
        }

        public static (int width1, int width2) CalculateOptionsWidths(IEnumerable<CommandOption> options)
        {
            if (!options.Any())
                return default;

            int width1 = options.Max(f => f.Name.Length + ((f.IsRequired) ? 0 : 2)) + 3;

            if (options.Any(f => !string.IsNullOrEmpty(f.ShortName)))
                width1 += 4;

            int width2 = options.Select(f => f.MetaValue?.Length ?? 0).DefaultIfEmpty().Max();

            return (width1, width2);
        }

        public static (int width1, int width2) CalculateOptionValuesWidths(IEnumerable<OptionValue> optionValues)
        {
            if (!optionValues.Any())
                return default;

            int width1 = optionValues.DefaultIfEmpty().Max(f =>
            {
                return f switch
                {
                    SimpleOptionValue enumOptionValue => enumOptionValue.Value.Length,
                    KeyValuePairOptionValue keyOptionValue => keyOptionValue.Key.Length + 1 + keyOptionValue.Value.Length,
                    _ => throw new InvalidOperationException(),
                };
            }) + 1;
            int width2 = optionValues.DefaultIfEmpty().Max(f =>
            {
                return f switch
                {
                    SimpleOptionValue enumOptionValue => enumOptionValue.ShortValue.Length,
                    KeyValuePairOptionValue keyOptionValue => keyOptionValue.ShortKey.Length,
                    _ => throw new InvalidOperationException(),
                };
            }) + 1;

            return (width1, width2);
        }
    }
}
