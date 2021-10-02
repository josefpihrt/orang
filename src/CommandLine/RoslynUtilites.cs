// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal static class RoslynUtilites
    {
        public static Compilation CreateCompilation(SyntaxTree syntaxTree)
        {
            string assemblyName = Path.GetRandomFileName();

            IEnumerable<MetadataReference> metadataReferences = RuntimeMetadataReference.DefaultAssemblyNames
                .Select(f => MetadataReference.CreateFromFile(RuntimeMetadataReference.TrustedPlatformAssemblyMap[f]))
                .Append(RuntimeMetadataReference.CorLibReference);

            return CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new SyntaxTree[] { syntaxTree },
                references: metadataReferences,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        }

        public static SyntaxTree CreateSyntaxTree(
            string expressionText,
            string returnTypeName,
            string parameterType,
            string parameterName,
            string? namespaceName = null,
            string? className = null,
            string? methodName = null)
        {
            namespaceName ??= GetRandomIdentifier("N_");
            className ??= GetRandomIdentifier("C_");
            methodName ??= GetRandomIdentifier("M_");

            ExpressionSyntax expression = ParseExpression(expressionText);

            CompilationUnitSyntax compilationUnit = CompilationUnit(
                default,
                default,
                default,
                SingletonList<MemberDeclarationSyntax>(
                    NamespaceDeclaration(
                        ParseName(namespaceName),
                        default,
                        List(new UsingDirectiveSyntax[]
                            {
                                UsingDirective(ParseName("System")),
                                UsingDirective(ParseName("System.Collections.Generic")),
                                UsingDirective(ParseName("System.Linq")),
                                UsingDirective(ParseName("System.Text")),
                                UsingDirective(ParseName("System.Text.RegularExpressions")),
                            }),
                        SingletonList<MemberDeclarationSyntax>(
                            ClassDeclaration(
                                default,
                                TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)),
                                Identifier(className),
                                default,
                                default,
                                default,
                                SingletonList<MemberDeclarationSyntax>(
                                    MethodDeclaration(
                                        default,
                                        TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)),
                                        ParseTypeName(returnTypeName),
                                        default!,
                                        Identifier(methodName),
                                        default!,
                                        ParameterList(
                                            SingletonSeparatedList(
                                                Parameter(
                                                    default,
                                                    default,
                                                    ParseTypeName(parameterType),
                                                    Identifier(parameterName),
                                                    default))),
                                        default,
                                        default,
                                        ArrowExpressionClause(expression),
                                        Token(SyntaxKind.SemicolonToken))))))));

            compilationUnit = compilationUnit.NormalizeWhitespace();

            return CSharpSyntaxTree.Create(compilationUnit);
        }

        private static ConsoleColors GetColors(DiagnosticSeverity diagnosticSeverity)
        {
            return new ConsoleColors(GetColor(diagnosticSeverity));

            static ConsoleColor GetColor(DiagnosticSeverity diagnosticSeverity)
            {
                return diagnosticSeverity switch
                {
                    DiagnosticSeverity.Hidden => ConsoleColor.DarkGray,
                    DiagnosticSeverity.Info => ConsoleColor.Cyan,
                    DiagnosticSeverity.Warning => ConsoleColor.Yellow,
                    DiagnosticSeverity.Error => ConsoleColor.Red,
                    _ => throw new InvalidOperationException($"Unknown diagnostic severity '{diagnosticSeverity}'."),
                };
            }
        }

        private static void WriteDiagnostic(
            Diagnostic diagnostic,
            IFormatProvider? formatProvider = null,
            string? indentation = null,
            Verbosity verbosity = Verbosity.Diagnostic)
        {
            string text = FormatDiagnostic(diagnostic, formatProvider);

            Write(indentation, verbosity);
            WriteLine(text, GetColors(diagnostic.Severity), verbosity);
        }

        public static void WriteDiagnostic(
            Diagnostic diagnostic,
            SourceText sourceText,
            string indentation,
            Verbosity verbosity)
        {
            WriteDiagnostic(diagnostic, default(IFormatProvider), indentation, verbosity);

            const int codeContext = 1;
            TextSpan span = diagnostic.Location.SourceSpan;
            TextLineCollection lines = sourceText.Lines;
            int lineIndex = lines.IndexOf(span.Start);
            TextLine line = lines[lineIndex];

            int startLineIndex = Math.Max(0, lineIndex - codeContext);
            int endLineIndex = Math.Min(lines.Count - 1, lineIndex + codeContext);

            int maxLineNumberLength = (endLineIndex + 1).ToString().Length;

            for (int i = startLineIndex; i < lineIndex; i++)
                WriteTextLine(i);

            int index = span.Start - line.Span.Start;
            string text = line.ToString();

            Write(indentation, verbosity);
            WriteLineNumber(verbosity, lineIndex, maxLineNumberLength);
            Write(text.Substring(0, index), verbosity);
            Write(text.Substring(index, span.Length), new ConsoleColors(ConsoleColor.Red), verbosity);
            WriteLine(text.Substring(index + span.Length), verbosity);

            for (int i = lineIndex + 1; i <= endLineIndex; i++)
                WriteTextLine(i);

            void WriteTextLine(int i)
            {
                Write(indentation, verbosity);
                WriteLineNumber(verbosity, i, maxLineNumberLength);
                Write(" ", verbosity);
                WriteLine(lines[i].ToString(), new ConsoleColors(ConsoleColor.DarkGray), verbosity);
            }

            static void WriteLineNumber(Verbosity verbosity, int lineIndex, int maxLength)
            {
                Write((lineIndex + 1).ToString().PadLeft(maxLength), Colors.LineNumber, verbosity);
            }
        }

        public static string FormatDiagnostic(
            Diagnostic diagnostic,
            IFormatProvider? formatProvider = null)
        {
            StringBuilder sb = StringBuilderCache.GetInstance();

            FormatLocation(diagnostic.Location, ref sb);

            sb.Append(GetSeverityText(diagnostic.Severity));
            sb.Append(' ');
            sb.Append(diagnostic.Id);
            sb.Append(": ");

            string message = diagnostic.GetMessage(formatProvider);

            sb.Append(message);

            return StringBuilderCache.GetStringAndFree(sb);
        }

        internal static void FormatLocation(
            Location location,
            ref StringBuilder sb)
        {
            switch (location.Kind)
            {
                case LocationKind.SourceFile:
                case LocationKind.XmlFile:
                case LocationKind.ExternalFile:
                    {
                        FileLinePositionSpan span = location.GetMappedLineSpan();

                        if (span.IsValid)
                        {
                            sb.Append(span.Path);

                            LinePosition linePosition = span.StartLinePosition;

                            sb.Append('(');
                            sb.Append(linePosition.Line + 1);
                            sb.Append(',');
                            sb.Append(linePosition.Character + 1);
                            sb.Append("): ");
                        }

                        break;
                    }
            }
        }

        private static string GetSeverityText(DiagnosticSeverity diagnosticSeverity)
        {
            return diagnosticSeverity switch
            {
                DiagnosticSeverity.Hidden => "hidden",
                DiagnosticSeverity.Info => "info",
                DiagnosticSeverity.Warning => "warning",
                DiagnosticSeverity.Error => "error",
                _ => throw new InvalidOperationException(),
            };
        }

        private static string GetRandomIdentifier(string prefix) => prefix + Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
    }
}
