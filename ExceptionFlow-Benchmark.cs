using System;
using System.Threading.Tasks;

namespace test;

[BenchmarkDotNet.Attributes.MemoryDiagnoser]
//[BenchmarkDotNet.Attributes.DisassemblyDiagnoser(printSource: true)]
public class exceptionflowperf
{
    private bool out_b;
    public int seed = 2646;

    public exceptionflowperf() => out_b = Random.Shared.Next() % 2 == 0;

    //[BenchmarkDotNet.Attributes.Benchmark]
    public char RegularFlow()
    {
        var v1 = R1(seed);
        switch (v1)
        {
            case RegularEnum.None:
                var c = R2();
                if (!c)
                    return empt("3");
                else
                    return empt("4");

            case RegularEnum.Stuff:
                var b = R2();
                if (b)
                    return empt("2");
                else
                    return empt("1");

            default:
                return char.MinValue;
        }
    }

    private RegularEnum R1(int v) => out_b ? RegularEnum.Stuff : RegularEnum.None;

    public bool R2() => out_b;

    //[BenchmarkDotNet.Attributes.Benchmark]
    public object ExceptionFlow()
    {
        try
        {
            return Choice();
        }
        catch (ExceptionResult1 r1)
        {
            return empt(r1.X);
        }
        catch (ExceptionResult2 r2)
        {
            r2.Y++;
            return empt(" ");
        }
    }

    private char Choice()
    {
        if (out_b) throw new ExceptionResult1 { X = "z" };
        throw new ExceptionResult2() { Y = 5 };
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public char ObjectFlow()
    {
        switch (give_object())
        {
            case (bool _, string s):
                return empt(s);

            case Exception:
                return empt("e");

            default:
                return char.MinValue;
        }
    }

    [BenchmarkDotNet.Attributes.Benchmark]
    public async Task<object> test_async_throw()
    {
        try
        {
            return await async_throw();
        }
        catch
        {
            return 2;
        }
    }

    private static readonly TaskCompletionSource InfiniteTask = new();

    [BenchmarkDotNet.Attributes.Benchmark]
    public async Task<object> test_async_whenany()
    {
        var t = async_whenany();
        _ = await Task.WhenAny(t, InfiniteTask.Task);
        return t.IsCompletedSuccessfully ? t.Result : 2;
    }

    private async Task<object> async_throw() => throw new InvalidOperationException();

    private Task<object> async_whenany() => Task.FromException<object>(new InvalidOperationException());

    public char empt(string s) => s[0];

    private object give_object() => out_b ? (true, "/") : new Exception();

    class ExceptionResult1 : Exception { public string X = "-"; }
    class ExceptionResult2 : Exception { public int Y; }

    enum RegularEnum
    {
        None,
        Stuff,
    }
}

/*
| Method        | Mean          | Error      | StdDev     | Median        | Gen0   | Allocated |
|-------------- |--------------:|-----------:|-----------:|--------------:|-------:|----------:|
| RegularFlow   |     0.0021 ns |  0.0053 ns |  0.0047 ns |     0.0000 ns |      - |         - |
| ExceptionFlow | 5,585.4309 ns | 62.0282 ns | 58.0213 ns | 5,572.2816 ns | 0.0534 |     352 B |
| ObjectFlow    |    10.6596 ns |  0.0472 ns |  0.0394 ns |    10.6552 ns | 0.0204 |     128 B |

// * Warnings *
ZeroMeasurement
  exceptionflowperf.RegularFlow: Default -> The method duration is indistinguishable from the empty method duration


| Method        | Mean          | Error      | StdDev     | Gen0   | Code Size | Allocated |
|-------------- |--------------:|-----------:|-----------:|-------:|----------:|----------:|
| RegularFlow   |     0.2368 ns |  0.0048 ns |  0.0045 ns |      - |        NA |         - |
| ExceptionFlow | 5,530.6784 ns | 44.3156 ns | 41.4528 ns | 0.0610 |     373 B |     400 B |
| ObjectFlow    |    12.9467 ns |  0.1654 ns |  0.1381 ns | 0.0089 |        NA |      56 B |


| Method             | Mean        | Error     | StdDev    | Gen0   | Gen1   | Gen2   | Allocated |
|------------------- |------------:|----------:|----------:|-------:|-------:|-------:|----------:|
| test_async_throw   | 13,541.6 ns | 191.46 ns | 179.10 ns | 0.1831 |      - |      - |    1152 B |
| test_async_whenany |    983.5 ns |   5.40 ns |   5.05 ns | 0.1326 | 0.1316 | 0.0019 |     831 B |
*/
