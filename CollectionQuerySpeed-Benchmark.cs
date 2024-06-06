using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace test;

[MemoryDiagnoser]
public class bench_collection_speed
{
    static private readonly IEnumerable<int> _list = Enumerable.Range(1, 100);

    public int filter_ienu() => _list.Where(_ => _ % 2 == 1).Aggregate((a, b) => a + b);

    public int filter_ienu2() => _list.Where(_ => _ % 2 == 1).Where(_ => _ < 999).Aggregate((a, b) => a + b);

    public int filter_qrbl() => _list.AsQueryable().Where(_ => _ % 2 == 1).Aggregate((a, b) => a + b);

    public int filter_qrbl2() => _list.AsQueryable().Where(_ => _ % 2 == 1).Where(_ => _ < 999).Aggregate((a, b) => a + b);

    public int filter_fore()
    {
        int acc = 0;
        foreach (var i in _list) acc += i;
        return acc;
    }

    [Benchmark]
    public int filter_fore2()
    {
        int acc = 0;
        foreach (var i in _list) if (i < 999) acc += i;
        return acc;
    }
}
/*
size 100
| Method       | Mean         | Error       | StdDev      | Gen0   | Gen1   | Allocated |
|------------- |-------------:|------------:|------------:|-------:|-------:|----------:|
| filter_ienu  |     291.4 ns |     1.12 ns |     0.99 ns | 0.0153 |      - |      96 B |
| filter_ienu2 |     442.5 ns |     4.33 ns |     3.84 ns | 0.0391 |      - |     248 B |
| filter_qrbl  | 349,859.7 ns | 3,395.00 ns | 3,175.68 ns | 2.4414 | 1.9531 |   17705 B |
| filter_qrbl2 | 455,559.2 ns | 3,127.98 ns | 2,772.88 ns | 3.9063 | 2.9297 |   25163 B |
| filter_fore  |     154.4 ns |     2.29 ns |     2.14 ns | 0.0062 |      - |      40 B |
| filter_fore2 |     174.4 ns |     0.74 ns |     0.70 ns | 0.0062 |      - |      40 B |

size 500_000_000
| Method       | Mean       | Error    | StdDev   | Allocated |
|------------- |-----------:|---------:|---------:|----------:|
| filter_ienu  | 3,102.7 ms | 21.36 ms | 18.93 ms |     496 B |
| filter_ienu2 | 1,552.1 ms |  4.24 ms |  3.76 ms |     648 B |
| filter_qrbl  | 3,028.1 ms | 20.54 ms | 17.15 ms |   19048 B |
| filter_qrbl2 | 1,648.3 ms | 16.25 ms | 15.20 ms |   26456 B |
| filter_fore  |   887.8 ms |  4.46 ms |  3.95 ms |     440 B |
| filter_fore2 | 1,000.8 ms |  3.23 ms |  2.86 ms |     440 B |
*/
