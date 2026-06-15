using BenchmarkDotNet.Attributes;

namespace test;

[MemoryDiagnoser, IterationCount(1)]
public class bench_addstring
{
    public IEnumerable<int> enumstringcount()
    {
        for (int esc = 500; esc < 2000; esc += 10) yield return esc;
    }

    //[Benchmark]
    public object liststring_11()
    {
        var l = new List<string>();
        for (var c = 0; c < 11; ++c) l.Add("aaaaaaaaaa" + c);
        return l;
    }

    //[Benchmark]
    public object liststring_111()
    {
        var l = new List<string>();
        for (var c = 0; c < 111; ++c) l.Add("bbbbbbbbbb" + c);
        return l;
    }

    [Benchmark]
    [ArgumentsSource(nameof(enumstringcount))]
    public object liststring_1111(int stringcount)
    {
        var l = new List<string>();
        for (var c = 0; c < stringcount; ++c) l.Add("cccccccccc" + c);
        return l;
    }

    //[Benchmark]
    public object linkedliststrings_1111()
    {
        var ll = new stringlist();
        for (var c = 0; c < 1111; ++c) ll.Add("dddddddddd" + c);
        return ll;
    }

    //[Benchmark]
    public object linkedliststrings_111()
    {
        var ll = new stringlist();
        for (var c = 0; c < 111; ++c) ll.Add("eeeeeeeeee" + c);
        return ll;
    }

    //[Benchmark]
    public object linkedliststrings_11()
    {
        var ll = new stringlist();
        for (var c = 0; c < 11; ++c) ll.Add("ffffffffff" + c);
        return ll;
    }

    private stringlistrent? slr = null;

    public void freeslr()
    {
        slr?.payrent();
        slr = null;
    }

    [Benchmark]
    [ArgumentsSource(nameof(enumstringcount))]
    public object rented_linkedliststrings_1111(int stringcount)
    {
        slr = new stringlistrent();
        for (var c = 0; c < stringcount; ++c) slr.Add("gggggggggg" + c);
        return slr.get();
    }

    //[Benchmark]
    public object rented_linkedliststrings_111()
    {
        slr = new stringlistrent();
        for (var c = 0; c < 111; ++c) slr.Add("hhhhhhhhhh" + c);
        return slr.get();
    }

    //[Benchmark]
    public object rented_linkedliststrings_11()
    {
        slr = new stringlistrent();
        for (var c = 0; c < 11; ++c) slr.Add("iiiiiiiiii" + c);
        return slr.get();
    }
}
/*
| Method                        | Mean        | Error     | StdDev    | Gen0    | Gen1   | Allocated |
|------------------------------ |------------:|----------:|----------:|--------:|-------:|----------:|
| linkedliststrings_11          |    136.8 ns |   1.12 ns |   0.93 ns |  0.1211 | 0.0002 |     760 B |
| rented_linkedliststrings_11   |    139.3 ns |   2.25 ns |   2.92 ns |  0.1211 | 0.0002 |     760 B |
| liststring_11                 |    195.0 ns |   2.14 ns |   1.90 ns |  0.1364 | 0.0002 |     856 B |
| rented_linkedliststrings_111  |  1,253.0 ns |  12.13 ns |  11.35 ns |  1.0757 | 0.0229 |    6760 B |
| linkedliststrings_111         |  1,292.0 ns |  14.33 ns |  13.40 ns |  1.0757 | 0.0229 |    6760 B |
| liststring_111                |  1,348.4 ns |   7.71 ns |   6.84 ns |  1.1978 | 0.0286 |    7520 B |
| liststring_1111               | 16,947.8 ns | 159.18 ns | 141.11 ns | 18.0359 | 3.8452 |  113176 B |
| linkedliststrings_1111        | 18,962.7 ns | 140.24 ns | 124.32 ns | 15.0146 | 3.0212 |   94200 B |
| rented_linkedliststrings_1111 | 19,923.1 ns | 122.45 ns | 114.54 ns | 15.0146 | 3.0212 |   94200 B |

| Method                       | Mean     | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|----------------------------- |---------:|----------:|----------:|-------:|-------:|----------:|
| liststring_514               | 7.171 us | 0.1059 us | 0.0884 us | 7.6599 | 0.8392 |  46.99 KB |
| rented_linkedliststrings_514 | 7.326 us | 0.0383 us | 0.0340 us | 6.0806 | 0.6485 |  37.26 KB |
| linkedliststrings_514        | 8.060 us | 0.0661 us | 0.0586 us | 6.0730 | 0.6409 |  37.26 KB |
*/
