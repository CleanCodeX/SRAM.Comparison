using System;
using System.Collections.Generic;
using System.Linq;
using Common.Shared.Min.Extensions;
using SramComparer.Properties;

namespace SramComparer.Extensions
{
	public static class EnumExtensions
	{
		public static string ToFlagsString(this Enum source) => source.ToString() == "0" ? Resources.EnumNone : source.ToString();

		public static IDictionary<string, Enum> ToDictionary<TEnum>(this TEnum source) where TEnum: struct, Enum => Enum.GetNames<TEnum>().ToDictionary(k => k, v => (Enum)v.ParseEnum<TEnum>());
	}
}