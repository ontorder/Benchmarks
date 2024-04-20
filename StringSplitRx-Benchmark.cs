using BenchmarkDotNet.Attributes;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace test;

[MemoryDiagnoser]
public partial class bench_split_rx
{
    static public readonly string Data = """
        valor1 = mucho mucho texto
        valor2 := esempio
        valor3 == tiesto {dinamico + 1}

        valor4 = un name
        valor5 = descrizione
        """;

    private static readonly Regex _rxSplit = new(@"^(.*)$", RegexOptions.Compiled | RegexOptions.Multiline);

    [GeneratedRegex(@"^(.*)$", RegexOptions.Multiline)]
    private static partial Regex GeneratedRxSplit();

    [Benchmark]
    public string string_split()
        => Data.Split('\n').Last();

    [Benchmark]
    public string rx_split()
        => _rxSplit.Split(Data)[^1];

    [Benchmark]
    public string rx_gen()
    {
        var last = GeneratedRxSplit().Matches(Data)[^1];
        return last.Captures[0].Value;
    }

    [Benchmark]
    public string rx_matches()
    {
        var matches = _rxSplit.Matches(Data);
        return matches[^1].Captures[0].Value;
    }

    [Benchmark]
    public string rx_enum()
    {
        var strspan = Data.AsSpan();
        var enu = _rxSplit.EnumerateMatches(strspan);
        ReadOnlySpan<char> ret = default;
        while (enu.MoveNext()) ret = strspan.Slice(enu.Current.Index, enu.Current.Length);
        return new string(ret);
    }
}

/*
| Method       | Mean     | Error   | StdDev  | Gen0   | Gen1   | Allocated |
|------------- |---------:|--------:|--------:|-------:|-------:|----------:|
| string_split | 126.2 ns | 1.04 ns | 0.92 ns | 0.0663 |      - |     416 B |
| rx_split     | 644.7 ns | 5.28 ns | 4.41 ns | 0.2384 |      - |    1496 B |
| rx_gen       | 585.7 ns | 8.68 ns | 8.12 ns | 0.2899 | 0.0019 |    1824 B |
| rx_matches   | 607.9 ns | 1.60 ns | 1.33 ns | 0.2899 | 0.0019 |    1824 B |
| rx_enum      | 330.3 ns | 4.31 ns | 4.03 ns | 0.0100 |      - |      64 B |
*/
