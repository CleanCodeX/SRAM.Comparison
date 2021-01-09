using System.IO;
using System.Text.Json;

namespace SramComparer.Helpers
{
	public static class JsonFileSerializer
	{
		public static TValue Deserialize<TValue>(string filePath, JsonSerializerOptions? options = null)
		{
			var json = File.ReadAllText(filePath);
			return JsonSerializer.Deserialize<TValue>(json, options)!;
		}

		public static void Serialize<TValue>(string filePath, TValue value, bool writeIndented) => Serialize(filePath, value, new JsonSerializerOptions {WriteIndented = writeIndented });
		public static void Serialize<TValue>(string filePath, TValue value, JsonSerializerOptions? options = null)
		{
			var json = JsonSerializer.Serialize(value, options)!;
			File.WriteAllText(filePath, json);
		}
	}
}
