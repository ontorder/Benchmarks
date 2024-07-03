using BenchmarkDotNet.Attributes;
using System.IO;

namespace test;

[MemoryDiagnoser]
public class bench_write
{
    [Benchmark]
    public void Write()
    {
        var bytes = new byte[1000];
        using var write = File.OpenWrite(@"c:\temp\centomila");
        for (int a = 0; a < 100; ++a)
            write.Write(bytes, 0, bytes.Length);
    }
}
/*
| Method | Mean     | Error   | StdDev  | Gen0   | Allocated |
|------- |---------:|--------:|--------:|-------:|----------:|
| Write  | 181.4 us | 3.50 us | 3.27 us | 0.7324 |   5.26 KB |
*/
