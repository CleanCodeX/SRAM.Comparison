using System;
using System.Diagnostics.CodeAnalysis;
using Common.Shared.Min.Extensions;

namespace SRAM.Comparison.Extensions
{
	public static class EnumExtensions
	{
		/// <summary>
		/// Returns a byte array of the underlying type data.
		/// </summary>
		public static byte[] ToBytes([NotNull] this Enum source) =>
			BitConverter.GetBytes(Type.GetTypeCode(Enum.GetUnderlyingType(source.GetType())) switch
			{
				TypeCode.SByte => source.ToSbyte(),
				TypeCode.Byte => source.ToByte(),
				TypeCode.Int16 => source.ToShort(),
				TypeCode.UInt16 => source.ToUShort(),
				TypeCode.Int32 => source.ToInt(),
				TypeCode.UInt32 => source.ToUInt(),
				TypeCode.Int64 => source.ToLong(),
				TypeCode.UInt64 => source.ToULong(),
				_ => throw new ArgumentOutOfRangeException()
			});
	}
}
