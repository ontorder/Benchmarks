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

    private void LogInfo(string msg, params object[] templateArgs)
    {
    }
}

/*

| Method   | Mean      | Error    | StdDev   | Median    | Gen0   | Allocated |
|--------- |----------:|---------:|---------:|----------:|-------:|----------:|
| LogRent  | 133.10 ns | 2.392 ns | 2.237 ns | 132.31 ns | 0.0305 |     192 B |
| LogAlloc |  45.89 ns | 1.098 ns | 3.134 ns |  44.68 ns | 0.0535 |     336 B |

*/
