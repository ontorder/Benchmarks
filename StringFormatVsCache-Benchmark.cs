using BenchmarkDotNet.Attributes;
using System.Collections.Generic;

namespace test;

[MemoryDiagnoser]
public class bench_sformat_vs_cache
{
    private readonly SortedList<long, string> _cache = new()
    {
        { 123, "somekey:123" },
        { 124, "somekey:124" },
    };
    private static long id1 = 123;
    private static long id2 = 124;

    [Benchmark]
    public string test_format()
    {
        var distr = $"distrkey:{id1}";
        return $"{distr}:somekey:{id2}";
    }

    [Benchmark]
    public string test_cache()
        => _cache[id1];
}

/*
| Method      | Mean      | Error     | StdDev    | Gen0   | Allocated |
|------------ |----------:|----------:|----------:|-------:|----------:|
| test_format | 90.432 ns | 0.2825 ns | 0.2359 ns | 0.0191 |     120 B |
| test_cache  |  4.029 ns | 0.0365 ns | 0.0323 ns |      - |         - |
*/
