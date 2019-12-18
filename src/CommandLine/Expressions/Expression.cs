// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang.Expressions
{
    internal abstract class Expression
    {
        protected Expression(string identifier)
        {
            Identifier = identifier;
        }

        public string Identifier { get; }

        public abstract ExpressionKind Kind { get; }

        public static Expression Parse(string value)
        {
            if (!TryParse(value, out Expression expression))
                throw new ArgumentException("", nameof(value));

            return expression;
        }

        public static bool TryParse(string value, out Expression expression)
        {
            expression = null;

            int i = 0;
            int length = value.Length;

            while (i < length)
            {
                switch (value[i])
                {
                    case '=':
                        {
                            switch (Peek())
                            {
                                case '(':
                                case '<':
                                    {
                                        expression = ParseInterval();

                                        return expression != null;
                                    }
                                case '-':
                                    {
                                        expression = new DecrementExpression(value.Substring(0, i), value.Substring(i + 2));
                                        return true;
                                    }
                                default:
                                    {
                                        expression = new BinaryExpression(
                                            value.Substring(0, i),
                                            value.Substring(i + 1), ExpressionKind.EqualsExpression);

                                        return true;
                                    }
                            }
                        }
                    case '>':
                        {
                            if (Peek() == '=')
                            {
                                expression = new BinaryExpression(
                                    value.Substring(0, i),
                                    value.Substring(i + 2), ExpressionKind.GreaterThanOrEqualExpression);
                            }
                            else
                            {
                                expression = new BinaryExpression(
                                    value.Substring(0, i),
                                    value.Substring(i + 1), ExpressionKind.GreaterThanExpression);
                            }

                            return true;
                        }
                    case '<':
                        {
                            if (Peek() == '=')
                            {
                                expression = new BinaryExpression(
                                    value.Substring(0, i),
                                    value.Substring(i + 2), ExpressionKind.LessThanOrEqualExpression);
                            }
                            else
                            {
                                expression = new BinaryExpression(
                                    value.Substring(0, i),
                                    value.Substring(i + 1), ExpressionKind.LessThanExpression);
                            }

                            return true;
                        }
                }

                i++;
            }

            return false;

            char Peek()
            {
                return (i < length - 1) ? value[i + 1] : default;
            }

            bool ScanToChar(char ch)
            {
                while (i < length)
                {
                    if (value[i] == ch)
                        return true;

                    i++;
                }

                return false;
            }

            bool ScanToChar2(char ch1, char ch2)
            {
                while (i < length)
                {
                    if (value[i] == ch1
                        || value[i] == ch2)
                    {
                        return true;
                    }

                    i++;
                }

                return false;
            }

            Expression ParseInterval()
            {
                int equalsIndex = i;

                i++;

                int openTokenIndex = i;

                if (!ScanToChar(';'))
                    return null;

                int semicolonIndex = i;

                i++;

                if (!ScanToChar2(')', '>'))
                    return null;

                string identifier = value.Substring(0, equalsIndex);

                BinaryExpression expression1 = CreateBinaryExpression(value, identifier, openTokenIndex, semicolonIndex);

                BinaryExpression expression2 = CreateBinaryExpression(value, identifier, semicolonIndex, i);

                return new IntervalExpression(identifier, expression1, expression2);
            }
        }

        private static BinaryExpression CreateBinaryExpression(string value, string identifier, int index1, int index2)
        {
            ExpressionKind kind = GetBinaryOperatorKind(value[index1]);

            if (kind == ExpressionKind.None)
                return null;

            return new BinaryExpression(
                identifier,
                value.Substring(index1 + 1, index2 - index1 - 1),
                kind);

            static ExpressionKind GetBinaryOperatorKind(char ch)
            {
                switch (ch)
                {
                    case '<':
                        return ExpressionKind.GreaterThanOrEqualExpression;
                    case '(':
                        return ExpressionKind.GreaterThanExpression;
                    case '>':
                        return ExpressionKind.LessThanOrEqualExpression;
                    case ')':
                        return ExpressionKind.LessThanExpression;
                    default:
                        return ExpressionKind.None;
                }
            }
        }
    }
}
