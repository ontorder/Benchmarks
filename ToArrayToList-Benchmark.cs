  using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace test;

using ISGD = (int, string, Guid, DateTime);

[MemoryDiagnoser]
public class bench_toarray
{
    //static HashSet<string> data = Enumerable.Range(1, 1000_000).Select(_ => _.ToString()).ToHashSet();
    //static HashSet<ISGD> data = Enumerable.Range(1, 1000_000).Select(_ => (_, "adasdasd", Guid.Empty, DateTime.MaxValue)).ToHashSet();

    //[Benchmark] public int[] toarray_100() => Enumerable.Range(1, 100).ToArray();
    //[Benchmark] public List<int> tolist_100() => Enumerable.Range(1, 100).ToList();
    //[Benchmark] public int[] toarray_1000() => Enumerable.Range(1, 1000).ToArray();
    //[Benchmark] public List<int> tolist_1000() => Enumerable.Range(1, 1000).ToList();
    //[Benchmark] public int[] toarray_10_000() => Enumerable.Range(1, 10_000).ToArray();
    //[Benchmark] public List<int> tolist_10_000() => Enumerable.Range(1, 10_000).ToList();
    //[Benchmark] public int[] toarray_100_000() => Enumerable.Range(1, 100_000).ToArray();
    //[Benchmark] public List<int> tolist_100_000() => Enumerable.Range(1, 100_000).ToList();
    //[Benchmark] public int[] toarray_1000_000() => Enumerable.Range(1, 1000_000).ToArray();
    //[Benchmark] public List<int> tolist_100_0000() => Enumerable.Range(1, 1000_000).ToList();

    //[Benchmark] public ISGD[] toarray_100() => data.Take(100).ToArray();
    //[Benchmark] public List<ISGD> tolist_100() => data.Take(100).ToList();
    //[Benchmark] public ISGD[] toarray_1000() => data.Take(1000).ToArray();
    //[Benchmark] public List<ISGD> tolist_1000() => data.Take(1000).ToList();
    //[Benchmark] public ISGD[] toarray_10_000() => data.Take(10_000).ToArray();
    //[Benchmark] public List<ISGD> tolist_10_000() => data.Take(10_000).ToList();
    //[Benchmark] public ISGD[] toarray_100_000() => data.Take(100_000).ToArray();
    //[Benchmark] public List<ISGD> tolist_100_000() => data.Take(100_000).ToList();
    //[Benchmark] public ISGD[] toarray_1000_000() => data.Take(1000_000).ToArray();
    //[Benchmark] public List<ISGD> tolist_100_0000() => data.Take(1000_000).ToList();
}
/*
int
| Method           | Mean            | Error         | StdDev        | Gen0     | Gen1     | Gen2     | Allocated |
|----------------- |----------------:|--------------:|--------------:|---------:|---------:|---------:|----------:|
| toarray_100      |        38.51 ns |      0.278 ns |      0.260 ns |   0.0739 |        - |        - |     464 B |
| tolist_100       |        44.02 ns |      0.286 ns |      0.253 ns |   0.0790 |   0.0001 |        - |     496 B |
| toarray_1000     |       230.60 ns |      1.128 ns |      1.055 ns |   0.6475 |        - |        - |    4064 B |
| tolist_1000      |       232.71 ns |      1.761 ns |      1.647 ns |   0.6526 |   0.0098 |        - |    4096 B |
| toarray_10_000   |     2,336.00 ns |     27.988 ns |     26.180 ns |   6.3667 |        - |        - |   40064 B |
| tolist_10_000    |     2,378.10 ns |     18.717 ns |     17.508 ns |   6.3667 |   0.7935 |        - |   40096 B |
| toarray_100_000  |   129,519.23 ns |    678.279 ns |    601.276 ns | 124.7559 | 124.7559 | 124.7559 |  400106 B |
| tolist_100_000   |   129,388.89 ns |  1,417.252 ns |  1,256.357 ns | 124.7559 | 124.7559 | 124.7559 |  400138 B |
| toarray_1000_000 | 1,154,179.79 ns |  6,827.507 ns |  6,052.407 ns | 998.0469 | 998.0469 | 998.0469 | 4000400 B |
| tolist_100_0000  | 1,153,559.91 ns | 12,242.105 ns | 11,451.272 ns | 998.0469 | 998.0469 | 998.0469 | 4000432 B |

string
| Method           | Mean          | Error       | StdDev      | Gen0     | Gen1     | Gen2     | Allocated   |
|----------------- |--------------:|------------:|------------:|---------:|---------:|---------:|------------:|
| toarray_100      |      1.422 us |   0.0088 us |   0.0078 us |   0.3109 |        - |        - |     1.91 KB |
| tolist_100       |      1.277 us |   0.0038 us |   0.0034 us |   0.3643 |   0.0019 |        - |     2.23 KB |
| toarray_1000     |     11.279 us |   0.0280 us |   0.0249 us |   2.6245 |   0.0305 |        - |    16.13 KB |
| tolist_1000      |      9.949 us |   0.0456 us |   0.0426 us |   2.6550 |   0.0763 |        - |     16.3 KB |
| toarray_10_000   |    113.443 us |   0.4615 us |   0.4317 us |  25.5127 |   2.6855 |        - |   156.99 KB |
| tolist_10_000    |    206.142 us |   0.6665 us |   0.6234 us |  24.1699 |   3.4180 |   3.4180 |    256.4 KB |
| toarray_100_000  |  1,913.336 us |  37.5200 us |  38.5303 us |  48.8281 |  37.1094 |  29.2969 |  1563.42 KB |
| tolist_100_000   |  1,940.319 us |  38.3207 us |  75.6414 us |  54.6875 |  39.0625 |  35.1563 |  2048.65 KB |
| toarray_1000_000 | 16,612.464 us | 304.0848 us | 298.6519 us | 250.0000 | 250.0000 | 250.0000 | 15626.41 KB |
| tolist_100_0000  | 21,748.392 us |  59.8283 us |  55.9634 us | 406.2500 | 406.2500 | 406.2500 | 16384.68 KB |

ISGD
| Method           | Mean          | Error       | StdDev        | Gen0     | Gen1     | Gen2     | Allocated   |
|----------------- |--------------:|------------:|--------------:|---------:|---------:|---------:|------------:|
| toarray_100      |      4.333 us |   0.0122 us |     0.0114 us |   1.3580 |   0.0076 |        - |     8.34 KB |
| tolist_100       |      4.018 us |   0.0104 us |     0.0092 us |   1.6556 |   0.0381 |        - |    10.17 KB |
| toarray_1000     |     39.688 us |   0.0752 us |     0.0667 us |  12.8174 |   0.2441 |        - |    78.81 KB |
| tolist_1000      |     38.343 us |   0.1187 us |     0.0991 us |  13.0615 |   2.1362 |        - |    80.24 KB |
| toarray_10_000   |    542.914 us |   7.5092 us |     7.0242 us |  41.9922 |  17.5781 |   4.8828 |   782.19 KB |
| tolist_10_000    |    688.622 us |  10.5717 us |     9.8888 us |  39.0625 |  15.6250 |  13.6719 |  1280.39 KB |
| toarray_100_000  |  5,332.788 us |  71.6108 us |    66.9848 us | 109.3750 |  93.7500 |  85.9375 |  7813.95 KB |
| tolist_100_000   |  5,912.886 us | 116.2582 us |   242.6742 us | 132.8125 | 109.3750 | 109.3750 | 10240.84 KB |
| toarray_1000_000 | 48,503.195 us | 955.7720 us | 1,242.7737 us | 700.0000 | 700.0000 | 700.0000 | 78126.25 KB |
| tolist_100_0000  | 47,113.904 us | 904.1876 us |   801.5388 us | 400.0000 | 400.0000 | 400.0000 | 81920.64 KB |
// * Warnings *
*/
