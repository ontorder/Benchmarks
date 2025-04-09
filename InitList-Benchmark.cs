using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace test;

[MemoryDiagnoser]
public class bench_list_init
{
    private readonly int[] _itemsToAdd = Enumerable.Range(0, 9999).ToArray();

    [Benchmark]
    public IReadOnlyCollection<int> regularadd()
    {
        var l = new List<int>();
        foreach (var i in _itemsToAdd) l.Add(i);
        return l;
    }

    [Benchmark]
    public IReadOnlyCollection<int> initdouble()
    {
        var l = new List<int>(_itemsToAdd.Length * 2);
        foreach (var i in _itemsToAdd) l.Add(i);
        return l;
    }
}
/*
x 100
| Method     | Mean     | Error   | StdDev  | Gen0   | Gen1   | Allocated |
|----------- |---------:|--------:|--------:|-------:|-------:|----------:|
| regularadd | 231.8 ns | 1.38 ns | 1.22 ns | 0.1886 | 0.0005 |    1184 B |
| initdouble | 165.9 ns | 1.89 ns | 1.58 ns | 0.1352 | 0.0002 |     848 B |

x 10
| Method     | Mean     | Error    | StdDev   | Gen0   | Allocated |
|----------- |---------:|---------:|---------:|-------:|----------:|
| regularadd | 55.40 ns | 0.372 ns | 0.330 ns | 0.0344 |     216 B |
| initdouble | 24.17 ns | 0.205 ns | 0.191 ns | 0.0204 |     128 B |

x 1000
| Method     | Mean     | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|----------- |---------:|----------:|----------:|-------:|-------:|----------:|
| regularadd | 1.643 us | 0.0076 us | 0.0071 us | 1.3409 | 0.0229 |   8.23 KB |
| initdouble | 1.501 us | 0.0070 us | 0.0066 us | 1.2836 | 0.0362 |   7.86 KB |

x 10000
| Method     | Mean     | Error    | StdDev   | Gen0    | Gen1   | Allocated |
|----------- |---------:|---------:|---------:|--------:|-------:|----------:|
| regularadd | 17.63 us | 0.072 us | 0.064 us | 20.8130 | 4.1504 | 128.32 KB |
| initdouble | 14.06 us | 0.096 us | 0.080 us | 12.6495 | 2.5177 |  78.17 KB |

*/
