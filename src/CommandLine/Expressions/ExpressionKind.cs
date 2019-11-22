// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.Expressions
{
    internal enum ExpressionKind
    {
        None = 0,
        IntervalExpression = 1,
        DecrementExpression = 2,
        EqualsExpression = 3,
        GreaterThanExpression = 4,
        LessThanExpression = 5,
        GreaterThanOrEqualExpression = 6,
        LessThanOrEqualExpression = 7,
    }
}
