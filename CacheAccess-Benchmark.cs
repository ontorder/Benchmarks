using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace test;

[MemoryDiagnoser]
public class dic100_bench
{
    private readonly Dictionary<long, SampleData> _dic = [];
    private readonly SortedList<long, SampleData> _sl;
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());

    public dic100_bench()
    {
        for (long i = 0; i < 100; ++i)
        {
            _dic.Add(i, new(i, $"asd {i}", DateTime.MinValue, DateTime.MinValue, true, default, default, 1));
        }
        _sl = new(_dic);
        _cache.Set("dic", _dic);
        _cache.Set("sl", _sl);
    }

    [Benchmark] public SampleData UsingValue() => _dic.Values.Skip(50).First();
    [Benchmark] public SampleData UsingIEnum() => _dic.Skip(50).Select(_ => _.Value).First();
    [Benchmark] public SampleData UsingIQuery() => _dic.AsQueryable().Skip(50).Select(_ => _.Value).First();
    [Benchmark] public SampleData MemCacheDic() => _cache.Get<Dictionary<long, SampleData>>("dic").Skip(50).First().Value;
    [Benchmark] public SampleData MemCacheSorted() => _cache.Get<SortedList<long, SampleData>>("sl").Skip(50).First().Value;

    public record SampleData(long Id, string Description, DateTime CreatedTime, DateTime ModifiedTime, bool Enabled, Guid CreatedUser, Guid ModifiedUser, int AnEnum);
}
/*

| Method         | Mean         | Error       | StdDev      | Gen0   | Gen1   | Allocated |
|--------------- |-------------:|------------:|------------:|-------:|-------:|----------:|
| UsingValue     |     202.2 ns |     0.92 ns |     0.82 ns | 0.0153 |      - |      96 B |
| UsingIEnum     |     228.6 ns |     1.43 ns |     1.34 ns | 0.0279 |      - |     176 B |
| MemCacheDic    |     250.9 ns |     0.82 ns |     0.73 ns | 0.0191 |      - |     120 B |
| MemCacheSorted |     259.9 ns |     2.26 ns |     1.77 ns | 0.0191 |      - |     120 B |
| UsingIQuery    | 319,951.9 ns | 2,091.51 ns | 1,854.07 ns | 1.9531 | 1.4648 |   14867 B |

*/
