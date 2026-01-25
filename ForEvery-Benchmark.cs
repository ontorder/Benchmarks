using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace test;

[MemoryDiagnoser]
public class bench_forevery
{
    private readonly int[] arr = [1, 2, 3, 4, 5, 6, 7, 8, 9, 0];
    private readonly int t = 1;

    [Benchmark]
    public int linq() => arr.Select(a => a + t).Count();

    [Benchmark]
    public int forevery() => arr.ForEvery(t).Select(static x => x.Source + x.Arg).Count();

    [Benchmark]
    public int shortened() => arr.ForEverySelect(t, static (s, a) => s + a).Count();
}

file static class E
{
    public static IEnumerable<(TSource Source, TArg Arg)> ForEvery<TSource, TArg>(this ICollection<TSource> source, TArg pArg)
    {
        foreach (var item in source)
            yield return (item, pArg);
    }

    public static IEnumerable<TResult> ForEverySelect<TSource, TArg, TResult>(this ICollection<TSource> source, TArg pArg, System.Func<TSource, TArg, TResult> f)
    {
        foreach (var item in source)
            yield return f(item, pArg);
    }
}
/*
| Method    | Mean     | Error    | StdDev   | Gen0   | Allocated |
|---------- |---------:|---------:|---------:|-------:|----------:|
| linq      | 43.61 ns | 0.307 ns | 0.240 ns | 0.0178 |     112 B |
| shortened | 83.19 ns | 0.252 ns | 0.236 ns | 0.0178 |     112 B |
| forevery  | 88.35 ns | 0.703 ns | 0.657 ns | 0.0242 |     152 B |
*/
