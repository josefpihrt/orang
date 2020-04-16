// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

namespace Orang
{
    public class HelpWriter
    {
        public HelpWriter(TextWriter writer, HelpWriterOptions options = null, IEnumerable<OptionValueProvider> optionValueProviders = default)
        {
            Writer = writer;
            Options = options ?? HelpWriterOptions.Default;
            OptionValueProviders = optionValueProviders?.ToImmutableArray() ?? ImmutableArray<OptionValueProvider>.Empty;
        }

        public TextWriter Writer { get; }

        public HelpWriterOptions Options { get; }

        public ImmutableArray<OptionValueProvider> OptionValueProviders { get; }

        public virtual void WriteCommand(Command command)
        {
            WriteStartCommand(command);

            ImmutableArray<CommandArgument> arguments = command.Arguments;

            if (arguments.Any())
            {
                WriteStartArguments(command);
                WriteArguments(arguments);
                WriteEndArguments(command);
            }

            ImmutableArray<CommandOption> options = command.Options;

            if (options.Any())
            {
                WriteStartOptions(command);
                WriteOptions(options);
                WriteEndOptions(command);
            }

            WriteEndCommand(command);
        }

        public virtual void WriteStartCommand(Command command)
        {
        }

        public virtual void WriteEndCommand(Command command)
        {
        }

        private void WriteOptions(ImmutableArray<CommandOption> options)
        {
            bool anyIsOptional = options.Any(f => !f.IsRequired);
            bool anyHasShortName = options.Any(f => !string.IsNullOrEmpty(f.ShortName));

            int maxWidth1 = options.Max(f => f.Name.Length) + 3;

            if (anyIsOptional)
                maxWidth1 += 2;

            if (anyHasShortName)
                maxWidth1 += 4;

            int maxWidth2 = options.Select(f => f.MetaValue?.Length ?? 0).DefaultIfEmpty().Max();

            var sb = new StringBuilder();

            foreach (CommandOption option in options)
            {
                Write(Options.Indent);

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

                Write(sb.ToString());

                WriteSpaces(maxWidth1 - sb.Length);

                sb.Clear();

                if (!string.IsNullOrEmpty(option.MetaValue))
                {
                    Write(option.MetaValue);
                    WriteSpaces(maxWidth2 - option.MetaValue.Length);
                }
                else
                {
                    WriteSpaces(maxWidth2);
                }

                Write(" ");
                Write(option.Description);
                WriteLine();
            }
        }

        public virtual void WriteStartOptions(Command command)
        {
            WriteLine();
            WriteHeading("Options");
        }

        public virtual void WriteEndOptions(Command command)
        {
        }

        private void WriteArguments(ImmutableArray<CommandArgument> arguments)
        {
            bool anyIsOptional = arguments.Any(f => !f.IsRequired);

            int maxWidth = arguments.Max(f => f.Name.Length) + 1;

            foreach (CommandArgument argument in arguments)
            {
                Write(Options.Indent);

                if (!argument.IsRequired)
                {
                    Write("[");
                }
                else if (anyIsOptional)
                {
                    Write(" ");
                }

                Write(argument.Name);

                if (!argument.IsRequired)
                    Write("]");

                if (!string.IsNullOrEmpty(argument.Description))
                {
                    WriteSpaces(maxWidth - (argument.Name.Length) + ((argument.IsRequired) ? 1 : 0));
                    WriteLine(argument.Description);
                }
            }
        }

        public virtual void WriteStartArguments(Command command)
        {
            WriteLine();
            WriteHeading("Arguments");
        }

        public virtual void WriteEndArguments(Command command)
        {
        }

        public virtual void WriteCommands(IEnumerable<Command> commands)
        {
            WriteStartCommands(commands);

            int maxWidth = commands.Max(f => f.Name.Length) + 1;

            foreach (Command command in commands)
            {
                Write(Options.Indent);
                Write(command.Name);
                WriteSpaces(maxWidth - command.Name.Length);
                WriteLine(command.Description);
            }

            WriteEndCommands(commands);
        }

        public virtual void WriteStartCommands(IEnumerable<Command> commands)
        {
            WriteHeading("Commands");
        }

        public virtual void WriteEndCommands(IEnumerable<Command> commands)
        {
        }

        public void WriteValues(IEnumerable<Command> commands)
        {
            WriteValues(commands.SelectMany(f => f.Options));
        }

        protected virtual void WriteValues(IEnumerable<CommandOption> options)
        {
            IEnumerable<OptionValueProvider> providers = options
                .Join(OptionValueProviders, f => f.MetaValue, f => f.Name, (_, f) => f)
                .Distinct(OptionValueProviderEqualityComparer.ByName);

            WriteValues(providers);
        }

        protected void WriteValues(IEnumerable<OptionValueProvider> providers)
        {
            using (IEnumerator<OptionValueProvider> en = providers.OrderBy(f => f.Name).GetEnumerator())
            {
                if (en.MoveNext())
                {
                    WriteStartValues();

                    IEnumerable<OptionValue> optionValues = providers.SelectMany(p => p.Values.Where(v => !v.Hidden));

                    (int width1, int width2) = CalculateColumnWidths(optionValues);

                    while (true)
                    {
                        WriteLine(en.Current.Name);

                        WriteValues(en.Current.Values.Where(v => !v.Hidden), width1, width2);

                        WriteLine();

                        if (en.MoveNext())
                        {
                            WriteLine();
                        }
                        else
                        {
                            break;
                        }
                    }

                    WriteEndValues();

                    if (optionValues.Any(f => f.Description?.Contains("(See 'Expression syntax' for other expressions)") == true))
                    {
                        WriteLine();
                        WriteLine("Expression syntax:");
                        WriteLine(GetExpressionSyntax(Options.Indent));
                    }
                }
            }
        }

        public void WriteValues(IEnumerable<OptionValue> optionValues)
        {
            (int width1, int width2) = CalculateColumnWidths(optionValues);

            WriteValues(optionValues, width1, width2);
        }

        private void WriteValues(IEnumerable<OptionValue> values, int width1, int width2)
        {
            using (IEnumerator<OptionValue> en = values.GetEnumerator())
            {
                if (en.MoveNext())
                {
                    while (true)
                    {
                        Write(Options.Indent);

                        string value = GetValue(en.Current);

                        Write(value);
                        WriteSpaces(width1 - value.Length);

                        string shortValue = GetShortValue(en.Current);

                        if (string.IsNullOrEmpty(shortValue))
                            shortValue = "-";

                        Write(shortValue);

                        string description = en.Current.Description;

                        if (!string.IsNullOrEmpty(description))
                        {
                            WriteSpaces(width2 - shortValue.Length);
                            Write(description);
                        }

                        if (en.MoveNext())
                        {
                            WriteLine();
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

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

        private static (int width1, int width2) CalculateColumnWidths(IEnumerable<OptionValue> optionValues)
        {
            int width1 = optionValues.Max(f =>
            {
                return f switch
                {
                    SimpleOptionValue enumOptionValue => enumOptionValue.Value.Length,
                    KeyValuePairOptionValue keyOptionValue => keyOptionValue.Key.Length + 1 + keyOptionValue.Value.Length,
                    _ => throw new InvalidOperationException(),
                };
            }) + 1;
            int width2 = optionValues.Max(f =>
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "RCS1197")]
        internal static string GetExpressionSyntax(string indent, bool includeDate = true)
        {
            StringBuilder sb = StringBuilderCache.GetInstance()
                .AppendLine($"{indent}x=n")
                .AppendLine($"{indent}x<n")
                .AppendLine($"{indent}x>n")
                .AppendLine($"{indent}x<=n")
                .AppendLine($"{indent}x>=n")
                .AppendLine($"{indent}x=<min;max>          Inclusive interval")
                .Append($"{indent}x=(min;max)          Exclusive interval");

            if (includeDate)
            {
                sb.AppendLine();
                sb.Append($"{indent}x=-d|[d.]hh:mm[:ss]  x is greater than actual date - <VALUE>");
            }

            return StringBuilderCache.GetStringAndFree(sb);
        }

        public virtual void WriteStartValues()
        {
            WriteLine();
            WriteHeading("Values");
        }

        public virtual void WriteEndValues()
        {
        }

        private void WriteHeading(string value)
        {
            Write(value);
            WriteLine(":");
        }

        protected void Write(string value)
        {
            Writer.Write(value);
        }

        protected void WriteLine()
        {
            Writer.WriteLine();
        }

        protected void WriteLine(string value)
        {
            Writer.WriteLine(value);
        }

        protected void WriteSpaces(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Writer.Write(' ');
            }
        }
    }
}
