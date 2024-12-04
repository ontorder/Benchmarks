using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace test;

[MemoryDiagnoser]
public class ienumcount_bench
{
    private static readonly IEnumerable<int> _items = GetExpensiveItems();
    //private static readonly IEnumerable<int> _items = new int[999];

    [Benchmark]
    public bool ArrayCount() => _items.Count() >= 2;

    [Benchmark]
    public bool ArrayExtension() => _items.CountAtLeast2();

    [Benchmark]
    public bool ArrayForeach() => _items.CountAtLeast2foreach();

    [Benchmark]
    public bool GenCount() => Enumerable.Range(0, 999).Count() >= 2;

    [Benchmark]
    public bool GenExt() => Enumerable.Range(0, 999).CountAtLeast2();

    [Benchmark]
    public bool GenForeach() => Enumerable.Range(0, 999).CountAtLeast2foreach();

    [Benchmark]
    public bool ArraySkipAny() => _items.Skip(1).Any();

    [Benchmark]
    public bool ArrayGeneric() => _items.CountAtLeast(2);

    [Benchmark]
    public bool GenSkipAny() => Enumerable.Range(0, 999).Skip(1).Any();

    [Benchmark]
    public bool GenGeneric() => Enumerable.Range(0, 999).CountAtLeast(2);

    private static IEnumerable<int> GetExpensiveItems()
    {
        for (int i = 0; i < 999; i++)
        {
            var n = 34_234d;
            var y = (long)n;
            while (true)
            {
                if (n % --y == 0)
                {
                    break;
                }
            }
            yield return i;
        }
    }
}
file static class LocalExtensions
{
    public static bool CountAtLeast2(this IEnumerable<int> items)
    {
        var enumerator = items.GetEnumerator();
        enumerator.MoveNext();
        return enumerator.MoveNext();
    }

    public static bool CountAtLeast2foreach(this IEnumerable<int> items)
    {
        int count = 0;
        foreach (var item in items)
        {
            if (++count == 2)
            {
                return true;
            }
        }
        return false;
    }

    public static bool CountAtLeast(this IEnumerable<int> items, int count)
    {
        var enumerator = items.GetEnumerator();
        for (int i = 1; i <= count; i++)
        {
            if (false == enumerator.MoveNext())
            {
                return false;
            }
            if (i == count)
            {
                return true;
            }
        }
        return false;
    }
}
/*

array
| Method         | Mean      | Error     | StdDev    | Median    | Gen0   | Allocated |
|--------------- |----------:|----------:|----------:|----------:|-------:|----------:|
| ArrayExtension |  7.460 ns | 0.0774 ns | 0.0646 ns |  7.462 ns | 0.0051 |      32 B |
| ArrayGeneric   |  8.218 ns | 0.0757 ns | 0.0708 ns |  8.204 ns | 0.0051 |      32 B |
| GenCount       |  8.661 ns | 0.1821 ns | 0.1521 ns |  8.601 ns | 0.0064 |      40 B |
| GenExt         |  8.745 ns | 0.0803 ns | 0.0751 ns |  8.731 ns | 0.0064 |      40 B |
| GenForeach     |  9.323 ns | 0.0994 ns | 0.0776 ns |  9.303 ns | 0.0064 |      40 B |
| ArrayCount     |  9.352 ns | 0.1135 ns | 0.1006 ns |  9.339 ns |      - |         - |
| GenGeneric     |  9.431 ns | 0.1211 ns | 0.1955 ns |  9.375 ns | 0.0064 |      40 B |
| ArrayForeach   | 10.352 ns | 0.3083 ns | 0.9090 ns |  9.903 ns | 0.0051 |      32 B |
| ArraySkipAny   | 13.503 ns | 0.2979 ns | 0.6725 ns | 13.157 ns | 0.0076 |      48 B |
| GenSkipAny     | 20.007 ns | 0.4287 ns | 1.0596 ns | 20.699 ns | 0.0127 |      80 B |

GetExpensiveItems
| Method         | Mean               | Error             | StdDev            | Gen0   | Allocated |
|--------------- |-------------------:|------------------:|------------------:|-------:|----------:|
| GenExt         |           8.178 ns |         0.1086 ns |         0.1016 ns | 0.0064 |      40 B |
| GenCount       |           8.497 ns |         0.0626 ns |         0.0586 ns | 0.0064 |      40 B |
| GenForeach     |           9.309 ns |         0.0346 ns |         0.0289 ns | 0.0064 |      40 B |
| GenGeneric     |           9.441 ns |         0.0280 ns |         0.0249 ns | 0.0064 |      40 B |
| GenSkipAny     |          18.066 ns |         0.1079 ns |         0.0956 ns | 0.0127 |      80 B |
| ArrayGeneric   |     363,329.426 ns |     3,074.5549 ns |     2,725.5129 ns |      - |      32 B |
| ArrayForeach   |     366,072.743 ns |     4,132.0937 ns |     3,662.9934 ns |      - |      32 B |
| ArrayExtension |     367,354.352 ns |     3,525.0604 ns |     3,297.3436 ns |      - |      32 B |
| ArraySkipAny   |     369,743.171 ns |     7,044.4412 ns |     6,589.3747 ns |      - |      88 B |
| ArrayCount     | 198,563,590.909 ns | 3,898,293.2269 ns | 6,183,106.1609 ns |      - |     165 B |

*/
