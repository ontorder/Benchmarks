using BenchmarkDotNet.Attributes;
using System.Collections.Generic;

namespace test;

[MemoryDiagnoser]
public class bench_sformat_vs_cache
{
    private readonly SortedList<long, string> _cache = new()
    {
        { 1  , "somekey:123" },{ 2  , "somekey:123" },{ 3  , "somekey:123" },{ 4  , "somekey:123" },{ 5  , "somekey:123" },{ 6  , "somekey:123" },{ 7  , "somekey:123" },
        { 8  , "somekey:123" },{ 9  , "somekey:123" },{ 0  , "somekey:123" },{ 10 , "somekey:123" },{ 11 , "somekey:123" },{ 12 , "somekey:123" },{ 13 , "somekey:123" },
        { 23 , "somekey:123" },{ 24 , "somekey:123" },{ 25 , "somekey:123" },{ 26 , "somekey:123" },{ 27 , "somekey:123" },{ 28 , "somekey:123" },{ 29 , "somekey:123" },
        { 33 , "somekey:123" },{ 34 , "somekey:123" },{ 35 , "somekey:123" },{ 36 , "somekey:123" },{ 37 , "somekey:123" },{ 38 , "somekey:123" },{ 39 , "somekey:123" },
        { 43 , "somekey:123" },{ 44 , "somekey:123" },{ 45 , "somekey:123" },{ 46 , "somekey:123" },{ 47 , "somekey:123" },{ 48 , "somekey:123" },{ 49 , "somekey:123" },
        { 53 , "somekey:123" },{ 54 , "somekey:123" },{ 55 , "somekey:123" },{ 56 , "somekey:123" },{ 57 , "somekey:123" },{ 58 , "somekey:123" },{ 59 , "somekey:123" },
        { 123, "somekey:123" },{ 133, "somekey:123" },{ 143, "somekey:123" },{ 153, "somekey:123" },{ 163, "somekey:123" },{ 173, "somekey:123" },{ 183, "somekey:123" },
        { 223, "somekey:123" },{ 233, "somekey:123" },{ 243, "somekey:123" },{ 253, "somekey:123" },{ 263, "somekey:123" },{ 273, "somekey:123" },{ 283, "somekey:123" },
        { 313, "somekey:123" },{ 323, "somekey:123" },{ 333, "somekey:123" },{ 343, "somekey:123" },{ 353, "somekey:123" },{ 363, "somekey:123" },{ 373, "somekey:123" },
        { 424, "somekey:124" },{ 524, "somekey:124" },{ 624, "somekey:124" },{ 724, "somekey:124" },{ 824, "somekey:124" },{ 924, "somekey:124" },{ 994, "somekey:124" },
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
cache x2
| Method      | Mean      | Error     | StdDev    | Gen0   | Allocated |
|------------ |----------:|----------:|----------:|-------:|----------:|
| test_format | 90.432 ns | 0.2825 ns | 0.2359 ns | 0.0191 |     120 B |
| test_cache  |  4.029 ns | 0.0365 ns | 0.0323 ns |      - |         - |

cache x10
| Method      | Mean     | Error    | StdDev   | Gen0   | Allocated |
|------------ |---------:|---------:|---------:|-------:|----------:|
| test_format | 94.83 ns | 0.236 ns | 0.197 ns | 0.0191 |     120 B |
| test_cache  | 10.77 ns | 0.135 ns | 0.127 ns |      - |         - |

cache x70
| Method      | Mean      | Error    | StdDev   | Gen0   | Allocated |
|------------ |----------:|---------:|---------:|-------:|----------:|
| test_format | 106.07 ns | 2.157 ns | 2.398 ns | 0.0191 |     120 B |
| test_cache  |  17.77 ns | 0.195 ns | 0.173 ns |      - |         - |
*/
