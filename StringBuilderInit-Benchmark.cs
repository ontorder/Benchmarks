using BenchmarkDotNet.Attributes;
using System;
using System.Linq;
using System.Text;

namespace test;

[MemoryDiagnoser]
public class bench_init_sb
{
    private static string[] somedata;

    const int datasize = 256;
    const int maxstringlen = 32;

    static bench_init_sb()
    {
        somedata = new string[datasize];
        for (int i = 0; i < somedata.Length; i++)
        {
            somedata[i] = new string(Enumerable.Repeat('-', Random.Shared.Next(8, maxstringlen)).ToArray());
        }
    }

    [Benchmark]
    public string noinit()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < somedata.Length; i++) sb.Append(somedata[i]);
        return sb.ToString();
    }

    [Benchmark]
    public string initsize()
    {
        var sb = new StringBuilder(datasize * maxstringlen);
        for (int i = 0; i < somedata.Length; i++) sb.Append(somedata[i]);
        return sb.ToString();
    }

    [Benchmark]
    public string initsum()
    {
        var totsize = somedata.Sum(x => x.Length);
        var sb = new StringBuilder(totsize);
        for (int i = 0; i < somedata.Length; i++) sb.Append(somedata[i]);
        return sb.ToString();
    }
}
/*
4 lines 8 chars
| Method   | Mean     | Error    | StdDev   | Gen0   | Allocated |
|--------- |---------:|---------:|---------:|-------:|----------:|
| noinit   | 56.34 ns | 0.569 ns | 0.504 ns | 0.0471 |     296 B |
| initsize | 35.46 ns | 0.271 ns | 0.240 ns | 0.0357 |     224 B |
| initsum  | 56.83 ns | 0.511 ns | 0.478 ns | 0.0408 |     256 B |

30 lines 16 chars
| Method   | Mean     | Error   | StdDev  | Gen0   | Gen1   | Allocated |
|--------- |---------:|--------:|--------:|-------:|-------:|----------:|
| noinit   | 304.4 ns | 1.75 ns | 1.46 ns | 0.3467 | 0.0019 |   2.13 KB |
| initsize | 194.1 ns | 3.87 ns | 3.98 ns | 0.2766 | 0.0010 |    1.7 KB |
| initsum  | 279.7 ns | 3.34 ns | 2.79 ns | 0.2294 | 0.0005 |   1.41 KB |

256 lines 32 chars
| Method   | Mean     | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|--------- |---------:|----------:|----------:|-------:|-------:|----------:|
| noinit   | 2.605 us | 0.0208 us | 0.0184 us | 4.3564 | 0.2708 |   26.7 KB |
| initsize | 2.100 us | 0.0216 us | 0.0192 us | 4.2458 | 0.1297 |  26.04 KB |
| initsum  | 2.730 us | 0.0184 us | 0.0154 us | 3.2768 | 0.1259 |  20.08 KB |
*/
