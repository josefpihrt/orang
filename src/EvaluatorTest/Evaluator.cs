// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.RegularExpressions;

namespace Orang.Runtime;

public static class Evaluator
{
    public static string Evaluate(Match match)
    {
        string value = match.Value;

        return Helpers.FirstCharToUpper(value);
    }
}
