using BenchmarkDotNet.Attributes;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace test;

[MemoryDiagnoser]
[SimpleJob(BenchmarkDotNet.Engines.RunStrategy.ColdStart)]
public partial class bench_stringreplace
{
    public const string sample_string = "cat1/cat2/cat3 22221133 CERVINIA_DAY_1 *rec* *bt123* <anita>";
    public const string sample_string2 = "20250203_RAVA_CERVINIA_DAY 1_REDKX_REC 709 4K_A011_A087_020232*";

    [Benchmark]
    public string replace_span()
    {
        Span<char> buffer = stackalloc char[sample_string.Length];
        int pos = 0;
        Span<char> invalidcharsspan = invalidchars.AsSpan();
        foreach (ref readonly var c in sample_string.AsSpan())
        {
            buffer[pos++] = MemoryExtensions.Contains(invalidcharsspan, c)
                ? '_'
                : c;
        }
        return new string(buffer);
    }

    [Benchmark]
    public string replace_stringbuilder()
    {
        Span<char> invalidcharsspan = invalidchars.AsSpan();
        var sb = new System.Text.StringBuilder(sample_string.Length);
        foreach (char c in sample_string.AsSpan()) sb.Append(MemoryExtensions.Contains(invalidcharsspan, c) ? '_' : c);
        return sb.ToString();
    }

    [Benchmark]
    public string replace_stringbuilderref()
    {
        Span<char> invalidcharsspan = invalidchars.AsSpan();
        var sb = new System.Text.StringBuilder(sample_string.Length);
        foreach (ref readonly char c in sample_string.AsSpan()) sb.Append(MemoryExtensions.Contains(invalidcharsspan, c) ? '_' : c);
        return sb.ToString();
    }

    [Benchmark]
    public string replace_linq()
    {
        var chars = sample_string.Select(c => MemoryExtensions.Contains(invalidchars, c) ? '_' : c).ToArray();
        return new string(chars);
    }

    [Benchmark]
    public string replace_createstring()
    {
        var nremove = 0;
        Span<char> invalidcharsspan = invalidchars.AsSpan();
        var sp = sample_string.AsSpan();
        return string.Create(sample_string.Length, sample_string, static (span, str) =>
        {
            Span<char> invalidcharsspan2 = invalidchars.AsSpan();
            int pos = 0;
            foreach (char c in str.AsSpan()) span[pos++] = MemoryExtensions.Contains(invalidcharsspan2, c) ? '_' : c;
        });
    }

    [Benchmark]
    public string replace_loop()
    {
        Span<char> invalidcharsspan = invalidchars.AsSpan();
        char[] buffer = new char[sample_string.Length];
        var bufspan = buffer.AsSpan();
        var strspan = sample_string.AsSpan();
        int pos = 0;
        for (int i = 0; i < sample_string.Length; i++)
        {
            ref readonly char c = ref strspan[i];
            bufspan[pos++] = MemoryExtensions.Contains(invalidcharsspan, c) ? '_' : c;
        }
        return new string(bufspan);
    }

    [Benchmark]
    public string replace_regex_sg() => RemoveCharRx().Replace(sample_string, "_");

    static readonly char[] invalidchars = Path.GetInvalidFileNameChars();
    static readonly Regex invalidcharsrx = new Regex($"[{Regex.Escape(new string(invalidchars))}]", RegexOptions.Compiled);

    [Benchmark]
    public string replace_regex() => invalidcharsrx.Replace(sample_string, "_");

    [Benchmark]
    public string replace_split_join()
    {
        var parts = sample_string.Split(invalidchars);
        return string.Join('_', parts);
    }

    [Benchmark]
    public string replace_unsafe()
    {
        unsafe
        {
            fixed (char* pSrc = sample_string)
            {
                Span<char> invalidcharsspan = invalidchars.AsSpan();
                char* pDst = stackalloc char[sample_string.Length];
                int pos = 0;
                for (int i = 0; i < sample_string.Length; i++)
                {
                    ref char c = ref pSrc[i];
                    pDst[pos++] = MemoryExtensions.Contains(invalidcharsspan, c) ? '_' : c;
                }
                return new string(pDst);
            }
        }
    }

    [Benchmark]
    public string replace_filter()
    {
        var chars = sample_string.ToCharArray().Select(static c => MemoryExtensions.Contains(invalidchars, c) ? '_' : c).ToArray();
        return new string(chars);
    }

    //[GeneratedRegex("/", RegexOptions.Compiled)]
    [GeneratedRegex("[\"<>\\|\0\u0001\u0002\u0003\u0004\u0005\u0006\a\b\\t\\n\v\\f\\r\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f:\\*\\?\\\\/]", RegexOptions.Compiled)]
    private static partial Regex RemoveCharRx();
}

/*
| sample1                  | Mean     | Error   | StdDev  | Gen0   | Allocated |
|------------------------- |---------:|--------:|--------:|-------:|----------:|
| replace_createstring     | 230.9 ns | 4.48 ns | 3.97 ns | 0.0229 |     144 B |
| replace_loop             | 247.0 ns | 3.72 ns | 3.30 ns | 0.0458 |     288 B |
| replace_stringbuilder    | 249.4 ns | 4.99 ns | 6.12 ns | 0.0534 |     336 B |
| replace_stringbuilderref | 253.3 ns | 3.58 ns | 3.35 ns | 0.0534 |     336 B |
| replace_unsafe           | 264.2 ns | 1.57 ns | 1.47 ns | 0.0229 |     144 B |
| replace_split_join       | 271.9 ns | 2.86 ns | 2.39 ns | 0.0839 |     528 B |
| replace_filter           | 273.0 ns | 1.39 ns | 1.09 ns | 0.0763 |     480 B |
| replace_span             | 311.5 ns | 3.69 ns | 3.27 ns | 0.0229 |     144 B |
| replace_regex_sg         | 340.2 ns | 4.87 ns | 4.56 ns | 0.0229 |     144 B |
| replace_regex            | 382.2 ns | 7.56 ns | 7.07 ns | 0.0229 |     144 B |
| replace_linq             | 539.8 ns | 5.06 ns | 4.23 ns | 0.1097 |     688 B |

| sample2                  | Mean     | Error   | StdDev  | Gen0   | Allocated |
|------------------------- |---------:|--------:|--------:|-------:|----------:|
| replace_regex_sg         | 127.3 ns | 0.99 ns | 0.92 ns | 0.0241 |     152 B |
| replace_regex            | 132.7 ns | 1.17 ns | 1.10 ns | 0.0241 |     152 B |
| replace_split_join       | 171.0 ns | 2.90 ns | 2.71 ns | 0.0548 |     344 B |
| replace_createstring     | 253.5 ns | 2.86 ns | 2.53 ns | 0.0238 |     152 B |
| replace_loop             | 270.5 ns | 1.94 ns | 1.82 ns | 0.0482 |     304 B |
| replace_unsafe           | 278.1 ns | 2.48 ns | 2.20 ns | 0.0238 |     152 B |
| replace_stringbuilder    | 282.0 ns | 4.77 ns | 4.46 ns | 0.0558 |     352 B |
| replace_filter           | 293.6 ns | 1.12 ns | 0.99 ns | 0.0801 |     504 B |
| replace_stringbuilderref | 293.9 ns | 4.55 ns | 4.03 ns | 0.0558 |     352 B |
| replace_span             | 333.4 ns | 4.69 ns | 4.39 ns | 0.0238 |     152 B |
| replace_linq             | 550.2 ns | 6.34 ns | 5.62 ns | 0.1116 |     704 B |

| sample1 cold             | Mean      | Error     | StdDev      | Median   | Allocated |
|------------------------- |----------:|----------:|------------:|---------:|----------:|
| replace_regex_sg         |  86.63 us | 252.40 us |   744.21 us | 9.550 us |    2176 B |
| replace_loop             | 120.30 us | 397.02 us | 1,170.64 us | 3.100 us |     688 B |
| replace_unsafe           | 122.60 us | 408.17 us | 1,203.49 us | 2.100 us |     544 B |
| replace_createstring     | 124.29 us | 411.17 us | 1,212.35 us | 2.900 us |     544 B |
| replace_split_join       | 125.20 us | 412.83 us | 1,217.23 us | 2.800 us |     928 B |
| replace_stringbuilderref | 125.85 us | 414.31 us | 1,221.60 us | 3.400 us |     736 B |
| replace_stringbuilder    | 130.39 us | 422.37 us | 1,245.37 us | 3.500 us |     736 B |
| replace_filter           | 133.23 us | 431.22 us | 1,271.45 us | 5.500 us |     880 B |
| replace_linq             | 147.88 us | 479.56 us | 1,414.00 us | 5.800 us |    1088 B |
| replace_span             | 149.76 us | 499.59 us | 1,473.06 us | 2.200 us |     544 B |
| replace_regex            | 152.67 us | 476.30 us | 1,404.37 us | 9.750 us |    2176 B |

| sample2 cold             | Mean      | Error     | StdDev      | Median    | Allocated |
|------------------------- |----------:|----------:|------------:|----------:|----------:|
| replace_regex_sg         |  93.25 us | 275.95 us |   813.64 us | 10.250 us |    2184 B |
| replace_split_join       | 119.20 us | 394.68 us | 1,163.74 us |  2.600 us |     744 B |
| replace_stringbuilderref | 121.52 us | 400.30 us | 1,180.30 us |  3.300 us |     752 B |
| replace_createstring     | 123.40 us | 407.74 us | 1,202.22 us |  3.000 us |     552 B |
| replace_stringbuilder    | 124.93 us | 409.02 us | 1,206.00 us |  3.200 us |     752 B |
| replace_loop             | 128.79 us | 420.72 us | 1,240.50 us |  3.500 us |     704 B |
| replace_filter           | 135.70 us | 441.31 us | 1,301.20 us |  5.300 us |     904 B |
| replace_regex            | 156.51 us | 488.69 us | 1,440.90 us |  9.900 us |    2184 B |
| replace_span             | 150.13 us | 500.89 us | 1,476.90 us |  2.100 us |     552 B |
| replace_linq             | 150.41 us | 484.81 us | 1,429.47 us |  6.600 us |    1104 B |
| replace_unsafe           | 224.50 us | 746.53 us | 2,201.16 us |  2.400 us |     552 B |
*/
