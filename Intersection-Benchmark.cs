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

    const int size = 9;
    const int rndmax = size * 10;

    static bench_intersect()
    {
        seq1 = Enumerable.Repeat(0, size).Select(_ => Random.Shared.Next(rndmax)).ToArray();
        seq2 = Enumerable.Repeat(0, size).Select(_ => Random.Shared.Next(rndmax)).ToArray();
        ord1 = seq1.Order().ToArray();
        ord2 = seq2.Order().ToArray();
        hs1 = seq1.ToHashSet();
        hs2 = seq2.ToHashSet();
    }

    [Benchmark]
    public int[] intersect()
    {
        return seq2.Intersect(seq1).ToArray();
    }

    [Benchmark]
    public int[] sortedintersect()
    {
        return ord1.Intersect(ord2).ToArray();
    }

    [Benchmark]
    public int[] hashintersect()
    {
        return hs1.Intersect(hs2).ToArray();
    }
}
/*

precalc x9
| Method          | Mean     | Error   | StdDev  | Gen0   | Allocated |
|---------------- |---------:|--------:|--------:|-------:|----------:|
| intersect       | 222.1 ns | 0.79 ns | 0.70 ns | 0.0842 |     528 B |
| sortedintersect | 217.6 ns | 1.99 ns | 1.77 ns | 0.0842 |     528 B |
| hashintersect   | 177.3 ns | 0.81 ns | 0.67 ns | 0.0687 |     432 B |

non precalc x9
| Method          | Mean     | Error   | StdDev  | Gen0   | Gen1   | Allocated |
|---------------- |---------:|--------:|--------:|-------:|-------:|----------:|
| intersect       | 186.4 ns | 1.01 ns | 0.79 ns | 0.0725 |      - |     456 B |
| sortedintersect | 373.1 ns | 1.90 ns | 1.69 ns | 0.1144 |      - |     720 B |
| hashintersect   | 391.8 ns | 2.90 ns | 2.71 ns | 0.1731 | 0.0005 |    1088 B |

valori precalcolati x99
| Method          | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|---------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| intersect       | 1,602.1 ns |  6.60 ns |  6.17 ns | 0.3510 | 0.0019 |   2.16 KB |
| sortedintersect | 1,455.3 ns | 16.51 ns | 13.79 ns | 0.3223 | 0.0019 |   1.98 KB |
| hashintersect   |   942.6 ns |  5.37 ns |  5.02 ns | 0.3490 | 0.0019 |   2.14 KB |

valori non precalcolati x99
| Method          | Mean     | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|---------------- |---------:|----------:|----------:|-------:|-------:|----------:|
| intersect       | 1.458 us | 0.0212 us | 0.0188 us | 0.3223 | 0.0019 |   1.98 KB |
| sortedintersect | 2.764 us | 0.0148 us | 0.0116 us | 0.4959 | 0.0038 |   3.05 KB |
| hashintersect   | 2.595 us | 0.0129 us | 0.0121 us | 0.9422 | 0.0191 |   5.78 KB |

precalc x999
| Method          | Mean      | Error     | StdDev    | Median    | Gen0   | Gen1   | Allocated |
|---------------- |----------:|----------:|----------:|----------:|-------:|-------:|----------:|
| intersect       | 13.949 us | 0.2768 us | 0.5464 us | 13.934 us | 3.0212 | 0.1831 |  18.64 KB |
| sortedintersect | 12.792 us | 0.2017 us | 0.2893 us | 12.727 us | 3.0212 | 0.1831 |  18.57 KB |
| hashintersect   |  7.113 us | 0.1417 us | 0.3081 us |  6.973 us | 3.0212 | 0.1831 |  18.58 KB |

non precalc x999
| Method          | Mean     | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|---------------- |---------:|---------:|---------:|-------:|-------:|----------:|
| intersect       | 13.11 us | 0.240 us | 0.212 us | 3.0212 | 0.1831 |  18.59 KB |
| sortedintersect | 74.13 us | 0.563 us | 0.527 us | 4.2725 | 0.1221 |  26.54 KB |
| hashintersect   | 24.25 us | 0.195 us | 0.172 us | 8.6670 | 1.4343 |  53.38 KB |

*/
