using System.Text.Json;
using System.Text.Json.Serialization;

using BenchmarkDotNet.Attributes;

using STJ = System.Text.Json.JsonSerializer;
using STI = System.Text.Json.Serialization.Metadata;

namespace test;

[MemoryDiagnoser]
public class bench_stj_sg
{
    private static readonly DateTime dt = DateTime.Now;
    private static readonly List<target_type> to_serialize = [new("AAA:BBB:C", 123, 124), new("AAA:BBB:B", dt, dt.AddSeconds(1))];
    private static readonly JsonSerializerOptions ref_options = new() { IncludeFields = true };

    [Benchmark] public string ser_reference() => STJ.Serialize(to_serialize, options: ref_options);

    [Benchmark] public string ser_ctx_def() => STJ.Serialize(to_serialize, typeof(List<target_type>), context_def.Default);

    [Benchmark] public string ser_ctx_ful() => STJ.Serialize(to_serialize, typeof(List<target_type>), context_full.Default);

    [Benchmark] public string ser_ctx_meta() => STJ.Serialize(to_serialize, typeof(List<target_type>), context_meta.Default);

    private static readonly STI.JsonTypeInfo type_meta = STI.JsonTypeInfo.CreateJsonTypeInfo<List<target_type>>(new()
    {
        IncludeFields = true,
        TypeInfoResolver = meta_context.Default
    });

    [Benchmark] public string ser_meta() => STJ.Serialize(to_serialize, jsonTypeInfo: type_meta);
}

// con assistenza di TargetTypeConverter
[JsonSerializable(typeof(List<target_type>))]
[JsonSerializable(typeof(target_type))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(DateTimeOffset))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(string))]
[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Serialization,
    IncludeFields = true,
    UseStringEnumConverter = true,
    WriteIndented = false)]
internal partial class context_full : JsonSerializerContext { }

[JsonSerializable(typeof(List<target_type>))]
[JsonSerializable(typeof(target_type))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(DateTimeOffset))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(string))]
[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    UseStringEnumConverter = true,
    WriteIndented = false)]
internal partial class context_meta : JsonSerializerContext { }

[JsonSerializable(typeof(List<target_type>))]
[JsonSerializable(typeof(target_type))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(int))]
[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    IncludeFields = true,
    UseStringEnumConverter = true,
    WriteIndented = false)]
internal partial class context_def : JsonSerializerContext { }

[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(List<target_type>))]
internal partial class meta_context : JsonSerializerContext { }

[JsonConverter(typeof(TargetTypeConverter))]
internal sealed class target_type(string flatFieldName, object? oldValue, object? newValue)
{
    public string FlatFieldName = flatFieldName;
    public object? NewValue = newValue;
    public object? OldValue = oldValue;
}

internal sealed class TargetTypeConverter : JsonConverter<target_type>
{
    public override target_type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotSupportedException();

    public override void Write(Utf8JsonWriter writer, target_type value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString(nameof(target_type.FlatFieldName), value.FlatFieldName);

        writer.WritePropertyName(nameof(target_type.NewValue));
        WriteValue(writer, value.NewValue);

        writer.WritePropertyName(nameof(target_type.OldValue));
        WriteValue(writer, value.OldValue);

        writer.WriteEndObject();
    }

    private static void WriteValue(Utf8JsonWriter writer, object? value)
    {
        switch (value)
        {
            case null:
                writer.WriteNullValue();
                break;
            case int number:
                writer.WriteNumberValue(number);
                break;
            case long number:
                writer.WriteNumberValue(number);
                break;
            case bool boolean:
                writer.WriteBooleanValue(boolean);
                break;
            case string text:
                writer.WriteStringValue(text);
                break;
            case DateTime dateTime:
                writer.WriteStringValue(dateTime);
                break;
            case DateTimeOffset dateTimeOffset:
                writer.WriteStringValue(dateTimeOffset);
                break;
            default:
                throw new JsonException($"Unsupported value type '{value.GetType()}'.");
        }
    }
}
/*

| net10         | Mean     | Error   | StdDev  | Gen0   | Allocated |
|-------------- |---------:|--------:|--------:|-------:|----------:|
| ser_ctx_def   | 604.5 ns | 2.96 ns | 2.77 ns | 0.0620 |     392 B |
| ser_meta      | 620.6 ns | 7.81 ns | 7.31 ns | 0.1116 |     704 B |
| ser_reference | 639.0 ns | 2.73 ns | 2.56 ns | 0.1116 |     704 B |
| ser_ctx_meta  | 639.0 ns | 6.28 ns | 5.57 ns | 0.1116 |     704 B |

added TargetTypeConverter
| Method        | Mean     | Error   | StdDev  | Gen0   | Allocated |
|-------------- |---------:|--------:|--------:|-------:|----------:|
| ser_meta      | 493.0 ns | 5.33 ns | 4.98 ns | 0.0620 |     392 B |
| ser_reference | 511.8 ns | 4.68 ns | 4.15 ns | 0.0620 |     392 B |
| ser_ctx_meta  | 515.0 ns | 5.31 ns | 4.71 ns | 0.0620 |     392 B |
| ser_ctx_def   | 527.9 ns | 5.11 ns | 4.53 ns | 0.0620 |     392 B |
| ser_ctx_ful   | 528.7 ns | 5.60 ns | 5.24 ns | 0.0620 |     392 B |

*/
