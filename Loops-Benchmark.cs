using System.Collections.Generic;
using System.Linq;

namespace test;

[BenchmarkDotNet.Attributes.MemoryDiagnoser]
public class bench_loop
{
    //[BenchmarkDotNet.Attributes.Benchmark]
    public void viaAggregate()
    {
        _ = Enumerable.Range(0, 99).Aggregate(0, (_, _) => default);
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public void viaEnumerator()
    {
        var t = Enumerable.Range(0, 99).GetEnumerator();
        while (t.MoveNext()) ;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public void viaFor()
    {
        for (int i = 0; i < 100; ++i) ;
    }

    private readonly int[] _testArr = [1, 2, 3, 4, 5, 6, 7, 8, 9, 0];
    private readonly List<int> _testList = new([1, 2, 3, 4, 5, 6, 7, 8, 9, 0]);
    private readonly int[,] _testMatrix = new int[9,9];

    //[BenchmarkDotNet.Attributes.Benchmark]
    public int forArrayLength()
    {
        int i = 0;
        for (; i < _testArr.Length; ++i) ;
        return i;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public int forArrayCountFn()
    {
        int i = 0;
        for (; i < _testArr.Count(); ++i) ;
        return i;
    }

    [BenchmarkDotNet.Attributes.Benchmark]
    public int forArrayLongCount()
    {
        int i = 0;
        for (; i < _testArr.LongCount(); ++i) ;
        return i;
    }

    [BenchmarkDotNet.Attributes.Benchmark]
    public int forArrayLongLength()
    {
        int i = 0;
        for (; i < _testArr.LongLength; ++i) ;
        return i;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public int forListCountFn()
    {
        int i = 0;
        for (; i < _testList.Count(); ++i) ;
        return i;
    }

    [BenchmarkDotNet.Attributes.Benchmark]
    public int forListLongCount()
    {
        int i = 0;
        for (; i < _testList.LongCount(); ++i) ;
        return i;
    }

    [BenchmarkDotNet.Attributes.Benchmark]
    public int forListCount()
    {
        int i = 0;
        for (; i < _testList.Count; ++i) ;
        return i;
    }

    [BenchmarkDotNet.Attributes.Benchmark]
    public int forArrayGetDimension()
    {
        int i = 0;
        for (; i < _testArr.GetLength(0); ++i) ;
        return i;
    }

    [BenchmarkDotNet.Attributes.Benchmark]
    public int forArrayGetUpper()
    {
        int i = 0;
        for (; i < _testArr.GetUpperBound(0); ++i) ;
        return i;
    }

    [BenchmarkDotNet.Attributes.Benchmark]
    public int forArrayGetLongLength()
    {
        int i = 0;
        for (; i < _testArr.GetLongLength(0); ++i) ;
        return i;
    }
}
/*
| Method        | Mean      | Error    | StdDev   | Gen0   | Allocated |
|-------------- |----------:|---------:|---------:|-------:|----------:|
| viaAggregate  | 127.98 ns | 0.871 ns | 0.815 ns | 0.0062 |      40 B |
| viaEnumerator | 131.61 ns | 1.775 ns | 1.823 ns | 0.0062 |      40 B |
| viaFor        |  28.22 ns | 0.286 ns | 0.254 ns |      - |         - |

| Method                | Mean       | Error     | StdDev    | Allocated |
|---------------------- |-----------:|----------:|----------:|----------:|
| forListLongCount      | 273.482 ns | 3.2011 ns | 2.9943 ns |     440 B |
| forArrayLongCount     | 229.158 ns | 2.3605 ns | 1.9711 ns |     352 B |
| forArrayCountFn       | 111.334 ns | 0.9932 ns | 0.8805 ns |         - |
| forListCountFn        |  39.828 ns | 0.3146 ns | 0.2789 ns |         - |
| forArrayGetLongLength |   9.907 ns | 0.0854 ns | 0.0799 ns |         - |
| forArrayGetDimension  |   9.267 ns | 0.2096 ns | 0.3444 ns |         - |
| forArrayGetUpper      |   8.714 ns | 0.0498 ns | 0.0441 ns |         - |
| forArrayLongLength    |   2.716 ns | 0.0387 ns | 0.0362 ns |         - |
| forArrayLength        |   2.501 ns | 0.0223 ns | 0.0208 ns |         - |
| forListCount          |   2.470 ns | 0.0461 ns | 0.0408 ns |         - |
*/
