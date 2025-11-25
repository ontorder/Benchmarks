using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace test;

[MemoryDiagnoser]
public class bench_cycle
{
    static cose[] coses = new cose[8]
    {
        new(){ rank=5 },
        new(){ rank=3 },
        new(){ rank=8 },
        new(){ rank=1 },
        new(){ rank=4 },
        new(){ rank=7 },
        new(){ rank=2 },
        new(){ rank=9 },
    };

    static List<cose> coses_list = [.. coses];

    //[Benchmark]
    public List<cose> listforeach()
    {
        uint rank = 1;
        coses_list.ForEach(_ => _.rank = rank++);
        return coses_list;
    }

    //[Benchmark]
    public cose[] justfor()
    {
        for (uint rank = 1, id = 0; id < coses.Length; ++id) coses[id].rank = rank++;
        return coses;
    }

    [Benchmark]
    public cose[] spanfor()
    {
        var s = coses.AsSpan();
        for (var id = 0; id < coses.Length; ++id) s[id].rank = (uint)id + 1;
        return coses;
    }

    //[Benchmark]
    public cose[] justforeach()
    {
        uint rank = 1;
        foreach (var c in coses) c.rank = rank++;
        return coses;
    }

    //[Benchmark]
    public cose[] linqselectlast()
    {
        _ = coses.Select(static (item, index) => item.rank = (uint)index).Last();
        return coses;
    }

    //[Benchmark]
    public cose[] refloop()
    {
        uint rank = 1;
        foreach (ref var c in coses.AsSpan()) c.rank = rank++;
        return coses;
    }

    //[Benchmark]
    public cose[] innerselect()
    {
        foreach (var z in coses.Select((Item, Index) => (Item, Index))) z.Item.rank = (uint)z.Index;
        return coses;
    }
}

public sealed class cose
{
    public uint rank;
    public string data = "djdgjahdg;ahfg";
}

/*
| Method         | Mean       | Error     | StdDev    | Gen0   | Allocated |
|--------------- |-----------:|----------:|----------:|-------:|----------:|
| refloop        |   3.793 ns | 0.0307 ns | 0.0287 ns |      - |         - |
| justforeach    |   4.333 ns | 0.0266 ns | 0.0249 ns |      - |         - |
| justfor        |   4.926 ns | 0.0448 ns | 0.0397 ns |      - |         - |
| spanfor        |   4.965 ns | 0.0382 ns | 0.0358 ns |      - |         - |
| listforeach    |  24.771 ns | 0.1288 ns | 0.1076 ns | 0.0140 |      88 B |
| linqselectlast |  84.164 ns | 0.5718 ns | 0.5069 ns | 0.0166 |     104 B |
| innerselect    | 117.2   ns | 0.83   ns | 0.77   ns | 0.0191 |     120 B |
*/
