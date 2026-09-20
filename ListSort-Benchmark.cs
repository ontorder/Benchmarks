using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace test;

public class dumbsortbench
{
    private readonly List<Cue> _cues = [];

    [GlobalSetup]
    //[IterationSetup]
    public void Setup()
    {
        var rng = new Random(42);
        _cues.Clear();
        for (var i = 0; i < 100; ++i)
        {
            var start = rng.Next(int.MaxValue);
            var end = rng.Next(int.MaxValue);
            _cues.Add(new Cue { Start = new(start), End = new(end) });
        }
    }

    [Benchmark]
    public object flat_ifs()
    {
        var cues = _cues.ToList();
        var cmp = new Comparison<Cue>(static (a, b) =>
        {
            if (a.Start > b.Start) return 1;
            if (a.Start < b.Start) return -1;
            if (a.End > b.End) return 1;
            if (a.End < b.End) return -1;
            return 0;
        });
        cues.Sort(cmp);
        return _cues;
    }

    [Benchmark]
    public object dumb_switch()
    {
        var cues = _cues.ToList();
        var cmp = new Comparison<Cue>(static (a, b) =>
            (a, b) switch
            {
                _ when a.Start > b.Start => 1,
                _ when a.Start < b.Start => -1,
                _ when a.End > b.End => 1,
                _ when a.End < b.End => -1,
                _ => 0,
            });
        cues.Sort(cmp);
        return _cues;
    }

    [Benchmark]
    public object compareto()
    {
        var cues = _cues.ToList();
        var cmp = new Comparison<Cue>(static (a, b) =>
        {
            var temp = a.Start.CompareTo(b.Start);
            return temp == 0 ? a.End.CompareTo(b.End) : temp;
        });
        cues.Sort(cmp);
        return _cues;
    }

    [Benchmark]
    public object tuple_compare()
    {
        //var cues =  new List<Cue>(_cues);
        var cues = _cues.ToList();
        var cmp = new Comparison<Cue>(static (a, b) => (a.Start, a.End).CompareTo((b.Start, b.End)));
        cues.Sort(cmp);
        return _cues;
    }

    [Benchmark]
    public object ticks_compare()
    {
        var cues = _cues.ToList();
        var cmp = new Comparison<Cue>(static (a, b) =>
        {
            var temp = a.Start.Ticks.CompareTo(b.Start.Ticks);
            return temp == 0 ? a.End.Ticks.CompareTo(b.End.Ticks) : temp;
        });
        cues.Sort(cmp);
        return _cues;
    }

    private const int quantizBit = 14;

    [Benchmark]
    public object quantize()
    {
        var cues = _cues.ToList();
        var cmp = new Comparison<Cue>(static (a, b) =>
        {
// correggere
            var key1 = (((ulong)a.Start.Ticks >> quantizBit) << 32) | (ulong)a.End.Ticks >> quantizBit;
            var key2 = (((ulong)b.Start.Ticks >> quantizBit) << 32) | (ulong)b.End.Ticks >> quantizBit;
            return
                key1 > key2 ? -1 :
                key1 < key2 ? -1 :
                0;
        });
        cues.Sort(cmp);
        return _cues;
    }
}

public sealed class Cue
{
    public TimeSpan End;
    public TimeSpan Start;
}

/*

x10
| Method        | Mean     | Error   | StdDev  |
|-------------- |---------:|--------:|--------:|
| tuple_compare | 128.8 ns | 1.11 ns | 0.99 ns |
| flat_ifs      | 129.0 ns | 2.52 ns | 2.70 ns |
| dumb_switch   | 129.1 ns | 0.59 ns | 0.49 ns |
| ticks_compare | 129.4 ns | 0.55 ns | 0.51 ns |
| compareto     | 141.9 ns | 1.08 ns | 1.01 ns |
| quantize      | 212.3 ns | 1.98 ns | 1.85 ns |

x100
| Method        | Mean     | Error     | StdDev    |
|-------------- |---------:|----------:|----------:|
| flat_ifs      | 1.980 us | 0.0240 us | 0.0225 us |
| dumb_switch   | 2.154 us | 0.0141 us | 0.0132 us |
| tuple_compare | 2.251 us | 0.0162 us | 0.0151 us |
| ticks_compare | 2.452 us | 0.0151 us | 0.0126 us |
| compareto     | 2.541 us | 0.0161 us | 0.0150 us |
| quantize      |       NA |        NA |        NA |

*/
