// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.Runtime
{
    public static class Helpers
    {
        public static string FirstCharToUpper(string value)
        {
            if (value.Length > 0)
            {
                return char.ToUpper(value[0]).ToString() + value.Substring(1);
            }

            return value;
        }
    }
}
