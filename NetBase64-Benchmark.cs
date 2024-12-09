using BenchmarkDotNet.Attributes;
using System;
using System.Linq;
using System.Text;

namespace test;

[MemoryDiagnoser]
public class bench_20241209
{
    private readonly string[] strings = ["eUHIduY", "SkpaYFA", "+SHQDMp", "bOSbyjb", "8AM/m3f"];

    //[Benchmark]
    public bool JustReplaceTest()
    {
        try
        {
            foreach (var b64 in strings) _ = JustReplace(b64);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string JustReplace(string b64)
        => b64
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", string.Empty);

    //[Benchmark]
    public bool LinqStringbuilderTest()
    {
        try
        {
            foreach (var b64 in strings) _ = LinqStringbuilder(b64);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string LinqStringbuilder(string b64)
        => b64
            .Select(c => c switch
            {
                '+' => '-',
                '/' => '_',
                //'=' => '\0',
                _ => c
            })
            .Aggregate(new StringBuilder(), (sb, c) => sb.Append(c))
            .ToString();

    [Benchmark]
    public bool ForSpanTest()
    {
        try
        {
            foreach (var b64 in strings) _ = ForSpan(b64);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string ForSpan(string b64)
    {
        Span<char> done = stackalloc char[b64.Length];
        var l = b64.Length;
        for (int id = 0; id < l; ++id)
        {
            var c = b64[id];
            done[id] = c switch
            {
                '+' => '-',
                '/' => '_',
                //'=' => continue,
                _ => c
            };
        }
        return new(done);
    }

    private string ForSlice(string b64)
    {
        var span = b64.AsSpan();
        Span<char> done = stackalloc char[b64.Length];
        for (int id = 0; id < span.Length; ++id)
        {
            var c = span.Slice(id, 1)[0];
            done[id] = c switch
            {
                '+' => '-',
                '/' => '_',
                //'=' => continue,
                _ => c
            };
        }
        return new string(done);
    }

    //[Benchmark]
    public bool ForStringTest()
    {
        try
        {
            foreach (var b64 in strings) _ = ForString(b64);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string ForString(string b64)
    {
        var done = new char[b64.Length];
        for (int id = 0; id < b64.Length; ++id)
        {
            done[id] = b64[id] switch
            {
                '+' => '-',
                '/' => '_',
                //'=' => continue,
                _ => b64[id]
            };
        }
        return new string(done);
    }

    [Benchmark]
    public bool ForSliceTest()
    {
        try
        {
            foreach (var b64 in strings) _ = ForSlice(b64);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
/*

| Method                | Mean     | Error   | StdDev  | Gen0   | Allocated |
|---------------------- |---------:|--------:|--------:|-------:|----------:|
| ForSpanTest           | 78.51 ns | 0.80 ns | 0.75 ns | 0.0318 |     200 B |
| ForSliceTest          | 82.14 ns | 0.73 ns | 0.65 ns | 0.0318 |     200 B |
| ForStringTest         | 103.5 ns | 0.71 ns | 0.59 ns | 0.0637 |     400 B |
| JustReplaceTest       | 131.0 ns | 0.82 ns | 0.77 ns | 0.0126 |      80 B |
| LinqStringbuilderTest | 424.9 ns | 2.36 ns | 1.97 ns | 0.1845 |    1160 B |

 */
