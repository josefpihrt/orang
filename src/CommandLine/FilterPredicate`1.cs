// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Orang.Expressions;

namespace Orang.CommandLine
{
    internal class FilterPredicate<T>
    {
        public FilterPredicate(Expression expression, Func<T, bool> predicate)
        {
            Expression = expression;
            Predicate = predicate;
        }

        public Expression Expression { get; }

        public Func<T, bool> Predicate { get; }
    }
}
