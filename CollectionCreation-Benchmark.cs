using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace test;

[MemoryDiagnoser]
public class bench_list_alloc
{
    static private readonly IEnumerable<int> _list = Enumerable.Range(1, 999);

    public void with_list()
    {
        var l = new List<int>();
        foreach (var i in _list)
        {
            if (i % 2 == 1 && i < 999)
                l.Add(i);
        }
    }

    public void with_array()
    {
        var filt = _list.Where(i => i % 2 == 1 && i < 999);
        var count = filt.Count();
        var arr = new int[count];
        foreach (var it in filt.Select((v, id) => (v, id)))
        {
            arr[it.id] = it.v;
        }
    }

    public void with_array_man()
    {
        var filt = _list.Where(i => i % 2 == 1 && i < 999);
        var count = filt.Count();
        var arr = new int[count];
        var id = 0;
        foreach (var it in filt)
        {
            arr[id++] = it;
        }
    }

    [Benchmark]
    public void with_array_partial()
    {
        var filt = _list.Where(i => i % 2 == 1 && i < 999);
        var arr = new int[_list.Count()];
        var id = 0;
        foreach (var it in filt)
        {
            arr[id++] = it;
        }
    }

    public void to_array()
    {
        _ = _list.Where(i => i % 2 == 1 && i < 999).ToArray();
    }
}

/*
size 999
| Method             | Mean     | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|---------------     |---------:|----------:|----------:|-------:|-------:|----------:|
| with_list          | 2.337 us | 0.0154 us | 0.0129 us | 0.6905 | 0.0038 |   4.24 KB |
| with_array         | 8.323 us | 0.0501 us | 0.0444 us | 0.3510 |      - |   2.19 KB |
| with_array_man     | 5.710 us | 0.0124 us | 0.0110 us | 0.3433 |      - |   2.11 KB |
| to_array           | 2.557 us | 0.0148 us | 0.0139 us | 0.7172 |      - |   4.41 KB |
| with_array_partial | 3.260 us | 0.0068 us | 0.0057 us | 0.6561 |      - |   4.02 KB |

size 9
| Method             | Mean      | Error    | StdDev   | Gen0   | Allocated |
|---------------     |----------:|---------:|---------:|-------:|----------:|
| with_list          |  52.37 ns | 0.329 ns | 0.308 ns | 0.0268 |     168 B |
| with_array         | 133.48 ns | 0.730 ns | 0.683 ns | 0.0420 |     264 B |
| with_array_man     |  94.60 ns | 0.640 ns | 0.567 ns | 0.0293 |     184 B |
| to_array           |  81.16 ns | 0.530 ns | 0.442 ns | 0.0381 |     240 B |
| with_array_partial |  65.14 ns | 0.476 ns | 0.422 ns | 0.0254 |     160 B |
*/
