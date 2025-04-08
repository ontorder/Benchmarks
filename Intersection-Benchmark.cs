using BenchmarkDotNet.Attributes;
using System;
using System.Linq;

namespace test;

[MemoryDiagnoser]
public class bench_intersect
{
    static private int[] seq1;
    static private int[] seq2;
    static int[] ord1;
    static int[] ord2;
    static System.Collections.Generic.HashSet<int> hs1;
    static System.Collections.Generic.HashSet<int> hs2;

    static bench_intersect()
    {
        seq1 = Enumerable.Repeat(0, 99).Select(_ => Random.Shared.Next(999)).ToArray();
        seq2 = Enumerable.Repeat(0, 99).Select(_ => Random.Shared.Next(999)).ToArray();
    }

    [Benchmark]
    public int[] intersect()
    {
        return seq2.Intersect(seq1).ToArray();
    }

    [Benchmark]
    public int[] sortedintersect()
    {
        ord1 = seq1.Order().ToArray();
        ord2 = seq2.Order().ToArray();
        return ord1.Intersect(ord2).ToArray();
    }

    [Benchmark]
    public int[] hashintersect()
    {
        hs1 = seq1.ToHashSet();
        hs2 = seq2.ToHashSet();
        return hs1.Intersect(hs2).ToArray();
    }
}
/*

valori precalcolati
| Method          | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|---------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| intersect       | 1,602.1 ns |  6.60 ns |  6.17 ns | 0.3510 | 0.0019 |   2.16 KB |
| sortedintersect | 1,455.3 ns | 16.51 ns | 13.79 ns | 0.3223 | 0.0019 |   1.98 KB |
| hashintersect   |   942.6 ns |  5.37 ns |  5.02 ns | 0.3490 | 0.0019 |   2.14 KB |

valori non precalcolati
| Method          | Mean     | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|---------------- |---------:|----------:|----------:|-------:|-------:|----------:|
| intersect       | 1.458 us | 0.0212 us | 0.0188 us | 0.3223 | 0.0019 |   1.98 KB |
| sortedintersect | 2.764 us | 0.0148 us | 0.0116 us | 0.4959 | 0.0038 |   3.05 KB |
| hashintersect   | 2.595 us | 0.0129 us | 0.0121 us | 0.9422 | 0.0191 |   5.78 KB |

*/
