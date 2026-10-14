using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using BenchmarkDotNet.Attributes;

namespace test;

[MemoryDiagnoser]
public class bench_jsondto
{
    const string json_model_min = """
        {"campo1":1234,"campo2":"tre","campo3":111.222}
        """;

    const string json_model = """
        {
            "campo1": 1234,
            "campo2": "tre",
            "campo3": 111.222
        }
        """;

    const string json_dto = @"[1234,""tre"",111.222]";

    static readonly byte[] json_model_utf8 = Encoding.UTF8.GetBytes(json_model);
    static readonly byte[] json_dto_utf8 = Encoding.UTF8.GetBytes(json_dto);
    static readonly byte[] json_model_min_utf8 = Encoding.UTF8.GetBytes(json_model_min);
    static readonly byte[] json_short_utf8 = """{"a":1234,"b":"tre","c":111.222}"""u8.ToArray();

    [Benchmark]
    public object de_model_min_reader()
    {
        var reader = new Utf8JsonReader((ReadOnlySpan<byte>)json_model_min_utf8);
        var m = new test_model();
        reader.Read(); // StartObject
        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            if (reader.ValueTextEquals("campo1"u8)) { reader.Read(); m.campo1 = reader.GetInt32(); }
            else if (reader.ValueTextEquals("campo2"u8)) { reader.Read(); m.campo2 = reader.GetString(); }
            else if (reader.ValueTextEquals("campo3"u8)) { reader.Read(); m.campo3 = reader.GetDouble(); }
            else { reader.Read(); reader.Skip(); }
        }
        return m;
    }

    [Benchmark]
    public object de_short_typeinfo()
    {
        return JsonSerializer.Deserialize(json_short_utf8, bench_json_context.Default.test_model_short)!;
    }

    [Benchmark]
    public object de_short_reader()
    {
        var reader = new Utf8JsonReader((ReadOnlySpan<byte>)json_short_utf8);
        var m = new test_model();
        reader.Read(); // StartObject
        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            var key = reader.ValueSpan;
            reader.Read();
            if (key.Length == 1)
            {
                switch (key[0])
                {
                    case (byte)'a': m.campo1 = reader.GetInt32(); continue;
                    case (byte)'b': m.campo2 = reader.GetString(); continue;
                    case (byte)'c': m.campo3 = reader.GetDouble(); continue;
                }
            }
            reader.Skip();
        }
        return m;
    }

    [Benchmark]
    public test_model_struct de_short_reader_struct()
    {
        var reader = new Utf8JsonReader((ReadOnlySpan<byte>)json_short_utf8);
        test_model_struct m = default;
        reader.Read(); // StartObject
        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            var key = reader.ValueSpan;
            reader.Read();
            if (key.Length == 1)
            {
                switch (key[0])
                {
                    case (byte)'a': m.campo1 = reader.GetInt32(); continue;
                    case (byte)'b': m.campo2 = reader.GetString(); continue;
                    case (byte)'c': m.campo3 = reader.GetDouble(); continue;
                }
            }
            reader.Skip();
        }
        return m;
    }

    [Benchmark]
    public object de_model_min_utf8() => JsonSerializer.Deserialize<test_model>(json_model_min_utf8)!;

    [Benchmark]
    public object de_model_min_typeinfo() => JsonSerializer.Deserialize(json_model_min_utf8, bench_json_context.Default.test_model)!;

    [Benchmark]
    public object de_model_min() => JsonSerializer.Deserialize<test_model>(json_model_min)!;

    [Benchmark]
    public object de_model() => JsonSerializer.Deserialize<test_model>(json_model)!;

    [Benchmark]
    public object de_model_typeinfo() => JsonSerializer.Deserialize(json_model_utf8, bench_json_context.Default.test_model)!;

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
    public test_model_struct de_dto_reader_struct()
    {
        var reader = new Utf8JsonReader((ReadOnlySpan<byte>)json_dto_utf8);
        reader.Read(); // StartArray
        reader.Read();
        var c1 = reader.GetInt32();
        reader.Read();
        var c2 = reader.GetString();
        reader.Read();
        var c3 = reader.GetDouble();
        return new test_model_struct { campo1 = c1, campo2 = c2, campo3 = c3 };
    }

    [Benchmark]
    public test_model_struct de_dto_reader_trygets()
    {
        var reader = new Utf8JsonReader((ReadOnlySpan<byte>)json_dto_utf8);
        reader.Read(); // StartArray
        reader.Read();
        reader.TryGetInt32(out var c1);
        reader.Read();
        var c2 = reader.GetString();
        reader.Read();
        reader.TryGetDouble(out var c3);
        return new test_model_struct { campo1 = c1, campo2 = c2, campo3 = c3 };
    }

    [Benchmark]
    public test_model_struct de_dto_span()
    {
        ReadOnlySpan<byte> s = json_dto_utf8;
        if (s[0] != (byte)'[') ThrowFormat();
        s = s[1..];
        _ = Utf8Parser.TryParse(s, out int c1, out int n);
        s = s[(n + 2)..]; // ,"
        int q = s.IndexOf((byte)'"');
        var c2 = Encoding.UTF8.GetString(s[..q]);
        s = s[(q + 2)..]; // ",
        _ = Utf8Parser.TryParse(s, out double c3, out _);
        return new test_model_struct { campo1 = c1, campo2 = c2, campo3 = c3 };
    }

    [Benchmark]
    public test_model_struct de_dto_unsafe()
    {
        var arr = json_dto_utf8;
        ref byte p = ref MemoryMarshal.GetArrayDataReference(arr);
        nint i = 1; // skip [

        bool neg = Unsafe.Add(ref p, i) == (byte)'-';
        if (neg) i++;
        int c1 = 0;
        uint d;
        while ((d = (uint)(Unsafe.Add(ref p, i) - (byte)'0')) <= 9)
        {
            c1 = c1 * 10 + (int)d;
            i++;
        }
        if (neg) c1 = -c1;

        i += 2; // ,"
        var rest = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref p, i), arr.Length - (int)i);
        int q = rest.IndexOf((byte)'"');
        var c2 = Encoding.UTF8.GetString(rest[..q]);

        _ = Utf8Parser.TryParse(rest[(q + 2)..], out double c3, out _);
        return new test_model_struct { campo1 = c1, campo2 = c2, campo3 = c3 };
    }

    static void ThrowFormat() => throw new FormatException();

    [Benchmark]
    public test_model_dto_struct de_dto_converter_struct() => JsonSerializer.Deserialize(json_dto_utf8, bench_json_context.Default.test_model_dto_struct);

    [Benchmark]
    public object de_dto_converter() => JsonSerializer.Deserialize(json_dto_utf8, bench_json_context.Default.test_model_dto)!;

    internal sealed class test_model
    {
        public int campo1 { get; set; }
        public string campo2 { get; set; }
        public double campo3 { get; set; }
    }

    internal sealed class test_model_short
    {
        [JsonPropertyName("a")] public int campo1 { get; set; }
        [JsonPropertyName("b")] public string campo2 { get; set; }
        [JsonPropertyName("c")] public double campo3 { get; set; }
    }

    public struct test_model_struct
    {
        public int campo1 { get; set; }
        public string campo2 { get; set; }
        public double campo3 { get; set; }
    }

    [JsonConverter(typeof(test_model_dto_struct_converter))]
    public struct test_model_dto_struct
    {
        public int campo1 { get; set; }
        public string campo2 { get; set; }
        public double campo3 { get; set; }
    }

    public sealed class test_model_dto_struct_converter : JsonConverter<test_model_dto_struct>
    {
        public override test_model_dto_struct Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                ThrowHelper();
            reader.Read();
            var c1 = reader.GetInt32();
            reader.Read();
            var c2 = reader.GetString();
            reader.Read();
            var c3 = reader.GetDouble();
            reader.Read(); // EndArray
            return new test_model_dto_struct { campo1 = c1, campo2 = c2, campo3 = c3 };
        }

        static void ThrowHelper() => throw new JsonException();

        public override void Write(Utf8JsonWriter writer, test_model_dto_struct value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.campo1);
            writer.WriteStringValue(value.campo2);
            writer.WriteNumberValue(value.campo3);
            writer.WriteEndArray();
        }
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
[JsonSerializable(typeof(bench_jsondto.test_model_struct))]
[JsonSerializable(typeof(bench_jsondto.test_model_dto_struct))]
[JsonSerializable(typeof(bench_jsondto.test_model_short))]
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

| Method                  | Mean     | Error   | StdDev  | Gen0   | Allocated |
|------------------------ |---------:|--------:|--------:|-------:|----------:|
| de_dto_reader_trygets   | 152.3 ns | 0.62 ns | 0.58 ns | 0.0050 |      32 B |
| de_dto_reader_struct    | 153.4 ns | 1.76 ns | 1.65 ns | 0.0050 |      32 B |
| de_dto_reader           | 164.5 ns | 0.45 ns | 0.40 ns | 0.0114 |      72 B |
| de_dto_converter_struct | 186.4 ns | 0.68 ns | 0.64 ns | 0.0050 |      32 B |
| de_dto_converter        | 196.0 ns | 2.06 ns | 1.93 ns | 0.0114 |      72 B |
| de_short_reader_struct  | 198.7 ns | 0.78 ns | 0.69 ns | 0.0050 |      32 B |
| de_short_reader         | 203.9 ns | 0.82 ns | 0.77 ns | 0.0114 |      72 B |
| de_model_min_reader     | 207.8 ns | 0.61 ns | 0.57 ns | 0.0114 |      72 B |
| de_short_typeinfo       | 287.8 ns | 1.08 ns | 1.01 ns | 0.0114 |      72 B |
| de_model_min_typeinfo   | 290.9 ns | 1.37 ns | 1.28 ns | 0.0114 |      72 B |
| de_model_min_utf8       | 297.2 ns | 1.73 ns | 1.61 ns | 0.0114 |      72 B |
| de_model_min            | 317.7 ns | 2.51 ns | 2.22 ns | 0.0114 |      72 B |
| de_model_typeinfo       | 335.0 ns | 1.66 ns | 1.39 ns | 0.0114 |      72 B |
| de_model                | 350.1 ns | 1.53 ns | 1.35 ns | 0.0114 |      72 B |
| de_dto                  | 590.6 ns | 3.50 ns | 3.27 ns | 0.1192 |     752 B |

| Method                  | Mean      | Error    | StdDev   | Gen0   | Allocated |
|------------------------ |----------:|---------:|---------:|-------:|----------:|
| de_dto_unsafe           |  60.16 ns | 0.389 ns | 0.345 ns | 0.0050 |      32 B |
| de_dto_span             |  60.64 ns | 0.601 ns | 0.533 ns | 0.0050 |      32 B |
| de_dto_reader_trygets   | 158.03 ns | 0.732 ns | 0.649 ns | 0.0050 |      32 B |
| de_dto_reader_struct    | 160.97 ns | 1.699 ns | 1.589 ns | 0.0050 |      32 B |
| de_dto_reader           | 169.56 ns | 3.153 ns | 2.950 ns | 0.0114 |      72 B |
| de_dto_converter_struct | 192.40 ns | 1.604 ns | 1.501 ns | 0.0050 |      32 B |
| de_dto_converter        | 206.79 ns | 2.542 ns | 2.123 ns | 0.0114 |      72 B |
| de_model_min_reader     | 211.34 ns | 3.428 ns | 4.577 ns | 0.0114 |      72 B |
| de_short_reader         | 212.74 ns | 2.668 ns | 2.228 ns | 0.0114 |      72 B |
| de_short_reader_struct  | 206.97 ns | 3.497 ns | 3.271 ns | 0.0050 |      32 B |
| de_short_typeinfo       | 303.35 ns | 4.754 ns | 6.012 ns | 0.0114 |      72 B |
| de_model_min_typeinfo   | 304.59 ns | 1.935 ns | 1.715 ns | 0.0114 |      72 B |
| de_model_min_utf8       | 305.73 ns | 1.116 ns | 0.989 ns | 0.0114 |      72 B |
| de_model_min            | 326.26 ns | 2.522 ns | 2.106 ns | 0.0114 |      72 B |
| de_model_typeinfo       | 352.94 ns | 1.784 ns | 1.668 ns | 0.0114 |      72 B |
| de_model                | 373.67 ns | 1.653 ns | 1.465 ns | 0.0114 |      72 B |
| de_dto                  | 633.25 ns | 3.283 ns | 2.741 ns | 0.1192 |     752 B |

*/
