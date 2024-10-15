using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace test;

[MemoryDiagnoser]
public class bench_compare
{
    private static readonly string _string1 = Environment.MachineName; // "ab00";
    private static readonly string _string2 = "dellpc-phenom"; // "ab22";
    private static readonly string[] _strings = ["ab0000", "ab1111", "ab22", "ab3333"];

    private static readonly byte[] _bytes1 = System.Text.Encoding.ASCII.GetBytes(_string1);
    private static readonly List<byte> _bytes1_list = _bytes1.ToList();
    private static readonly byte[] _bytes2 = System.Text.Encoding.ASCII.GetBytes(_string2);
    private static readonly byte[][] _bytess = _strings.Select(System.Text.Encoding.ASCII.GetBytes).ToArray();

    //[Benchmark()]
    public bool GetBytesUnrolled()
    {
        var aBytes = System.Text.Encoding.ASCII.GetBytes(_string1);
        var bBytes = System.Text.Encoding.ASCII.GetBytes(_string2);

        return CompareUnrolled4(aBytes, bBytes);
    }

    //[Benchmark()]
    public bool BytesUnrolled() => CompareUnrolled4(_bytes2, _bytes1);

    //[Benchmark()]
    public bool BytesUnrolledCycle()
    {
        var ret = true;
        foreach (var b in _bytess) ret &= CompareUnrolled4(_bytes2, b);
        return ret;
    }

    //[Benchmark()]
    public bool StringRegular() => _string2 != _string1;

    //[Benchmark()]
    public bool StringRegularIndirect() => RegularCompare(_string2, _string1);

    public bool RegularCompare(string a, string b) => a != b;

    //[Benchmark()]
    public bool StringEquals() => _string2.Equals(_string1);

    //[Benchmark()]
    public bool StringRegularCycle()
    {
        var cmp = true;
        foreach (var s in _strings) cmp &= _string2 != s;
        return cmp;
    }

    [Benchmark()]
    public bool StringNoCase() => string.Compare(_string1, _string2, ignoreCase: true) == 0;

    [Benchmark()]
    public bool StringCultureIgnoreCase() => string.Compare(_string1, _string2, StringComparison.CurrentCultureIgnoreCase) == 0;

    [Benchmark()]
    public bool CmpInvCultureIgnoreCase() => string.Compare(_string1, _string2, StringComparison.InvariantCultureIgnoreCase) == 0;

    [Benchmark()]
    public bool CmpOrdinalIgnoreCase() => string.Compare(_string1, _string2, StringComparison.OrdinalIgnoreCase) == 0;

    [Benchmark()]
    public bool StringToUpper() => _string1.ToUpper() == _string2.ToUpper();

    [Benchmark()]
    public bool StringEqualsNoCase() => _string1.Equals(_string2, StringComparison.CurrentCultureIgnoreCase);

    [Benchmark()]
    public bool InvCultureNoCase() => _string1.Equals(_string2, StringComparison.InvariantCultureIgnoreCase);

    [Benchmark()]
    public bool OrdinalNoCaseOrdinal() => _string1.Equals(_string2, StringComparison.OrdinalIgnoreCase);

    //[Benchmark()]
    public bool RegularBytes()
    {
        var len = Math.Min(_bytes1.Length, _bytes2.Length);
        bool iden = true;
        for (int index = 0; iden && index < len; ++index) iden &= _bytes1[index] == _bytes2[index];
        return iden;
    }

    //[Benchmark()]
    public bool RegularBytesCycle()
    {
        var giden = true;
        foreach (var b in _bytess)
        {
            var len = Math.Min(b.Length, _bytes2.Length);
            bool iden = true;
            for (int index = 0; iden && index < len; ++index) iden &= _bytes1[index] == _bytes2[index];
            giden &= iden;
        }
        return giden;
    }

    //[Benchmark()]
    public bool CopilotUnrolled() => CopilotUnrolled4(_bytes1, _bytes2);

    //[Benchmark()]
    public bool ByteArraySpan() => ArraySpan(_bytes1, 0, _bytes2);

    //[Benchmark()]
    public bool ByteSpanSpan() => ArraySpan(_bytes1, 0, _bytes2);

    //[Benchmark()]
    public bool ByteListSpan() => ListSpan(_bytes1_list, 0, _bytes2);

    //[Benchmark()]
    public bool ViaLinqNo() => _bytes2.SequenceEqual(_bytes1);

    // 4/8 byte posso provare a convertirli in uint/ulong e fare un confronto diretto

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

    private static bool CopilotUnrolled4(byte[] a, byte[] b)
    {
        if (a.Length != b.Length) return false;
        if (a.Length == 1) return a[0] == b[0];
        if (a.Length == 2) return a[0] == b[0] && a[1] == b[1];
        if (a.Length == 3) return a[0] == b[0] && a[1] == b[1] && a[2] == b[2];
        if (a.Length == 4) return a[0] == b[0] && a[1] == b[1] && a[2] == b[2] && a[3] == b[3];
        throw new Exception();
    }

    private static bool ListSpan(List<byte> a, int offset, Span<byte> b)
    {
        int i = 0;
        for (; i < b.Length; ++i) if (a[offset + i] != b[i]) return false;
        return true;
    }

    private static bool ArraySpan(byte[] a, int offset, Span<byte> b)
    {
        int i = 0;
        for (; i < b.Length; ++i) if (a[offset + i] != b[i]) return false;
        return true;
    }

    private static bool SpanSpan(Span<byte> a, Span<byte> b)
    {
        int i = 0;
        for (; i < b.Length; ++i) if (a[i] != b[i]) return false;
        return true;
    }
}

/*

| Method           | Mean       | Error     | StdDev    | Median     | Gen0   | Allocated |
|----------------- |-----------:|----------:|----------:|-----------:|-------:|----------:|
| StringRegular    |  0.0004 ns | 0.0014 ns | 0.0013 ns |  0.0000 ns |      - |         - |
| ByteArraySpan    |   1.594 ns | 0.0282 ns | 0.0235 ns |
| ByteSpanSpan     |   1.626 ns | 0.0282 ns | 0.0264 ns |
| CopilotUnrolled  |   2.854 ns | 0.0239 ns | 0.0212 ns |
| RegularBytes     |   2.795 ns | 0.0613 ns | 0.0573 ns |
| ByteListSpan     |   3.668 ns | 0.0280 ns | 0.0262 ns |
| BytesUnrolled    |  8.5182 ns | 0.0358 ns | 0.0318 ns |  8.5175 ns |      - |         - |
| ViaLinqNo        | 23.284 ns  | 0.3924 ns | 0.3479 ns |
| GetBytesUnrolled | 41.3684 ns | 0.2309 ns | 0.2047 ns | 41.3439 ns | 0.0102 |      64 B |
| StringNoCase     | 42.1062 ns | 0.2677 ns | 0.2504 ns | 42.0516 ns |      - |         - |

ciclo
| Method        | Mean      | Error     | StdDev    | Allocated |
|-------------- |----------:|----------:|----------:|----------:|
| StringRegular |  3.532 ns | 0.0173 ns | 0.0153 ns |         - |
| RegularBytes  | 21.210 ns | 0.1899 ns | 0.1683 ns |         - |
| BytesUnrolled | 60.968 ns | 0.1751 ns | 0.1638 ns |         - |

stringa lunga
| Method                  | Mean       | Error     | StdDev    | Median     | Gen0   | Allocated |
|------------------------ |-----------:|----------:|----------:|-----------:|-------:|----------:|
| StringEquals            |  0.0017 ns | 0.0037 ns | 0.0034 ns |  0.0000 ns |      - |         - |
| StringRegularIndirect   |  0.2251 ns | 0.0152 ns | 0.0142 ns |  0.2173 ns |      - |         - |
| StringRegular           |  0.2695 ns | 0.0136 ns | 0.0127 ns |  0.2633 ns |      - |         - |
| StringNoCase            | 29.7319 ns | 0.3423 ns | 0.2858 ns | 29.7275 ns |      - |         - |
| StringEqualsNoCase      | 31.0747 ns | 0.3569 ns | 0.2980 ns | 31.0604 ns |      - |         - |
| StringCultureIgnoreCase | 31.6723 ns | 0.3426 ns | 0.3037 ns | 31.6149 ns |      - |         - |
| StringToUpper           | 32.5622 ns | 0.4227 ns | 0.3748 ns | 32.6019 ns | 0.0076 |      48 B |

stringa lunga 2
| Method                  | Mean      | Error     | StdDev    | Gen0   | Allocated |
|------------------------ |----------:|----------:|----------:|-------:|----------:|
| OrdinalNoCaseOrdinal    |  1.930 ns | 0.0038 ns | 0.0032 ns |      - |         - |
| CmpOrdinalIgnoreCase    |  3.953 ns | 0.0160 ns | 0.0141 ns |      - |         - |
| CmpInvCultureIgnoreCase | 31.128 ns | 0.0734 ns | 0.0651 ns |      - |         - |
| InvCultureNoCase        | 32.081 ns | 0.1609 ns | 0.1344 ns |      - |         - |
| StringNoCase            | 32.553 ns | 0.0571 ns | 0.0477 ns |      - |         - |
| StringEqualsNoCase      | 33.049 ns | 0.1256 ns | 0.0981 ns |      - |         - |
| StringCultureIgnoreCase | 34.095 ns | 0.0640 ns | 0.0567 ns |      - |         - |
| StringToUpper           | 36.873 ns | 0.1276 ns | 0.1131 ns | 0.0076 |      48 B |

*/
