using BenchmarkDotNet.Attributes;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace test;

[MemoryDiagnoser]
//[SimpleJob(BenchmarkDotNet.Engines.RunStrategy.ColdStart)]
public partial class bench_stringremove
{
    //public const string sample_string = "v4v4v4/V3/V33V3";
    public const string sample_string = "cat1/cat2/cat3 22221133 CERVINIA_DAY_1 *rec* *bt123* <anita>";
    //public const string sample_string2 = "20250203_RAVA_CERVINIA_DAY 1_REDKX_REC 709 4K_A011_A087_020232*";

    //[Benchmark]
    public string remove_replace() => sample_string.Replace("/", string.Empty);

    [Benchmark]
    public string remove_span()
    {
        Span<char> buffer = stackalloc char[sample_string.Length];
        int pos = 0;
        Span<char> invalidcharsspan = invalidchars.AsSpan();
        foreach (char c in sample_string)
        {
            //if (c != '/')
            if (MemoryExtensions.Contains(invalidcharsspan, c) == false)
            {
                buffer[pos++] = c;
            }
        }
        return new string(buffer.Slice(0, pos));
    }

    [Benchmark]
    public string remove_stringbuilder()
    {
        Span<char> invalidcharsspan = invalidchars.AsSpan();
        var sb = new System.Text.StringBuilder(sample_string.Length);
        foreach (char c in sample_string)
        {
            //if (c != '/')
            if (MemoryExtensions.Contains(invalidcharsspan, c) == false)
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    [Benchmark]
    public string remove_linq()
    {
        var chars = sample_string.Where(c => invalidchars.Contains(c) == false).ToArray();
        //var chars = sample_string.Where(static c => c != '/').ToArray();
        return new string(chars);
    }

    [Benchmark]
    public string remove_createstring()
    {
        var nremove = 0;
        Span<char> invalidcharsspan = invalidchars.AsSpan();
        var sp = sample_string.AsSpan();
        for (var q = 0; q < sample_string.Length; ++q) if (MemoryExtensions.Contains(invalidcharsspan, sp[q])) nremove++;
        return string.Create(sample_string.Length - nremove, sample_string, static (span, str) =>
        {
            Span<char> invalidcharsspan2 = invalidchars.AsSpan();
            int pos = 0;
            foreach (char c in str)
            {
                //if (c != '/')
                if (MemoryExtensions.Contains(invalidcharsspan2, c) == false)
                {
                    span[pos++] = c;
                }
            }
        });
    }

    [Benchmark]
    public string remove_loop()
    {
        Span<char> invalidcharsspan = invalidchars.AsSpan();
        char[] buffer = new char[sample_string.Length];
        int pos = 0;
        for (int i = 0; i < sample_string.Length; i++)
        {
            char c = sample_string[i];
            //if (c != '/')
            if (MemoryExtensions.Contains(invalidcharsspan, c) == false)
            {
                buffer[pos++] = c;
            }
        }
        return new string(buffer, 0, pos);
    }

    [Benchmark]
    public string remove_regex_sg() => RemoveCharRx().Replace(sample_string, string.Empty);

    static readonly char[] invalidchars = Path.GetInvalidFileNameChars();
    static readonly Regex invalidcharsrx = new Regex($"[{Regex.Escape(new string(invalidchars))}]", RegexOptions.Compiled);

    [Benchmark]
    public string remove_regex() => invalidcharsrx.Replace(sample_string, string.Empty);

    [Benchmark]
    public string remove_split_join()
    {
        var parts = sample_string.Split(invalidchars, StringSplitOptions.RemoveEmptyEntries);
        //var parts = sample_string.Split(['/'], StringSplitOptions.RemoveEmptyEntries);
        return string.Join(string.Empty, parts);
    }

    [Benchmark]
    public string remove_unsafe()
    {
        unsafe
        {
            Span<char> invalidcharsspan = invalidchars.AsSpan();
            fixed (char* pSrc = sample_string)
            {
                char* pDst = stackalloc char[sample_string.Length];
                int pos = 0;
                for (int i = 0; i < sample_string.Length; i++)
                {
                    char c = pSrc[i];
                    if (false == MemoryExtensions.Contains(invalidcharsspan, c))
                    //if (c != '/')
                    {
                        pDst[pos++] = c;
                    }
                }
                return new string(pDst, 0, pos);
            }
        }
    }

    //[Benchmark]
    public string remove_aggregate() => sample_string.Aggregate(string.Empty, static (current, c) => invalidchars.Contains(c) ? current : current + c);
    //public string remove_aggregate() => sample_string.Aggregate(string.Empty, static (current, c) => c == '/' ? current : current + c);

    //[Benchmark]
    public string remove_pointer()
    {
        unsafe
        {
            char* pSrc = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalUni(sample_string);
            char* pDst = stackalloc char[sample_string.Length];
            int pos = 0;
            for (int i = 0; i < sample_string.Length; i++)
            {
                char c = pSrc[i];
                if (c != '/')
                {
                    pDst[pos++] = c;
                }
            }
            System.Runtime.InteropServices.Marshal.FreeHGlobal((System.IntPtr)pSrc);
            return new string(pDst, 0, pos);
        }
    }

    [Benchmark]
    public string remove_filter()
    {
        var chars = sample_string.ToCharArray().Where(static c => false == MemoryExtensions.Contains(invalidchars, c)).ToArray();
        //var chars = sample_string.ToCharArray().Where(static c => c != '/').ToArray();
        return new string(chars);
    }

    [Benchmark]
    public string remove_custom_loop()
    {
        int count = 0;
        Span<char> invalidcharsspan = invalidchars.AsSpan();
        foreach (char c in sample_string)
        {
            //if (c != '/')
            if (false == MemoryExtensions.Contains(invalidcharsspan, c))
            {
                count++;
            }
        }
        char[] buffer = new char[count];
        int pos = 0;
        foreach (char c in sample_string)
        {
            //if (c != '/')
            if (false == MemoryExtensions.Contains(invalidcharsspan, c))
            {
                buffer[pos++] = c;
            }
        }
        return new string(buffer);
    }

    [Benchmark]
    public string remove_custom_span()
    {
        Span<char> invalidcharsspan = invalidchars.AsSpan();
        int count = 0;
        foreach (char c in sample_string)
        {
            //if (c != '/')
            if (false == MemoryExtensions.Contains(invalidcharsspan, c))
            {
                count++;
            }
        }
        Span<char> buffer = stackalloc char[count];
        int pos = 0;
        foreach (char c in sample_string)
        {
            //if (c != '/')
            if (false == MemoryExtensions.Contains(invalidcharsspan, c))
            {
                buffer[pos++] = c;
            }
        }
        return new string(buffer);
    }

    //[GeneratedRegex("/", RegexOptions.Compiled)]
    [GeneratedRegex("[\"<>\\|\0\u0001\u0002\u0003\u0004\u0005\u0006\a\b\\t\\n\v\\f\\r\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f:\\*\\?\\\\/]", RegexOptions.Compiled)]
    private static partial Regex RemoveCharRx();
}
/*
| sample 60 /          | Mean      | Error    | StdDev   | Gen0   | Allocated |
|--------------------- |----------:|---------:|---------:|-------:|----------:|
| remove_unsafe        |  54.50 ns | 0.558 ns | 0.466 ns | 0.0229 |     144 B |
| remove_replace       |  55.43 ns | 0.742 ns | 0.694 ns | 0.0229 |     144 B |
| remove_loop          |  82.66 ns | 0.597 ns | 0.530 ns | 0.0459 |     288 B |
| remove_createstring  |  95.62 ns | 0.628 ns | 0.557 ns | 0.0229 |     144 B |
| remove_span          | 118.78 ns | 0.622 ns | 0.519 ns | 0.0229 |     144 B |
| remove_split_join    | 119.32 ns | 0.679 ns | 0.602 ns | 0.0663 |     416 B |
| remove_stringbuilder | 135.61 ns | 0.997 ns | 0.833 ns | 0.0534 |     336 B |
| remove_custom_span   | 154.56 ns | 0.698 ns | 0.653 ns | 0.0229 |     144 B |
| remove_custom_loop   | 158.66 ns | 3.145 ns | 4.089 ns | 0.0458 |     288 B |
| remove_regex         | 174.10 ns | 0.556 ns | 0.520 ns | 0.0229 |     144 B |
| remove_filter        | 270.35 ns | 2.053 ns | 1.920 ns | 0.1249 |     784 B |
| remove_linq          | 276.65 ns | 1.362 ns | 1.274 ns | 0.1097 |     688 B |
| remove_aggregate     | 781.25 ns | 8.200 ns | 7.269 ns | 0.7811 |    4904 B |

| sample 15 /          | Mean      | Error    | StdDev   | Median    | Gen0   | Allocated |
|--------------------- |----------:|---------:|---------:|----------:|-------:|----------:|
| remove_unsafe        |  22.54 ns | 0.150 ns | 0.140 ns |  22.52 ns | 0.0076 |      48 B |
| remove_span          |  30.84 ns | 0.669 ns | 1.080 ns |  31.24 ns | 0.0076 |      48 B |
| remove_createstring  |  31.97 ns | 0.686 ns | 0.642 ns |  31.65 ns | 0.0076 |      48 B |
| remove_loop          |  31.98 ns | 0.160 ns | 0.142 ns |  31.94 ns | 0.0166 |     104 B |
| remove_replace       |  44.69 ns | 0.489 ns | 0.458 ns |  44.49 ns | 0.0076 |      48 B |
| remove_custom_loop   |  45.01 ns | 0.084 ns | 0.070 ns |  45.04 ns | 0.0166 |     104 B |
| remove_custom_span   |  45.28 ns | 0.105 ns | 0.098 ns |  45.31 ns | 0.0076 |      48 B |
| remove_stringbuilder |  47.48 ns | 1.011 ns | 1.603 ns |  46.67 ns | 0.0242 |     152 B |
| remove_split_join    | 107.34 ns | 0.965 ns | 0.806 ns | 107.15 ns | 0.0370 |     232 B |
| remove_linq          | 137.12 ns | 0.476 ns | 0.445 ns | 137.14 ns | 0.0484 |     304 B |
| remove_aggregate     | 146.15 ns | 0.579 ns | 0.513 ns | 146.14 ns | 0.0854 |     536 B |
| remove_filter        | 115.68 ns | 0.698 ns | 0.618 ns | 115.65 ns | 0.0508 |     320 B |
| remove_regex         | 168.08 ns | 3.200 ns | 2.837 ns | 166.60 ns | 0.0076 |      48 B |

| sample 60 multi      | Mean     | Error    | StdDev   | Gen0   | Allocated |
|--------------------- |---------:|---------:|---------:|-------:|----------:|
| remove_unsafe        | 235.9 ns |  2.34 ns |  2.07 ns | 0.0203 |     128 B |
| remove_loop          | 240.0 ns |  3.18 ns |  2.98 ns | 0.0429 |     272 B |
| remove_stringbuilder | 243.6 ns |  4.81 ns |  4.73 ns | 0.0505 |     320 B |
| remove_span          | 255.8 ns |  4.97 ns |  5.32 ns | 0.0200 |     128 B |
| remove_split_join    | 338.4 ns |  2.41 ns |  2.13 ns | 0.0954 |     600 B |
| remove_regex_sg      | 346.8 ns |  1.79 ns |  1.59 ns | 0.0200 |     128 B |
| remove_filter        | 374.9 ns |  2.66 ns |  2.36 ns | 0.1197 |     752 B |
| remove_regex         | 387.0 ns |  2.10 ns |  1.75 ns | 0.0200 |     128 B |
| remove_createstring  | 412.1 ns |  5.06 ns |  4.74 ns | 0.0200 |     128 B |
| remove_custom_loop   | 418.3 ns |  2.21 ns |  2.07 ns | 0.0405 |     256 B |
| remove_custom_span   | 448.2 ns |  2.40 ns |  2.13 ns | 0.0200 |     128 B |
| remove_linq          | 703.5 ns | 10.76 ns | 10.07 ns | 0.1040 |     656 B |
| remove_aggregate     | 998.1 ns |  3.02 ns |  2.52 ns | 0.6504 |    4088 B |

| sample2 multi        | Mean       | Error   | StdDev  | Gen0   | Allocated |
|--------------------- |-----------:|--------:|--------:|-------:|----------:|
| remove_regex         |   135.2 ns | 0.63 ns | 0.56 ns | 0.0241 |     152 B |
| remove_split_join    |   166.6 ns | 1.81 ns | 1.69 ns | 0.0355 |     224 B |
| remove_span          |   283.2 ns | 5.07 ns | 4.74 ns | 0.0238 |     152 B |
| remove_unsafe        |   244.5 ns | 1.45 ns | 1.36 ns | 0.0238 |     152 B |
| remove_loop          |   262.4 ns | 1.21 ns | 1.08 ns | 0.0482 |     304 B |
| remove_stringbuilder |   272.6 ns | 3.68 ns | 3.45 ns | 0.0558 |     352 B |
| remove_filter        |   413.3 ns | 3.53 ns | 3.30 ns | 0.1297 |     816 B |
| remove_custom_loop   |   470.4 ns | 1.94 ns | 1.72 ns | 0.0477 |     304 B |
| remove_custom_span   |   479.0 ns | 2.73 ns | 2.55 ns | 0.0238 |     152 B |
| remove_createstring  |   483.9 ns | 5.84 ns | 5.47 ns | 0.0238 |     152 B |
| remove_linq          |   714.3 ns | 7.26 ns | 6.79 ns | 0.1116 |     704 B |
| remove_aggregate     | 1,164.4 ns | 9.95 ns | 9.31 ns | 0.8736 |    5488 B |

| sample 60 multi cold | Mean      | Error     | StdDev      | Median   | Allocated |
|--------------------- |----------:|----------:|------------:|---------:|----------:|
| remove_regex_sg      |  89.27 us | 259.84 us |   766.15 us | 9.100 us |    2016 B |
| remove_loop          | 116.10 us | 379.96 us | 1,120.31 us | 3.100 us |     672 B |
| remove_stringbuilder | 116.61 us | 380.51 us | 1,121.96 us | 3.500 us |     720 B |
| remove_split_join    | 118.16 us | 382.91 us | 1,129.02 us | 3.500 us |    1000 B |
| remove_unsafe        | 119.39 us | 397.46 us | 1,171.92 us | 1.900 us |     528 B |
| remove_custom_loop   | 120.14 us | 389.80 us | 1,149.32 us | 4.300 us |     656 B |
| remove_custom_span   | 121.72 us | 400.50 us | 1,180.89 us | 2.400 us |     528 B |
| remove_createstring  | 124.35 us | 394.60 us | 1,163.50 us | 4.100 us |     528 B |
| remove_filter        | 125.68 us | 407.75 us | 1,202.27 us | 4.800 us |    1152 B |
| remove_aggregate     | 126.31 us | 402.19 us | 1,185.87 us | 7.400 us |    4488 B |
| remove_span          | 143.15 us | 478.81 us | 1,411.77 us | 1.800 us |     528 B |
| remove_linq          | 145.45 us | 445.28 us | 1,312.93 us | 8.100 us |    1056 B |
| remove_regex         | 146.80 us | 454.05 us | 1,338.76 us | 8.550 us |    2016 B |

| sample2 multi cold   | Mean      | Error     | StdDev      | Median   | Allocated |
|--------------------- |----------:|----------:|------------:|---------:|----------:|
| remove_regex_sg      |  78.71 us | 228.42 us |   673.52 us | 9.350 us |    2040 B |
| remove_unsafe        | 118.32 us | 394.77 us | 1,163.98 us | 1.800 us |     552 B |
| remove_stringbuilder | 119.35 us | 386.75 us | 1,140.35 us | 3.700 us |     752 B |
| remove_split_join    | 120.43 us | 387.99 us | 1,143.99 us | 3.600 us |     624 B |
| remove_custom_loop   | 123.98 us | 399.05 us | 1,176.61 us | 4.600 us |     704 B |
| remove_createstring  | 125.13 us | 400.73 us | 1,181.56 us | 4.000 us |     552 B |
| remove_custom_span   | 127.69 us | 425.00 us | 1,253.11 us | 2.300 us |     552 B |
| remove_aggregate     | 130.89 us | 416.05 us | 1,226.74 us | 7.800 us |    5888 B |
| remove_span          | 133.62 us | 446.37 us | 1,316.13 us | 1.800 us |     552 B |
| remove_linq          | 135.17 us | 428.09 us | 1,262.23 us | 8.300 us |    1104 B |
| remove_loop          | 137.27 us | 452.25 us | 1,333.46 us | 3.500 us |     704 B |
| remove_filter        | 140.83 us | 444.01 us | 1,309.17 us | 6.450 us |    1216 B |
| remove_regex         | 157.34 us | 497.73 us | 1,467.57 us | 7.800 us |    2040 B |
*/
