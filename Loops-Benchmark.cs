using System.Linq;

namespace test;

[BenchmarkDotNet.Attributes.MemoryDiagnoser]
public class bench_loop
{
    [BenchmarkDotNet.Attributes.Benchmark]
    public void viaAggregate()
    {
        _ = Enumerable.Range(0, 99).Aggregate(0, (_, _) => default);
    }

    [BenchmarkDotNet.Attributes.Benchmark]
    public void viaEnumerator()
    {
        var t = Enumerable.Range(0, 99).GetEnumerator();
        while (t.MoveNext()) ;
    }

    [BenchmarkDotNet.Attributes.Benchmark]
    public void viaFor()
    {
        for (int i = 0; i < 100; ++i) ;
    }
}
/*
| Method        | Mean      | Error    | StdDev   | Gen0   | Allocated |
|-------------- |----------:|---------:|---------:|-------:|----------:|
| viaAggregate  | 127.98 ns | 0.871 ns | 0.815 ns | 0.0062 |      40 B |
| viaEnumerator | 131.61 ns | 1.775 ns | 1.823 ns | 0.0062 |      40 B |
| viaFor        |  28.22 ns | 0.286 ns | 0.254 ns |      - |         - |
*/
