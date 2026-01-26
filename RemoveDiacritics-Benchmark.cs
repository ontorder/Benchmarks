using BenchmarkDotNet.Attributes;
using System;
using System.Globalization;
using System.Linq;

namespace test;

[MemoryDiagnoser]
public class bench_strnorm
{
    private readonly string text = "pàrole èsulano da cóntestualizzazioni";

    [Benchmark]
    public string via_stringbuilder()
    {
        var sb = new System.Text.StringBuilder();
        var n = text.Normalize(System.Text.NormalizationForm.FormD);
        foreach (var c in n) if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
        return sb.ToString();
    }

    [Benchmark]
    public string via_sbspan()
    {
        var sb = new System.Text.StringBuilder();
        var n = text.Normalize(System.Text.NormalizationForm.FormD).AsSpan();
        foreach (var c in n) if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
        return sb.ToString();
    }

    //[Benchmark]
    public string via_linq()
        => new string(
            [.. text
                .Normalize(System.Text.NormalizationForm.FormD)
                .Where(static c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)]);
}
/*
| Method            | Mean     | Error   | StdDev  | Gen0   | Allocated |
|------------------ |---------:|--------:|--------:|-------:|----------:|
| via_stringbuilder | 327.7 ns | 2.27 ns | 2.01 ns | 0.0863 |     544 B |
| via_linq          | 446.8 ns | 3.15 ns | 2.95 ns | 0.1121 |     704 B |


| Method            | Mean     | Error   | StdDev  | Gen0   | Allocated |
|------------------ |---------:|--------:|--------:|-------:|----------:|
| via_sbspan        | 329.6 ns | 1.63 ns | 1.52 ns | 0.0863 |     544 B |
| via_stringbuilder | 334.0 ns | 6.63 ns | 6.81 ns | 0.0863 |     544 B |
*/
