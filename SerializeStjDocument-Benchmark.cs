using BenchmarkDotNet.Attributes;
using System;
using System.Text.Json;

namespace test;

[MemoryDiagnoser]
public class bench_seraliz
{
    private readonly static contained d = new() { campo1 = "abc", campo2 = 123, campo3 = DateTime.Now };
    private readonly static container_obj c_obj = new() { d = "tipo", c = d };
    private readonly static container_stj c_stj = new() { d = "tipo", c = JsonSerializer.SerializeToDocument(d) };
    private readonly static container_stj c_stj2 = new() { d = "tipo", c = null };

    [Benchmark]
    public string serialize_regular() => JsonSerializer.Serialize(c_obj);

    [Benchmark]
    public string serialize_stjdoc() => JsonSerializer.Serialize(c_stj);

    [Benchmark]
    public string serialize_2step()
    {
        var step1 = JsonSerializer.SerializeToDocument(d);
        c_stj2.c = step1;
        return JsonSerializer.Serialize(c_stj2);
    }

    public sealed class container_obj { public string d { get; set; } public object c { get; set; } }
    public sealed class container_stj { public string d { get; set; } public JsonDocument c { get; set; } }
    public sealed class contained { public string campo1 { get; set; } public int campo2 { get; set; } public DateTime campo3 { get; set; } }
}
/*
| Method            | Mean       | Error   | StdDev  | Gen0   | Gen1   | Allocated |
|------------------ |-----------:|--------:|--------:|-------:|-------:|----------:|
| serialize_stjdoc  |   411.7 ns | 4.00 ns | 3.74 ns | 0.0343 |      - |     216 B |
| serialize_regular |   532.5 ns | 7.18 ns | 6.36 ns | 0.0820 |      - |     520 B |
| serialize_2step   | 1,361.3 ns | 9.02 ns | 7.53 ns | 2.6875 | 0.1678 |   16880 B |
*/
