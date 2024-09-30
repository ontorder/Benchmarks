using BenchmarkDotNet.Attributes;
using System;

namespace test;

[MemoryDiagnoser]
public class bench_copy
{
    private byte[] _data = [0xAA, 0xEE, 0x09, 0xFF, 0xF0, 0x86, 0x07, 0x04, 0xC0, 0xA8, 0x0B, 0xA8, 0xEE, 0xFC];
    private byte _iplen = 4;

    [Benchmark]
    public byte[] BySpan()
    {
        var d = new byte[_iplen];
        _data[8..(8 + _iplen)].CopyTo(d.AsSpan());
        return d;
    }

    [Benchmark]
    public byte[] BySlice()
    {
        var d = new byte[_iplen];
        _data.AsSpan().Slice(8, _iplen).CopyTo(d);
        return d;
    }

    [Benchmark]
    public byte[] ByFor()
    {
        var d = new byte[_iplen];
        for (int i = 0; i < _iplen; ++i) d[i] = _data[8 + i];
        return d;
    }
}
/*
BySpan          ~12ns
BySlice, ByFor  ~7ns
*/
