using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace test;

[MemoryDiagnoser]
public class ienumcount_bench
{
    private static readonly IEnumerable<int> _items = new int[999];

    [Benchmark]
    public bool CallCount() => _items.Count() >= 2;

    [Benchmark]
    public bool CallExtension() => _items.CountAtLeast2();

    [Benchmark]
    public bool CallForeach() => _items.CountAtLeast2foreach();

    [Benchmark]
    public bool GenCount() => Enumerable.Range(0, 999).Count() >= 2;

    [Benchmark]
    public bool GenExt() => Enumerable.Range(0, 999).CountAtLeast2();

    [Benchmark]
    public bool GenForeach() => Enumerable.Range(0, 999).CountAtLeast2foreach();
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
}
/*

| Method        | Mean     | Error     | StdDev    | Gen0   | Allocated |
|-------------- |---------:|----------:|----------:|-------:|----------:|
| CallExtension | 7.446 ns | 0.0884 ns | 0.0783 ns | 0.0051 |      32 B |
| GenExt        | 7.901 ns | 0.0671 ns | 0.0628 ns | 0.0064 |      40 B |
| GenCount      | 8.568 ns | 0.1154 ns | 0.1079 ns | 0.0064 |      40 B |
| CallCount     | 8.791 ns | 0.0837 ns | 0.0783 ns |      - |         - |
| GenForeach    | 9.130 ns | 0.0789 ns | 0.0738 ns | 0.0064 |      40 B |
| CallForeach   | 9.766 ns | 0.1383 ns | 0.1293 ns | 0.0051 |      32 B |

*/
