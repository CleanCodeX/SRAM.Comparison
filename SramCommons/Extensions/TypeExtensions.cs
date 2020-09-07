using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SramCommons.Extensions
{
    public static class TypeExtensions
    {
        public static IReadOnlyDictionary<string, T> GetPublicConstants<T>(this Type type) =>
            type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(T))
                .ToDictionary(k => k.Name, v => (T)v.GetRawConstantValue())!;
    }
}
