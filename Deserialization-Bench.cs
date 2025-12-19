using BenchmarkDotNet.Attributes;
using System;
using System.Text;
using System.Text.Json;

namespace test;

[MemoryDiagnoser]
public class bench_deserz
{
    private const string json = @"{""Crap1"":123,""Crap2"":""much data"",""Crap3"":""01:02:03""}";

    [Benchmark]
    public Crap withStj()
    {
        return System.Text.Json.JsonSerializer.Deserialize<Crap>(json)!;
    }

    public Crap withUtf8Json()
    {
        var b = Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(b, isFinalBlock: true, default);
        return default;
    }

    [Benchmark]
    public Crap withNewtonsoft()
    {
        return Newtonsoft.Json.JsonConvert.DeserializeObject<Crap>(json)!;
    }

    [Benchmark]
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

    public sealed class Crap
    {
        public int Crap1 { get; set; }
        public string Crap2 { get; set; }
        public TimeSpan Crap3 { get; set; }
    }
}
/*

| Method         | Mean     | Error   | StdDev  | Gen0   | Allocated |
|--------------- |---------:|--------:|--------:|-------:|----------:|
| withStj        | 382.3 ns | 1.10 ns | 0.92 ns | 0.0124 |      80 B |
| manually       | 433.0 ns | 2.53 ns | 2.25 ns | 0.1540 |     968 B |
| withNewtonsoft | 870.8 ns | 7.91 ns | 7.40 ns | 0.4387 |    2752 B |

*/
