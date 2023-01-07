// Copyright 2023 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VDrumExplorer.Utility;

namespace VDrumExplorer.Model.Schema.Json;

/// <summary>
/// Revision filters are used for conditionally-present properties and values
/// in JSON schema files, based on the software revision of the module identifier.
/// 
/// A JSON property with a name of "(revision:[expression])xyz" is either removed
/// if the expression is not satisfied, or replaced with a property with the name "xyz"
/// if the expression is satisfied.
/// 
/// An array element which is a string value of the form "(revision:[expression])xyz"
/// is either removed if the expression is not satisfied, or replaced with a string value "xyz"
/// if the expression is satisfied.
/// 
/// Within a JSON object which is in an array, a JSON property with a name of
/// "revision" is never retained: it must have a value which is a revision expression,
/// and if the expression is satisfied the property is removed; otherwise,
/// the whole containing object is removed.
/// 
/// The expression itself is a comma-separated list of values, optionally preceded by
/// an exclamation mark. Without an exclamation mark, the expression is satisfied if
/// the actual software revision is in the list. With an exclamation mark, the expression
/// is satisfied if the actual software revision is not in the list.
/// </summary>
internal sealed class RevisionFilter
{
    private const string ContainerRevisionPropertyName = "revision";
    private static readonly Regex StringPattern = new Regex(@"^\(revision:(?<expression>!?[0-9a-fA-Fx, _]+)\)(?<remainder>.*)$");

    private readonly bool negated;
    private readonly IReadOnlyList<int> values;

    private RevisionFilter(bool negated, IReadOnlyList<int> values)
    {
        this.negated = negated;
        this.values = values;
    }

    private static RevisionFilter Parse(string expression)
    {
        if (expression == "")
        {
            throw new ArgumentException("Revision expressions cannot be empty");
        }
        bool negated = expression[0] == '!';
        if (negated)
        {
            expression = expression.Substring(1);
        }
        var values = expression.Split(',').ToReadOnlyList(x => HexInt32.Parse(x.Trim()).Value);
        return new RevisionFilter(negated, values);
    }

    private static bool TryParsePrefix(string text, [NotNullWhen(true)] out RevisionFilter? expression, [NotNullWhen(true)] out string? suffix)
    {
        var match = StringPattern.Match(text);
        if (!match.Success)
        {
            expression = null;
            suffix = null;
            return false;
        }
        expression = Parse(match.Groups["expression"].Value);
        suffix = match.Groups["remainder"].Value;
        return true;
    }

    private bool Matches(int revision) => values.Contains(revision) ^ negated;

    public static void VisitObject(JObject obj, int revision)
    {
        Visit(obj);

        void Visit(JToken token)
        {
            switch (token)
            {
                case JProperty property:
                    VisitProperty(property);
                    break;
                case JArray array:
                    VisitArray(array);
                    break;
                case JObject obj:
                    foreach (var property in obj.Properties().ToList())
                    {
                        Visit(property);
                    }
                    break;
            }
        }

        void VisitProperty(JProperty property)
        {
            if (TryParsePrefix(property.Name, out var revisionExpression, out var suffix))
            {
                if (revisionExpression.Matches(revision))
                {
                    property.Replace(new JProperty(suffix, property.Value));
                }
                else
                {
                    property.Remove();
                }
            }
            Visit(property.Value);
        }

        void VisitArray(JArray array)
        {
            for (int i = array.Count - 1; i >= 0; i--)
            {
                if (array[i] is JObject obj && obj.Property(ContainerRevisionPropertyName) is JProperty prop)
                {
                    var expression = Parse((string) prop.Value!);
                    if (expression.Matches(revision))
                    {
                        prop.Remove();
                    }
                    else
                    {
                        array.RemoveAt(i);
                        continue;
                    }
                }
                else if (array[i] is JValue { Type: JTokenType.String, Value: string text })
                {
                    if (TryParsePrefix(text, out var expression, out var suffix))
                    {
                        if (expression.Matches(revision))
                        {
                            array[i] = new JValue(suffix);
                        }
                        else
                        {
                            array.RemoveAt(i);
                            continue;
                        }
                    }
                }
                Visit(array[i]);
            }
        }
    }
}
