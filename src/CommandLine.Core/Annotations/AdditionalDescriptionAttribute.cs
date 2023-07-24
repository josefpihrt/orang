// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang.CommandLine.Annotations;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
internal sealed class AdditionalDescriptionAttribute : Attribute
{
    public AdditionalDescriptionAttribute(string text)
    {
        Text = text;
    }

    public string Text { get; }
}
