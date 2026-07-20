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

    [Benchmark] public string ser_ctx_meta() => STJ.Serialize(to_serialize, typeof(List<target_type>), context_meta.Default);

    private static readonly STI.JsonTypeInfo type_meta = STI.JsonTypeInfo.CreateJsonTypeInfo<List<target_type>>(new()
    {
        IncludeFields = true,
        TypeInfoResolver = meta_context.Default
    });

    [Benchmark] public string ser_meta() => STJ.Serialize(to_serialize, jsonTypeInfo: type_meta);
}

// can't
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

internal sealed class target_type(string flatFieldName, object? oldValue, object? newValue)
{
    public string FlatFieldName = flatFieldName;
    public object? NewValue = newValue;
    public object? OldValue = oldValue;
}
/*

| net10         | Mean     | Error   | StdDev  | Gen0   | Allocated |
|-------------- |---------:|--------:|--------:|-------:|----------:|
| ser_ctx_def   | 604.5 ns | 2.96 ns | 2.77 ns | 0.0620 |     392 B |
| ser_meta      | 620.6 ns | 7.81 ns | 7.31 ns | 0.1116 |     704 B |
| ser_reference | 639.0 ns | 2.73 ns | 2.56 ns | 0.1116 |     704 B |
| ser_ctx_meta  | 639.0 ns | 6.28 ns | 5.57 ns | 0.1116 |     704 B |

*/
