// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.Expressions;

internal class DecrementExpression : Expression
{
    public DecrementExpression(string text, string identifier, string value) : base(text, identifier, value)
    {
    }

    public override ExpressionKind Kind => ExpressionKind.DecrementExpression;
}
