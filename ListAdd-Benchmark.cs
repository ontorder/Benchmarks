using System.Buffers;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Attributes;

namespace test;

[MemoryDiagnoser]
//[IterationCount(1)]
public class bench_addstring
{
    public IEnumerable<int> enumstringcount()
    {
        for (int esc = 500; esc < 2000; esc += 10) yield return esc;
    }

    [Params(10, 100, 1000, 1100)]
    public int addscount;

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
    //[ArgumentsSource(nameof(enumstringcount))]
    public object liststring_1111()
    {
        var l = new List<string>();
        for (var c = 0; c < addscount; ++c) l.Add("cccccccccc" + c);
        return l;
    }

    [Benchmark]
    public object linkedliststrings_1111()
    {
        var ll = new stringlist();
        for (var c = 0; c < addscount; ++c) ll.Add("dddddddddd" + c);
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

    //[Benchmark]
    //[ArgumentsSource(nameof(enumstringcount))]
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

internal sealed class stringlistrent
{
    private readonly LinkedList<string[]> _ll = new();
    private string[] _current = ArrayPool<string>.Shared.Rent(16);
    private int _current_index = 0;

    public void Add(string s)
    {
        if (_current_index == _current.Length)
        {
            _current = ArrayPool<string>.Shared.Rent(16);
            _current_index = 0;
            _ll.AddLast(_current);
        }
        _current[_current_index++] = s;
    }

    public object get() => _ll;

    public void payrent()
    {
        for (var t = _ll.First; t != null; t = t.Next)
            ArrayPool<string>.Shared.Return(t.Value);
    }
}

internal sealed class stringlist
{
    private readonly LinkedList<string[]> _ll = new();
    private string[] _current = new string[16];
    private int _current_index = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(string s)
    {
        string[] arr = _current;
        int idx = _current_index;
        if ((uint)idx < (uint)arr.Length)
        {
            arr[idx] = s;
            _current_index = idx + 1;
        }
        else
        {
            AddSlow(s);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddSlow(string s)
    {
        var store = new string[16];
        store[0] = s;
        _ll.AddLast(store);
        _current = store;
        _current_index = 1;
    }

    public object get() => _ll;
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

stringlist v2
| Method            | addscount | Mean        | Error     | StdDev    | Median      | Gen0    | Gen1   | Allocated |
|------------------ |---------- |------------:|----------:|----------:|------------:|--------:|-------:|----------:|
| linkedliststrings | 10        |    147.0 ns |   4.40 ns |  12.27 ns |    142.7 ns |  0.1135 | 0.0002 |     712 B |
| liststring        | 10        |    176.1 ns |   4.38 ns |  11.76 ns |    172.5 ns |  0.1287 | 0.0002 |     808 B |
| liststring        | 100       |  1,211.0 ns |  10.91 ns |  10.21 ns |  1,215.4 ns |  1.1139 | 0.0248 |    6992 B |
| linkedliststrings | 100       |  1,211.7 ns |   8.42 ns |   7.88 ns |  1,209.8 ns |  0.9918 | 0.0191 |    6232 B |
| liststring        | 1000      | 13,934.5 ns | 114.40 ns |  95.53 ns | 13,957.7 ns | 13.8550 | 2.5940 |   87000 B |
| linkedliststrings | 1000      | 15,970.6 ns | 127.45 ns | 119.21 ns | 15,969.0 ns | 13.2141 | 2.4109 |   83032 B |
| liststring        | 1100      | 16,345.5 ns | 178.68 ns | 158.40 ns | 16,322.4 ns | 17.8528 | 4.2419 |  112208 B |
| linkedliststrings | 1100      | 17,490.1 ns | 163.77 ns | 145.18 ns | 17,497.0 ns | 14.8010 | 2.9907 |   93032 B |
*/
