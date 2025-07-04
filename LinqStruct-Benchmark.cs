using BenchmarkDotNet.Attributes;
using System;
using System.Linq;

namespace test;

[MemoryDiagnoser]
public class bench_struct_linq
{
    public static c1 c;
    public static string SeatNumber = "seat";

    static bench_struct_linq()
    {
        c = new();
        c.ss = new[]
        {
            new s1 { b = true, i = 1, s = "seat1" },
            new s1 { b = true, i = 2, s = "seat2" },
            new s1 { b = true, i = 3, s = "seat3" },
            new s1 { b = true, i = 4, s = "seat4" },
            new s1 { b = true, i = 5, s = "seat5" },
        };
        SeatNumber += Random.Shared.Next(1, 5);
    }

    [Benchmark]
    public long? linq_found_field()
    {
        var found = c.ss
            .Where(_ => _.s == SeatNumber)
            .Select(_ => (true, _.i))
            .FirstOrDefault();
        return found.Item1 ? found.i : null;
    }

    [Benchmark]
    public long? just_foreach()
    {
        foreach (var z in c.ss)
            if (z.s == SeatNumber)
                return z.i;
        return null;
    }

    [Benchmark]
    public long? just_for()
    {
        for (var i = 0; i < c.ss.Length; ++i)
            if (c.ss[i].s == SeatNumber)
                return c.ss[i].i;
        return null;
    }

    [Benchmark]
    public long? linq_scalar()
    {
        var z = c.ss
            .Where(_ => _.s == SeatNumber)
            .Select(_ => _.i)
            .FirstOrDefault(-1);
        return z < 0 ? null : z;
    }

    [Benchmark]
    public long? linq_firstordefault()
    {
        var z = c.ss.FirstOrDefault(_ => _.s == SeatNumber, new() { i = -1 });
        return z.i < 0 ? null : z.i;
    }

    static s1 defaultValue = new() { i = -1 };

    [Benchmark]
    public long? linq_firstordefault_static()
    {
        var z = c.ss.FirstOrDefault(_ => _.s == SeatNumber, defaultValue);
        return z.i < 0 ? null : z.i;
    }

    public struct s1
    {
        public bool b;
        public long i;
        public string s;
    }

    public class c1
    {
        public s1[] ss;
    }
}
/*

| Method              | Mean      | Error     | StdDev    | Gen0   | Allocated |
|-------------------- |----------:|----------:|----------:|-------:|----------:|
| just_foreach        |  4.749 ns | 0.0386 ns | 0.0342 ns |      - |         - |
| linq_scalar         | 62.014 ns | 0.1935 ns | 0.1715 ns | 0.0191 |     120 B |
| linq_found_field    | 70.738 ns | 0.4309 ns | 0.4030 ns | 0.0204 |     128 B |

| Method                     | Mean      | Error     | StdDev    | Gen0   | Allocated |
|--------------------------- |----------:|----------:|----------:|-------:|----------:|
| just_for                   |  6.103 ns | 0.0332 ns | 0.0294 ns |      - |         - |
| just_foreach               | 13.480 ns | 0.1017 ns | 0.0951 ns |      - |         - |
| linq_firstordefault        | 27.20 ns  | 0.112 ns  | 0.099 ns  | 0.0051 |      32 B |
| linq_firstordefault_static | 42.55 ns  | 0.328 ns  | 0.290 ns  | 0.0051 |      32 B |
| linq_scalar                | 65.569 ns | 0.5561 ns | 0.4644 ns | 0.0191 |     120 B |
| linq_found_field           | 76.004 ns | 0.3167 ns | 0.2645 ns | 0.0204 |     128 B |
|--------------------------- |----------:|----------:|----------:|-------:|----------:|

*/
