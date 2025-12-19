using BenchmarkDotNet.Attributes;
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace test;

[MemoryDiagnoser]
public class bench_deserz
{
    private const string json = @"{""Crap1"":123,""Crap2"":""much data"",""Crap3"":""01:02:03""}";
    private const string json_camel = @"{""crap1"":123,""crap2"":""much data"",""crap3"":""01:02:03""}";

    [Benchmark]
    public Crap withStj() => System.Text.Json.JsonSerializer.Deserialize<Crap>(json)!;

    [Benchmark]
    public Crap withStjJsc() => System.Text.Json.JsonSerializer.Deserialize(json, Crap_jsc.Default.Crap)!;

    public Crap withUtf8Json()
    {
        var b = Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(b, isFinalBlock: true, default);
        return default;
    }

    //[Benchmark]
    public Crap withNewtonsoft() => Newtonsoft.Json.JsonConvert.DeserializeObject<Crap>(json)!;

    //[Benchmark]
    public Crap manually()
    {
        var split = json.Trim('{', '}').Split(',');
        var manually = new Crap();
        foreach (var part in split)
        {
            var tokens = part.Split(':', 2);
            var k = tokens[0].Trim('"');
            switch (k)
            {
                case "Crap1": manually.Crap1 = int.Parse(tokens[1]); break;
                case "Crap2": manually.Crap2 = tokens[1].Trim('"'); break;
                case "Crap3": manually.Crap3 = TimeSpan.Parse(tokens[1].Trim('"')); break;
            }
        }
        return manually;
    }

    //[Benchmark]
    public Crap_fields manually2()
    {
        var split = json.Trim('{', '}').Split(',');
        var manually = new Crap_fields();
        foreach (var part in split)
        {
            var tokens = part.Split(':', 2);
            var k = tokens[0].Trim('"');
            switch (k)
            {
                case "Crap1": manually.Crap1 = int.Parse(tokens[1]); break;
                case "Crap2": manually.Crap2 = tokens[1].Trim('"'); break;
                case "Crap3": manually.Crap3 = TimeSpan.Parse(tokens[1].Trim('"')); break;
            }
        }
        return manually;
    }

    public sealed class Crap_fields
    {
        public int Crap1;
        public string Crap2;
        public TimeSpan Crap3;
    }
}

public sealed partial class Crap
{
    public int Crap1 { get; set; }
    public string Crap2 { get; set; }
    public TimeSpan Crap3 { get; set; }
}

[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified,
    GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(Crap))]
internal sealed partial class Crap_jsc : JsonSerializerContext;
/*

| Method         | Mean     | Error   | StdDev  | Gen0   | Allocated |
|--------------- |---------:|--------:|--------:|-------:|----------:|
| withStjJsc     | 380.7 ns | 2.67 ns | 2.50 ns | 0.0124 |      80 B |
| withStj        | 382.3 ns | 1.10 ns | 0.92 ns | 0.0124 |      80 B |
| manually2      | 417.9 ns | 1.84 ns | 1.72 ns | 0.1540 |     968 B |
| manually       | 433.0 ns | 2.53 ns | 2.25 ns | 0.1540 |     968 B |
| withNewtonsoft | 870.8 ns | 7.91 ns | 7.40 ns | 0.4387 |    2752 B |

*/
