using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace test;

[MemoryDiagnoser]
//[SimpleJob(BenchmarkDotNet.Engines.RunStrategy.ColdStart)]
public class bench_enumcollections
{
    private static readonly string[] dataArray = [
        "apple", "banana", "cherry", "date", "elderberry",
        "fig", "grape", "honeydew", "kiwi", "lemon",
        "gnagna", "fafaggrr" ,"f4f34","q4tb3","q4hbq54"
    ];
    private static readonly HashSet<string> dataHashset = [.. dataArray];
    private static readonly SortedSet<string> dataSortedset = [.. dataArray];
    private static readonly FrozenSet<string> dataFrozenSet = dataArray.ToFrozenSet();
    private static readonly ImmutableArray<string> dataImmutable = dataArray.ToImmutableArray();
    private static readonly ImmutableArray<string>.Builder dataBuilder = dataArray.ToImmutableArray().ToBuilder();

    [Benchmark] public string enumArray() => dataArray.TestEnum();
    [Benchmark] public string enumHashset() => dataHashset.TestEnum();
    [Benchmark] public string enumSortedset() => dataSortedset.TestEnum();
    [Benchmark] public string enumFrozenset() => dataFrozenSet.TestEnum();
    [Benchmark] public string enumImmutable() => dataImmutable.TestEnum();
    [Benchmark] public string enumBuilder() => dataBuilder.TestEnum();
    [Benchmark] public string enumSpan() => dataArray.AsSpan().TestEnumSpan();
}
file static class E
{
    public static string TestEnum(this ICollection<string> source)
    {
        var temp = string.Empty;
        foreach (var item in source) temp = item;
        return temp;
    }

    public static string TestEnumSpan(this Span<string> source)
    {
        var temp = string.Empty;
        foreach (var item in source) temp = item;
        return temp;
    }
}
/*
| Method        | Mean       | Error     | StdDev    | Gen0   | Allocated |
|-------------- |-----------:|----------:|----------:|-------:|----------:|
| enumSpan      |   5.515 ns | 0.1552 ns | 0.1451 ns |      - |         - |
| enumArray     |  32.194 ns | 0.1060 ns | 0.0828 ns | 0.0051 |      32 B |
| enumImmutable |  39.245 ns | 0.4186 ns | 0.3496 ns | 0.0089 |      56 B |
| enumFrozenset |  40.100 ns | 0.2441 ns | 0.2039 ns | 0.0051 |      32 B |
| enumHashset   |  45.400 ns | 0.2104 ns | 0.1865 ns | 0.0063 |      40 B |
| enumBuilder   |  56.091 ns | 0.4031 ns | 0.3366 ns | 0.0063 |      40 B |
| enumSortedset | 184.835 ns | 1.2113 ns | 1.0115 ns | 0.0267 |     168 B |

| cold start 1  | Mean     | Error     | StdDev    | Median   | Allocated |
|-------------- |---------:|----------:|----------:|---------:|----------:|
| enumBuilder   | 30.95 us |  97.09 us | 286.28 us | 1.900 us |     776 B |
| enumImmutable | 32.25 us |  96.73 us | 285.20 us | 2.400 us |     792 B |
| enumFrozenset | 32.35 us |  95.83 us | 282.55 us | 2.200 us |     768 B |
| enumHashset   | 32.61 us |  98.36 us | 290.03 us | 2.150 us |     776 B |
| enumArray     | 42.55 us | 133.79 us | 394.48 us | 2.200 us |     768 B |
| enumSortedset | 46.01 us | 138.37 us | 408.00 us | 3.500 us |     904 B |

| cold start 2  | Mean     | Error     | StdDev    | Median    | Allocated |
|-------------- |---------:|----------:|----------:|----------:|----------:|
| enumSpan      | 28.89 us |  94.65 us | 279.07 us | 0.8000 us |     736 B |
| enumHashset   | 30.58 us |  95.42 us | 281.34 us | 1.9500 us |     776 B |
| enumImmutable | 31.12 us |  95.13 us | 280.49 us | 2.5000 us |     792 B |
| enumBuilder   | 31.74 us |  96.41 us | 284.27 us | 1.9000 us |     776 B |
| enumArray     | 34.81 us | 108.53 us | 320.02 us | 2.0500 us |     768 B |
| enumSortedset | 35.23 us | 101.46 us | 299.15 us | 3.4000 us |     904 B |
| enumFrozenset | 35.77 us |  98.21 us | 289.59 us | 2.1000 us |     768 B |
*/
