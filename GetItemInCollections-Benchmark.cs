using System;
using System.Collections.Generic;
using System.Linq;

namespace test;

[BenchmarkDotNet.Attributes.MemoryDiagnoser]
public class bench_collection
{
    private (int Id, (string, string) Data)[] _arr = Array.Empty<(int, (string, string))>();
    private Dictionary<int, (string, string)> _dict = new();
    private SortedList<int, (string, string)> _sorted = new();

    [BenchmarkDotNet.Attributes.GlobalSetup]
    public void setup()
    {
        _arr = new[]
        {
            (1, ("cat", "missy")),
            (2, ("cat", "polpetta")),
            (3, ("cat", "panto")),
            (4, ("cat", "ciccio")),
            (5, ("cat", "arturo")),
        };
        _dict = _arr.ToDictionary(_ => _.Id, _ => _.Data);
        _sorted = new SortedList<int, (string, string)>(_dict);
    }

    [BenchmarkDotNet.Attributes.Benchmark()]
    public object? by_array()
    {
        const int my_id = 3;
        for (int i = 0; i < _arr.Length; i++)
        {
            if (_arr[i].Id == my_id)
                return _arr[i].Data;
        }

        return null;
    }

    [BenchmarkDotNet.Attributes.Benchmark()]
    public object? by_dict()
    {
        const int my_id = 3;
        return _dict.TryGetValue(my_id, out var v) ? v : null;
    }

    [BenchmarkDotNet.Attributes.Benchmark()]
    public object? by_sorted()
    {
        const int my_id = 3;
        return _sorted.TryGetValue(my_id, out var v) ? v : null;
    }
}
