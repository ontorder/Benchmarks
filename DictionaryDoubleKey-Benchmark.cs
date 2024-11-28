using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;

namespace test;

[MemoryDiagnoser]
public class cache_bench
{
    private static readonly Dictionary<long[], SampleData> _bigkey;
    private static readonly Dictionary<Int128, SampleData> _cursed;
    private static readonly Dictionary<SampleKey, SampleData> _recordkey;
    private static readonly Dictionary<string, SampleData> _stringkey;
    private static readonly Dictionary<Vector128<long>, SampleData> _v2key;

    static cache_bench()
    {
        _stringkey = new();
        _bigkey = new(new LongVec2Compare());
        _cursed = new();
        _v2key = new();
        _recordkey = new();

        for (var i = 0; i < 1000; i++)
        {
            var str = $":{i}";
            _stringkey.Add(i.ToString(), new SampleData(str, i));
            _bigkey.Add(new long[] { i, 4637373 }, new SampleData(str, i));
            _cursed.Add(new Int128((ulong)i, 4637373), new SampleData(str, i));
            _v2key.Add(Vector128.Create(i, 4637373), new SampleData(str, i));
            _recordkey.Add(new SampleKey(i, 4637373), new SampleData(str, i));
        }
    }

    [Benchmark]
    public int AccessString()
    {
        var tot = 0;
        for (long i = 0; i < 30; i++) tot += _stringkey[i.ToString()].I;
        return tot;
    }

    [Benchmark]
    public int AccessVec()
    {
        var tot = 0;
        long[] key = new long[2];
        key[1] = 4637373;
        for (long i = 0; i < 30; i++)
        {
            key[0] = i;
            tot += _bigkey[key].I;
        }
        return tot;
    }

    [Benchmark]
    public int CursedDic()
    {
        var tot = 0;
        for (long i = 0; i < 30; i++) tot += _cursed[new Int128((ulong)i, 4637373)].I;
        return tot;
    }

    [Benchmark]
    public int AccessVec2()
    {
        var tot = 0;
        for (long i = 0; i < 30; i++)
        {
            var v = Vector128.Create(i, 4637373);
            tot += _v2key[v].I;
        }
        return tot;
    }

    [Benchmark]
    public int AccessRecord()
    {
        var tot = 0;
        for (long i = 0; i < 30; i++) tot += _recordkey[new(i, 4637373)].I;
        return tot;
    }

    record SampleData(string S, int I);
    record struct SampleKey(long L1, long L2);
}
sealed class LongVec2Compare : IEqualityComparer<long[]>
{
    public bool Equals(long[]? x, long[]? y) => x[0] == y[0] && x[1] == y[1];

    public int GetHashCode([DisallowNull] long[] obj) => obj[0].GetHashCode() ^ obj[1].GetHashCode();
}
/*

| Method       | Mean     | Error   | StdDev  | Gen0   | Allocated |
|------------- |---------:|--------:|--------:|-------:|----------:|
| AccessString | 330.2 ns | 6.56 ns | 6.44 ns |      - |         - |
| AccessVec    | 303.0 ns | 4.68 ns | 7.42 ns | 0.0062 |      40 B |
| AccessVec2   | 277.0 ns | 2.78 ns | 2.46 ns |      - |         - |
| CursedDic    | 194.4 ns | 1.33 ns | 1.18 ns |      - |         - |
| AccessRecord | 107.8 ns | 0.95 ns | 0.84 ns |      - |         - |

*/
