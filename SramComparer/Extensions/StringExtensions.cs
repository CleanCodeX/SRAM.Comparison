namespace SramComparer.Extensions
{
    public static class StringExtensions
    {
        public static int ParseGameId(this string source)
        {
            int.TryParse(source, out var result);

            return result;
        }
    }
}