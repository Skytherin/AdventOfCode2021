using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace AdventOfCode2021.Utils
{
    public static class StructuredRx
    {
        public static T Parse<T>(string input)
        {
            return (T?) ParseOrDefaultInternal(typeof(T), input) ?? throw new ApplicationException();
        }

        public static List<T> ParseLines<T>(string input)
        {
            return input.Lines().Select(StructuredRx.Parse<T>).ToList();
        }

        public static T? ParseOrDefault<T>(string input)
        {
            return (T?) ParseOrDefaultInternal(typeof(T), input);
        }

        private static string GetRegexForType(PropertyInfo property, string groupPrefix, Dictionary<string, Action<string>> actions, object parent)
        {
            var propertyType = property.PropertyType;
            var groupName = $"{groupPrefix}{property.Name}";
            if (propertyType == typeof(int))
            {
                actions[groupName] = g => property.SetValue(parent, Convert.ToInt32(g));
                return $"(?<{groupName}>-?\\d+)";
            }
            if (propertyType == typeof(int?))
            {
                actions[groupName] = g => property.SetValue(parent, string.IsNullOrWhiteSpace(g) ? null : Convert.ToInt32(g));
                return $"(?<{groupName}>-?\\d+)";
            }
            if (propertyType == typeof(string))
            {
                actions[groupName] = g => property.SetValue(parent, string.IsNullOrWhiteSpace(g) ? null : g);
                return $"(?<{groupName}>[a-zA-Z]+)";
            }

            if (propertyType == typeof(Dictionary<string, int>))
            {
                var keyrx = @"([a-zA-Z]+)";
                var valuerx = @"(-?\d+)";
                actions[groupName] = g =>
                {
                    var result = new Dictionary<string, int>();
                    var group = $@"(?<key>{keyrx}):\s*(?<value>{valuerx})\s*";
                    var match = Regex.Match(g, $@"^{group}");
                    while (match.Success)
                    {
                        result.Add(match.Groups["key"].Value, Convert.ToInt32(match.Groups["value"].Value));
                        g = g.Substring(match.Length);
                        match = Regex.Match(g, $@"^,\s*{group}");
                    }
                    property.SetValue(parent, result);
                };
                var group = $@"{keyrx}:\s*{valuerx}\s*";
                return $@"(?<{groupName}>({group})(,\s*{group})*)";
            }

            if (propertyType.IsEnum)
            {
                var mi = typeof(StructuredRx).GetMethod("GetEnumMap", BindingFlags.NonPublic | BindingFlags.Static);
                var fooRef = mi!.MakeGenericMethod(propertyType);
                var map = (Dictionary<string, int>)fooRef.Invoke(null, null)!;

                var alternation = map.Keys.Select(it => $"({it})").Join("|");

                actions[groupName] = (g) => property.SetValue(parent, string.IsNullOrWhiteSpace(g) ? 0 : map[g.ToLower()]);
                return $"(?<{groupName}>{alternation})";
            }
            if (propertyType.IsClass)
            {
                var (pattern, instance) = GetRegexForClass(propertyType, groupName, actions);
                foreach (var key in actions.Keys.Where(it => it.StartsWith(groupName)))
                {
                    var old = actions[key];
                    actions[key] = g =>
                    {
                        // TODO: this fixes my problem right now, but also implies that you
                        // can never instantiate a type with an empty match somewhere.
                        if (!string.IsNullOrWhiteSpace(g))
                        {
                            property.SetValue(parent, instance);
                        }
                        
                        old(g);
                    };
                }

                return pattern;
            }

            throw new ApplicationException();
        }

        private static (string regex, object instance) GetRegexForClass(Type propertyType, string prefix,
            Dictionary<string, Action<string>> actions)
        {
            var properties = propertyType.GetProperties().Where(p => p.SetMethod != null).ToList();
            var pattern = "";
            var ctor = propertyType.GetConstructor(new Type[] { })!;
            var instance = ctor.Invoke(new object?[] { });

            var alternations = new List<string>();

            foreach (var subProperty in properties)
            {
                var rxFormat = subProperty.GetCustomAttribute<RxFormat>() ?? new RxFormat();
                var rxAlternate = subProperty.GetCustomAttribute<RxAlternate>();

                if (rxAlternate == null && alternations.Any())
                {
                    pattern += $"({alternations.Select(a => $"({a})").Join("|")})";
                    alternations.Clear();
                }

                var altern = "";

                if (rxFormat.Before is { } before)
                {
                    altern += before;
                    altern += @"\s*";
                }

                altern += GetRegexForType(subProperty, $"{prefix}__", actions, instance);

                altern += @"\s*";

                if (rxFormat.After is { } after)
                {
                    altern += after;
                    altern += @"\s*";
                }

                if (rxAlternate != null)
                {
                    alternations.Add(altern);
                }
                else
                {
                    pattern += altern;
                }
            }

            if (alternations.Any())
            {
                pattern += $"({alternations.Select(a => $"({a})").Join("|")})";
                alternations.Clear();
            }

            return (pattern, instance);
        }

        private static object? ParseOrDefaultInternal(Type mainType, string input)
        {
            var groupToSetAction = new Dictionary<string, Action<string>>();
            var (pattern, instance) = GetRegexForClass(mainType, mainType.Name, groupToSetAction);

            pattern = $"^({pattern})$";

            var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return null;
            }

            foreach (var property in groupToSetAction.Keys)
            {
                var g = match.Groups[property].Value;
                groupToSetAction[property](g);
            }

            return instance;
        }

        [UsedImplicitly]
        private static Dictionary<string, int> GetEnumMap<T>()
        {
            var enumValues = typeof(T).GetEnumValues();
            var result = new Dictionary<string, int>();

            foreach (T value in enumValues)
            {
                var memberInfo = typeof(T)
                    .GetMember(value.ToString())
                    .First();

                if (memberInfo.GetCustomAttribute<DescriptionAttribute>() is { } description)
                {
                    result[description.Description.ToLower()] = Convert.ToInt32(value);
                }
                else
                {
                    result[memberInfo.Name.ToLower()] = Convert.ToInt32(value);
                }
            }

            return result;
        }
    }
}