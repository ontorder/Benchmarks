using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using BenchmarkDotNet.Attributes;

namespace test;

[MemoryDiagnoser]
public class bench_jsondto
{
    const string json_model = """
        {
        "campo1":1234,
        "campo2":"tre",
        "campo3":111.222
        }
        """;

    const string json_dto = @"[1234,""tre"",111.222]";

    static readonly byte[] json_model_utf8 = Encoding.UTF8.GetBytes(json_model);
    static readonly byte[] json_dto_utf8 = Encoding.UTF8.GetBytes(json_dto);

    [Benchmark]
    public object de_model()
    {
        return JsonSerializer.Deserialize<test_model>(json_model)!;
    }

    [Benchmark]
    public object de_model_typeinfo()
    {
        return JsonSerializer.Deserialize(json_model_utf8, bench_json_context.Default.test_model)!;
    }

    [Benchmark]
    public object de_dto()
    {
        var arr = JsonSerializer.Deserialize<object[]>(json_dto);
        return new test_model
        {
            campo1 = ((JsonElement)arr[0]).GetInt32(),
            campo2 = ((JsonElement)arr[1]).GetString(),
            campo3 = ((JsonElement)arr[2]).GetDouble()
        };
    }

    [Benchmark]
    public object de_dto_reader()
    {
        var reader = new Utf8JsonReader(json_dto_utf8);
        reader.Read(); // StartArray
        reader.Read();
        var c1 = reader.GetInt32();
        reader.Read();
        var c2 = reader.GetString();
        reader.Read();
        var c3 = reader.GetDouble();
        return new test_model { campo1 = c1, campo2 = c2, campo3 = c3 };
    }

    [Benchmark]
    public object de_dto_converter()
    {
        return JsonSerializer.Deserialize(json_dto_utf8, bench_json_context.Default.test_model_dto)!;
    }

    internal sealed class test_model
    {
        public int campo1 { get; set; }
        public string campo2 { get; set; }
        public double campo3 { get; set; }
    }

    [JsonConverter(typeof(test_model_dto_converter))]
    internal sealed class test_model_dto
    {
        public int campo1 { get; set; }
        public string campo2 { get; set; }
        public double campo3 { get; set; }
    }

    internal sealed class test_model_dto_converter : JsonConverter<test_model_dto>
    {
        public override test_model_dto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                return ThrowHelper();
            reader.Read();
            var c1 = reader.GetInt32();
            reader.Read();
            var c2 = reader.GetString();
            reader.Read();
            var c3 = reader.GetDouble();
            reader.Read(); // EndArray
            return new test_model_dto { campo1 = c1, campo2 = c2, campo3 = c3 };
        }

        static test_model_dto ThrowHelper() => throw new JsonException();

        public override void Write(Utf8JsonWriter writer, test_model_dto value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.campo1);
            writer.WriteStringValue(value.campo2);
            writer.WriteNumberValue(value.campo3);
            writer.WriteEndArray();
        }
    }
}

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(bench_jsondto.test_model))]
[JsonSerializable(typeof(bench_jsondto.test_model_dto))]
internal partial class bench_json_context : JsonSerializerContext
{
}

/*

| Method   | Mean     | Error   | StdDev  | Gen0   | Allocated |
|--------- |---------:|--------:|--------:|-------:|----------:|
| de_model | 336.5 ns | 0.99 ns | 0.88 ns | 0.0114 |      72 B |
| de_dto   | 596.5 ns | 2.16 ns | 1.81 ns | 0.1192 |     752 B |

| Method            | Mean     | Error   | StdDev  | Gen0   | Allocated |
|------------------ |---------:|--------:|--------:|-------:|----------:|
| de_dto_reader     | 160.2 ns | 0.86 ns | 0.76 ns | 0.0114 |      72 B |
| de_dto_converter  | 193.0 ns | 0.43 ns | 0.40 ns | 0.0114 |      72 B |
| de_model_typeinfo | 308.6 ns | 0.84 ns | 0.79 ns | 0.0114 |      72 B |
| de_model          | 336.1 ns | 1.11 ns | 1.03 ns | 0.0114 |      72 B |
| de_dto            | 598.0 ns | 1.80 ns | 1.68 ns | 0.1192 |     752 B |

*/
