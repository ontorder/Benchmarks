using BenchmarkDotNet.Attributes;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace test;

[MemoryDiagnoser]
[SimpleJob(BenchmarkDotNet.Engines.RunStrategy.ColdStart)]
public class bench_taskwhenany
{
    static private readonly Task t1 = Task.CompletedTask;
    static private readonly Task t2 = Task.CompletedTask;
    static private readonly Task? t3 = Task.CompletedTask;
    static private readonly Task? t4 = null;

    [Benchmark]
    public async Task by_list()
    {
        var l = new List<Task>(3) { t1, t2 };
        if (t3 != null) l.Add(t3);
        if (t4 != null) l.Add(t4);
        await Task.WhenAny(l);
    }

    [Benchmark]
    public async Task by_array()
    {
        int c = 2 + (t3 == null ? 0 : 1) + (t4 == null ? 0 : 1);
        var (a, i) = (new Task[c], 2);
        var s = a.AsSpan();
        (s[0], s[1]) = (t1, t2);
        if (t3 != null) s[i++] = t3;
        if (t4 != null) s[i++] = t4;
        await Task.WhenAny(a);
    }

    [Benchmark]
    public async Task by_array2()
    {
        var o1 = t3 == null ? 0 : 1;
        var o2 = t4 == null ? 0 : 1;
        int c = 2 + o1 + o2;
        var a = new Task[c];
        var s = a.AsSpan();
        (s[0], s[1]) = (t1, t2);
        if (t3 != null) s[2] = t3;
        if (t4 != null) s[c - 1] = t4;
        await Task.WhenAny(a);
    }

    [Benchmark]
    public async Task by_rent()
    {
        var (ap, id) = (ArrayPool<Task>.Shared.Rent(4), 2);
        ap[0] = t1;
        ap[1] = t2;
        if (t3 != null) ap[id++] = t3;
        if (t4 != null) ap[id++] = t4;
        _ = await Task.WhenAny(ap[..id]);
        ArrayPool<Task>.Shared.Return(ap);
    }

    [Benchmark]
    public async Task by_switch()
    {
        var t = (t3, t4) switch
        {
            (null, null) => Task.WhenAny(t1, t2),
            (not null, null) => Task.WhenAny(t1, t2, t3),
            (null, not null) => Task.WhenAny(t1, t2, t4),
            (not null, not null) => Task.WhenAny(t1, t2, t3, t4),
        };
        _ = await t;
    }
}
/*
| Method    | Mean      | Error    | StdDev   | Gen0   | Allocated |
|---------- |----------:|---------:|---------:|-------:|----------:|
| by_switch |  91.88 ns | 0.286 ns | 0.253 ns | 0.0293 |     184 B |
| by_list   | 140.42 ns | 1.792 ns | 1.588 ns | 0.0343 |     216 B |
| by_rent   | 147.19 ns | 0.715 ns | 0.668 ns | 0.0293 |     184 B |

| Method    | Mean      | Error    | StdDev   | Gen0   | Allocated |
|---------- |----------:|---------:|---------:|-------:|----------:|
| by_array  |  80.63 ns | 0.683 ns | 0.639 ns | 0.0293 |     184 B |
| by_switch |  80.74 ns | 0.492 ns | 0.411 ns | 0.0293 |     184 B |
| by_list   | 126.55 ns | 1.113 ns | 0.987 ns | 0.0343 |     216 B |
| by_rent   | 130.79 ns | 0.868 ns | 0.770 ns | 0.0293 |     184 B |

| Method    | Mean      | Error    | StdDev   | Gen0   | Allocated |
|---------- |----------:|---------:|---------:|-------:|----------:|
| by_switch |  80.55 ns | 0.702 ns | 0.657 ns | 0.0293 |     184 B |
| by_array  |  80.63 ns | 0.481 ns | 0.376 ns | 0.0293 |     184 B |
| by_array2 |  81.13 ns | 0.558 ns | 0.522 ns | 0.0293 |     184 B |
| by_rent   | 124.94 ns | 1.040 ns | 0.922 ns | 0.0293 |     184 B |
| by_list   | 126.00 ns | 2.539 ns | 3.118 ns | 0.0343 |     216 B |

| cold start| Mean     | Error    | StdDev    | Median   | Allocated |
|---------- |---------:|---------:|----------:|---------:|----------:|
| by_switch | 19.53 us | 49.09 us | 144.75 us | 4.100 us |     584 B |
| by_array  | 19.56 us | 48.17 us | 142.04 us | 3.900 us |     584 B |
| by_list   | 22.57 us | 51.69 us | 152.42 us | 6.100 us |     616 B |
| by_array2 | 24.19 us | 64.89 us | 191.33 us | 4.300 us |     584 B |
| by_rent   | 26.24 us | 56.41 us | 166.32 us | 5.200 us |    1072 B |
*/
