namespace SRAM.Comparison.Extensions
{
	public static class NumberExtensions
	{
		public static bool IsSingleBit(this byte source) => InternalCountChangedBits(0, source) <= 1;
		public static bool IsSingleBit(this ushort source) => InternalCountChangedBits(0, source) <= 1;
		public static bool IsSingleBit(this uint source) => InternalCountChangedBits(0, source) <= 1;
		public static bool IsSingleBit(this ulong source) => InternalCountChangedBits(0, source) <= 1;

		public static int CountChangedBits(this byte source, ulong compValue = 0) => InternalCountChangedBits(compValue, source);
		public static int CountChangedBits(this ushort source, ulong compValue = 0) => InternalCountChangedBits(compValue, source);
		public static int CountChangedBits(this uint source, ulong compValue = 0) => InternalCountChangedBits(compValue, source);
		public static int CountChangedBits(this ulong source, ulong compValue = 0) => InternalCountChangedBits(compValue, source);

		private static int InternalCountChangedBits(ulong source, ulong compValue)
		{
			var number = source;
			var xor = number ^ (number + compValue);

			return CountBits(xor);
		}

		private static int CountBits(ulong number) 
		{
			var setBits = 0;
			while (number != 0)
			{
				number = number & (number - 1);
				setBits++;
			}

			return setBits;
		}
	}
}
