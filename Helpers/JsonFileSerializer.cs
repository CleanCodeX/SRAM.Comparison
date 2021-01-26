using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SRAM.Comparison.Helpers
{
	public static class JsonFileSerializer
	{
		private static readonly JsonSerializerOptions DefaultOptions = new() { Converters = { new JsonStringEnumConverter() }, WriteIndented = true};

		public static TValue Deserialize<TValue>(string filePath) => Deserialize<TValue>(filePath, DefaultOptions);
		public static TValue Deserialize<TValue>(string filePath, JsonSerializerOptions? options)
		{
			var json = File.ReadAllText(filePath);
			return JsonSerializer.Deserialize<TValue>(json, options)!;
		}

		public static void Serialize<TValue>(string filePath, TValue value) => Serialize(filePath, value, DefaultOptions);
		public static void Serialize<TValue>(string filePath, TValue value, bool writeIndented) => Serialize(filePath, value, new JsonSerializerOptions {WriteIndented = writeIndented });
		public static void Serialize<TValue>(string filePath, TValue value, JsonSerializerOptions? options = null)
		{
			var json = JsonSerializer.Serialize(value, options)!;
			File.WriteAllText(filePath, json);
		}
	}
}
