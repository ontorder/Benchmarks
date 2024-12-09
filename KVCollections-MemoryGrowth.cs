[MemoryDiagnoser]
public class bench_growth
{
    //[Benchmark]
    public ICollection dictionary1()
    {
        var d = new Dictionary<int, double>();
        d.Add(1, 1.0);
        return d;
    }

    [Benchmark]
    public ICollection dictionary10()
    {
        var d = new Dictionary<int, double>();
        d.Add(1, 1.0);
        d.Add(2, 1.1);
        d.Add(3, 1.2);
        d.Add(4, 1.3);
        d.Add(5, 1.4);
        d.Add(6, 1.5);
        d.Add(7, 1.6);
        d.Add(9, 1.7);
        d.Add(8, 1.8);
        d.Add(0, 1.9);
        return d;
    }

    //[Benchmark]
    public ICollection dictionary100()
    {
        var d = new Dictionary<int, double>();
        for (int i = 0; i < 100; i++) d.Add(i, 1.0 + i);
        return d;
    }

    //[Benchmark]
    public ICollection slist1()
    {
        var l = new SortedList<int, double>();
        l.Add(1, 1.0);
        return l;
    }

    //[Benchmark]
    public ICollection slist10()
    {
        var l = new SortedList<int, double>();
        l.Add(1, 1.0);
        l.Add(2, 1.1);
        l.Add(3, 1.2);
        l.Add(4, 1.3);
        l.Add(5, 1.0);
        l.Add(6, 1.1);
        l.Add(7, 1.2);
        l.Add(8, 1.3);
        l.Add(9, 1.2);
        l.Add(0, 1.3);
        return l;
    }

    //[Benchmark]
    public ICollection slist100()
    {
        var l = new SortedList<int, double>();
        for (int i = 0; i < 100; i++) l.Add(i, 1.0 + i);
        return l;
    }

    /*
    | Method        | Mean        | Error     | StdDev   | Gen0   | Gen1   | Allocated |
    |-------------- |------------:|----------:|---------:|-------:|-------:|----------:|
    | slist1        |    24.88 ns |  0.219 ns | 0.205 ns | 0.0255 |      - |     160 B |
    | dictionary1   |    30.32 ns |  0.342 ns | 0.320 ns | 0.0344 |      - |     216 B |
    | slist10       |   216.29 ns |  0.606 ns | 0.506 ns | 0.0865 |      - |     544 B |
    | dictionary10  |    185.0 ns |   1.08 ns |  1.01 ns | 0.1581 | 0.0002 |     992 B |
    | slist100      | 1,349.67 ns | 10.773 ns | 8.996 ns | 0.5379 | 0.0019 |    3376 B |
    | dictionary100 | 1,402.85 ns | 11.522 ns | 9.622 ns | 1.6232 | 0.0401 |   10192 B |
    */
}
