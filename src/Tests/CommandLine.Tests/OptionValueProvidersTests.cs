// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Orang.CommandLine.Tests
{
    public static class OptionValueProvidersTests
    {
        [Fact]
        public static void VerifyUniqueName()
        {
            foreach (OptionValueProvider provider in OptionValueProviders.ProvidersByName.Select(f => f.Value))
            {
                var dic = new Dictionary<string, OptionValue>();

                foreach (OptionValue value in provider.Values)
                {
                    Assert.False(dic.ContainsKey(value.Name), value.Name);

                    dic[value.Name] = value;
                }
            }
        }

        [Fact]
        public static void VerifyUniqueValues()
        {
            foreach (OptionValueProvider provider in OptionValueProviders.ProvidersByName.Select(f => f.Value))
            {
                IDictionary<string, string> dic = new Dictionary<string, string>();

                IEnumerable<SimpleOptionValue> optionValues = provider
                    .Values
                    .OfType<SimpleOptionValue>();

                foreach (string value in optionValues
                    .Select(f => f.Value)
                    .Concat(optionValues.Select(f => f.ShortValue).Where(f => !string.IsNullOrEmpty(f))))
                {
                    Assert.DoesNotContain(value, dic);

                    dic[value] = value;
                }
            }
        }

        [Fact]
        public static void VerifyUniqueKeys()
        {
            foreach (OptionValueProvider provider in OptionValueProviders.ProvidersByName.Select(f => f.Value))
            {
                IDictionary<string, string> dic = new Dictionary<string, string>();

                IEnumerable<KeyValuePairOptionValue> optionValues = provider
                    .Values
                    .OfType<KeyValuePairOptionValue>();

                foreach (string value in optionValues
                    .Select(f => f.Key)
                    .Concat(optionValues.Select(f => f.ShortKey).Where(f => !string.IsNullOrEmpty(f))))
                {
                    Assert.DoesNotContain(value, dic);

                    dic[value] = value;
                }
            }
        }
    }
}
