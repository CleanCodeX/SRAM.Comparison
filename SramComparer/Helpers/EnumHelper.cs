using System;
using App.Commons.Extensions;

namespace SramComparer.Helpers
{
	public static class EnumHelper
	{
		public static Enum InvertUIntFlag(Enum flags, Enum flag)
		{
			var intFlag = flag.ToUInt();
			var intFlags = flags.ToUInt();

			var enumType = flags.GetType();
			var enumFlag = (Enum)Enum.ToObject(enumType, intFlag);

			if (flags.HasFlag(enumFlag))
				intFlags &= ~intFlag;
			else
				intFlags |= intFlag;

			flags = (Enum)Enum.ToObject(enumType, intFlags);

			return flags;
		}

		public static Enum InvertIntFlag(Enum flags, Enum flag)
		{
			var intFlag = flag.ToInt();
			var intFlags = flags.ToInt();

			var enumType = flags.GetType();
			var enumFlag = (Enum)Enum.ToObject(enumType, intFlag);

			if (flags.HasFlag(enumFlag))
				intFlags &= ~intFlag;
			else
				intFlags |= intFlag;

			flags = (Enum)Enum.ToObject(enumType, intFlags);

			return flags;
		}
	}
}
