using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SramComparer.Helpers
{
	public class EnumTextConverter : JsonConverter<Enum>
	{
		public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(Enum);

		public override Enum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (Enum.TryParse(typeToConvert, reader.GetString(), true, out var result))
				return (Enum)result!;

			return null!;
		}

		public override void Write(Utf8JsonWriter writer, Enum value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
	}
}