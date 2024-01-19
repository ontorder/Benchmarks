using System.Buffers;

namespace test;

[BenchmarkDotNet.Attributes.MemoryDiagnoser]
public class logperf
{
    private readonly ArrayPool<object> pool = ArrayPool<object>.Shared;

    [BenchmarkDotNet.Attributes.Benchmark]
    public void LogRent()
    {
        var arr = pool.Rent(3);
        arr[0] = 1;
        arr[1] = 2;
        arr[2] = 3;
        LogInfo("rent", arr);
        pool.Return(arr);

        arr = pool.Rent(4);
        arr[0] = 1;
        arr[1] = 2;
        arr[2] = 3;
        arr[3] = 4;
        LogInfo("rent", arr);
        pool.Return(arr);

        arr = pool.Rent(2);
        arr[0] = 10;
        arr[1] = "blablaeto";
        LogInfo("rent", arr);
        pool.Return(arr);
    }

    [BenchmarkDotNet.Attributes.Benchmark]
    public void LogAlloc()
    {
        LogInfo("allo", 1, 2, 3);
        LogInfo("allo", 1, 2, 3, 4);
        LogInfo("allo", 10, "blablaeto");
    }

    [BenchmarkDotNet.Attributes.Benchmark]
    public void LogAlloc2()
    {
        LogInfo2("allo", 1, 2, 3);
        LogInfo2("allo", 1, 2, 3, 4);
        LogInfo2("allo", 10, "blablaeto");
    }

    [BenchmarkDotNet.Attributes.Benchmark]
    public void LogAlloc3()
    {
        LogInfo3("allo", 1, 2, 3);
        LogInfo3("allo", 1, 2, 3, 4);
        LogInfo3("allo", 10, "blablaeto");
    }

    private void LogInfo(string msg, params object?[] templateArgs)
    {
        foreach (var ta in templateArgs)
        {
            if (ta == null) return;
            _ = ta.ToString();
        }
    }

    private void LogInfo2(string msg, object ta1)
    {
        _ = ta1.ToString();
    }

    private void LogInfo2(string msg, object ta1, object ta2)
    {
        _ = ta1.ToString();
        _ = ta2.ToString();
    }

    private void LogInfo2(string msg, object ta1, object ta2, object ta3)
    {
        _ = ta1.ToString();
        _ = ta2.ToString();
        _ = ta3.ToString();
    }
    private void LogInfo2(string msg, object ta1, object ta2, object ta3, object ta4)
    {
        _ = ta1.ToString();
        _ = ta2.ToString();
        _ = ta3.ToString();
        _ = ta4.ToString();
    }

    private void LogInfo3<lt1>(string msg, lt1 ta1)
    {
        _ = ta1.ToString();
    }

    private void LogInfo3<lt1,lt2>(string msg, lt1 ta1, lt2 ta2)
    {
        _ = ta1.ToString();
        _ = ta2.ToString();
    }

    private void LogInfo3<lt1,lt2,lt3>(string msg, lt1 ta1, lt2 ta2, lt3 ta3)
    {
        _ = ta1.ToString();
        _ = ta2.ToString();
        _ = ta3.ToString();
    }

    private void LogInfo3<lt1,lt2,lt3,lt4>(string msg, lt1 ta1, lt2 ta2, lt3 ta3, lt4 ta4)
    {
        _ = ta1.ToString();
        _ = ta2.ToString();
        _ = ta3.ToString();
        _ = ta4.ToString();
    }
}

/*

| Method    | Mean      | Error    | StdDev   | Median    | Gen0   | Allocated |
|---------- |----------:|---------:|---------:|----------:|-------:|----------:|
| LogRent   | 189.64 ns | 1.297 ns | 1.213 ns | 189.49 ns | 0.0355 |     224 B |
| LogAlloc  |  89.02 ns | 1.782 ns | 4.937 ns |  86.34 ns | 0.0587 |     368 B |
| LogAlloc2 |  45.94 ns | 0.294 ns | 0.230 ns |  45.87 ns | 0.0357 |     224 B |
| LogAlloc3 |  24.73 ns | 0.300 ns | 0.266 ns |  24.67 ns | 0.0051 |      32 B |

*/
