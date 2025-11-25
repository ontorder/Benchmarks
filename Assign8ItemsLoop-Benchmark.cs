[MemoryDiagnoser]
public class bench_cycle
{
    static cose[] coses = new cose[8]
    {
        new cose{ rank=5 },
        new cose{ rank=3 },
        new cose{ rank=8 },
        new cose{ rank=1 },
        new cose{ rank=4 },
        new cose{ rank=7 },
        new cose{ rank=2 },
        new cose{ rank=9 },
    };

    static List<cose> coses_list = coses.ToList();

    [Benchmark]
    public List<cose> listforeach()
    {
        uint rank = 1;
        coses_list.ForEach(_ => _.rank = rank++);
        return coses_list;
    }

    [Benchmark]
    public cose[] justfor()
    {
        for (uint rank = 1, id = 0; id < coses.Length; ++id) coses[id].rank = rank++;
        return coses;
    }

    [Benchmark]
    public cose[] justforeach()
    {
        uint rank = 1;
        foreach (var c in coses) c.rank = rank++;
        return coses;
    }

    [Benchmark]
    public cose[] linqselectlast()
    {
        _ = coses.Select(static (item, index) => item.rank = (uint)index).Last();
        return coses;
    }

    [Benchmark]
    public cose[] refloop()
    {
        uint rank = 1;
        foreach (ref var c in coses.AsSpan()) c.rank = rank++;
        return coses;
    }
}

public sealed class cose
{
    public uint rank;
    public string data = "djdgjahdg;ahfg";
}

/*
| Method         | Mean      | Error     | StdDev    | Gen0   | Allocated |
|--------------- |----------:|----------:|----------:|-------:|----------:|
| refloop        |  3.793 ns | 0.0307 ns | 0.0287 ns |      - |         - |
| justforeach    |  4.333 ns | 0.0266 ns | 0.0249 ns |      - |         - |
| justfor        |  4.926 ns | 0.0448 ns | 0.0397 ns |      - |         - |
| listforeach    | 24.771 ns | 0.1288 ns | 0.1076 ns | 0.0140 |      88 B |
| linqselectlast | 84.164 ns | 0.5718 ns | 0.5069 ns | 0.0166 |     104 B |
*/
