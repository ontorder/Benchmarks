// not a well made test, it's for my specific case

using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace test;

[MemoryDiagnoser]
public class bench_compare
{
    private static readonly string _string1 = "ab0000";
    private static readonly string _string2 = "ab111";

    private static readonly byte[] _bytes1 = System.Text.Encoding.ASCII.GetBytes(_string1);
    private static readonly byte[] _bytes2 = System.Text.Encoding.ASCII.GetBytes(_string2);

    [Benchmark()]
    public bool GetBytesUnrolled()
    {
        var aBytes = System.Text.Encoding.ASCII.GetBytes(_string1);
        var bBytes = System.Text.Encoding.ASCII.GetBytes(_string2);

        return CompareUnrolled4(aBytes, bBytes);
    }

    [Benchmark()]
    public bool BytesUnrolled()
    {
        return CompareUnrolled4(_bytes1, _bytes2);
    }

    [Benchmark()]
    public bool StringRegular()
    {
        return _string1 != _string2;
    }

    [Benchmark()]
    public bool StringNoCase()
    {
        return string.Compare(_string1, _string2, ignoreCase: true) == 0;
    }

    [Benchmark()]
    public bool RegularBytes()
    {
        bool iden = true;
        var len = Math.Min(_bytes1.Length, _bytes2.Length);
        for (int index = 0; iden && index < len; ++index) iden &= _bytes1[index] == _bytes2[index];
        return iden;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CompareUnrolled4(IReadOnlyList<byte> a, byte[] b, int aStart = 0, int bStart = 0)
    {
        bool r;
        int bLen = b.Length;
        int aLen = a.Count;

        if (aStart > aLen || bStart > bLen) return false;
        r = a[aStart] == b[bStart];
        if (bLen == 1) return r;
        if (!r) return false;

        ++aStart; ++bStart;
        if (aStart > aLen || bStart > bLen) return false;
        r = a[aStart] == b[bStart];
        if (bLen == 2) return r;
        if (!r) return false;

        ++aStart; ++bStart;
        if (aStart > aLen || bStart > bLen) return false;
        r = a[aStart] == b[bStart];
        if (bLen == 3) return r;
        if (!r) return false;

        ++aStart; ++bStart;
        if (aStart > aLen || bStart > bLen) return false;
        r = a[aStart] == b[bStart];
        if (bLen == 4) return r;
        if (!r) return false;

        throw new Exception("compare too long");
    }
}

/*

| Method           | Mean       | Error     | StdDev    | Median     | Gen0   | Allocated |
|----------------- |-----------:|----------:|----------:|-----------:|-------:|----------:|
| StringRegular    |  0.0004 ns | 0.0014 ns | 0.0013 ns |  0.0000 ns |      - |         - |
| RegularBytes     |  4.8723 ns | 0.0193 ns | 0.0181 ns |  4.8732 ns |      - |         - |
| BytesUnrolled    |  8.5182 ns | 0.0358 ns | 0.0318 ns |  8.5175 ns |      - |         - |
| GetBytesUnrolled | 41.3684 ns | 0.2309 ns | 0.2047 ns | 41.3439 ns | 0.0102 |      64 B |
| StringNoCase     | 42.1062 ns | 0.2677 ns | 0.2504 ns | 42.0516 ns |      - |         - |

*/
