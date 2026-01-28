using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace test;

[MemoryDiagnoser]
//[SimpleJob(BenchmarkDotNet.Engines.RunStrategy.ColdStart)]
public class bench_distinct
{
    [Benchmark]
    public string[] sorted_set_distinct()
    {
        var ss = new SortedSet<string>();
        foreach (var term in enumterms()) ss.Add(term);
        return [.. ss.Distinct()];
    }

    [Benchmark]
    public string[] linq_distinct()
        => [.. enumterms().Distinct()];

    [Benchmark]
    public string[] immutable_distinct()
        => [.. enumterms().ToImmutableSortedSet().Distinct()];

    [Benchmark]
    public string[] sort_distinct()
    {
        var items = enumterms().ToArray();
        Array.Sort(items);
        return [.. items.Distinct()];
    }

    [Benchmark]
    public string[] order_distinct()
        => [.. enumterms().Order().Distinct()];

    [Benchmark]
    public string[] builder_distinct()
    {
        var imm = ImmutableSortedSet.CreateBuilder<string>();
        foreach (var term in enumterms()) imm.Add(term);
        return [.. imm.Distinct()];
    }

    [Benchmark]
    public string[] frozen_distinct()
    {
        var frozen = ImmutableSortedSet.Create<string>();
        foreach (var term in enumterms()) frozen.Add(term);
        return [.. frozen.Distinct()];
    }

    private IEnumerable<string> enumterms()
    {
        yield return "trinité"; yield return "montagna"; yield return "hotel"; yield return "salle"; yield return "fuochi"; yield return "artificio"; yield return "montagna";
        yield return "hotel"; yield return "comune"; yield return "aymavilles";
        //yield return "aymavilles"; yield return "elicottero"; yield return "fuochi";
        //yield return "cervo"; yield return "stambecco"; yield return "polenta"; yield return "lard"; yield return "hone"; yield return "guardia"; yield return "montagna";

        //yield return "volpe"; yield return "cervo"; yield return "aosta"; yield return "sarre"; yield return "macchina"; yield return "fotografia"; yield return "video";
        //yield return "volpe"; yield return "brasato"; yield return "costa"; yield return "collina"; yield return "sole"; yield return "mare"; yield return "video";
        //yield return "quindici"; yield return "gennaio"; yield return "inverno"; yield return "estate"; yield return "primavera"; yield return "mare";

        //yield return "volpe1"; yield return "cervo1"; yield return "aosta1"; yield return "sarre1"; yield return "macchina1"; yield return "fotografia1"; yield return "video";
        //yield return "volpe1"; yield return "brasato1"; yield return "costa1"; yield return "collina1"; yield return "sole1"; yield return "mare1"; yield return "video";
        //yield return "quindici1"; yield return "gennaio1"; yield return "inverno1"; yield return "estate1"; yield return "primavera1"; yield return "mare";

        //yield return "1volpe"; yield return "1cervo"; yield return "1aosta"; yield return "1sarre"; yield return "1macchina"; yield return "1fotografia"; yield return "video";
        //yield return "1volpe"; yield return "1brasato"; yield return "1costa"; yield return "1collina"; yield return "1sole"; yield return "1mare"; yield return "video";
        //yield return "1quindici"; yield return "gennaio"; yield return "1inverno"; yield return "1estate"; yield return "primavera"; yield return "mare";
    }
}
/*
| 10 items            | Mean       | Error    | StdDev   | Gen0   | Allocated |
|-------------------- |-----------:|---------:|---------:|-------:|----------:|
| frozen_distinct     |   341.1 ns |  6.85 ns | 12.52 ns | 0.1564 |     984 B |
| linq_distinct       |   355.9 ns |  2.47 ns |  2.31 ns | 0.1502 |     944 B |
| sorted_set_distinct | 1,358.8 ns | 27.23 ns | 35.40 ns | 0.1755 |    1104 B |
| builder_distinct    | 1,587.6 ns | 17.06 ns | 14.25 ns | 0.1621 |    1024 B |
| order_distinct      | 1,949.4 ns | 15.48 ns | 14.48 ns | 0.2480 |    1576 B |
| sort_distinct       | 1,974.3 ns | 21.75 ns | 20.34 ns | 0.1411 |     888 B |
| immutable_distinct  | 2,723.3 ns | 26.71 ns | 24.98 ns | 0.2136 |    1352 B |

| 10 items cold start | Mean     | Error     | StdDev    | Median    | Allocated |
|-------------------- |---------:|----------:|----------:|----------:|----------:|
| linq_distinct       | 16.37 us |  25.37 us |  74.80 us |  7.700 us |   1.31 KB |
| order_distinct      | 24.23 us |  31.76 us |  93.64 us | 12.800 us |   1.93 KB |
| sort_distinct       | 27.86 us |  37.13 us | 109.46 us | 12.100 us |   1.26 KB |
| frozen_distinct     | 32.07 us |  76.64 us | 225.98 us |  6.550 us |   1.35 KB |
| sorted_set_distinct | 34.34 us |  61.29 us | 180.72 us | 13.700 us |   1.47 KB |
| immutable_distinct  | 62.24 us | 111.65 us | 329.20 us | 22.200 us |   1.71 KB |
| builder_distinct    | 67.21 us | 167.28 us | 493.22 us | 14.900 us |   1.39 KB |

| 20 items            | Mean       | Error    | StdDev   | Gen0   | Allocated |
|-------------------- |-----------:|---------:|---------:|-------:|----------:|
| linq_distinct       |   531.1 ns |  4.31 ns |  4.03 ns | 0.1593 |    1000 B |
| sorted_set_distinct | 3,001.4 ns | 53.26 ns | 49.82 ns | 0.2594 |    1632 B |
| sort_distinct       | 4,145.6 ns | 75.13 ns | 70.28 ns | 0.2289 |    1472 B |
| immutable_distinct  | 5,171.9 ns | 35.73 ns | 33.42 ns | 0.3357 |    2144 B |

| 40 items            | Mean        | Error     | StdDev    | Gen0   | Allocated |
|-------------------- |------------:|----------:|----------:|-------:|----------:|
| linq_distinct       |    958.5 ns |   6.38 ns |   5.97 ns | 0.3052 |   1.88 KB |
| sorted_set_distinct |  7,820.8 ns | 154.63 ns | 165.45 ns | 0.4578 |   2.88 KB |
| order_distinct      |  9,777.3 ns |  45.94 ns |  35.86 ns | 0.5493 |   3.41 KB |
| sort_distinct       | 10,390.0 ns |  59.61 ns |  55.76 ns | 0.3967 |   2.46 KB |
| immutable_distinct  | 13,203.3 ns | 115.65 ns | 102.53 ns | 0.6256 |   3.88 KB |

| 80 items            | Mean      | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|-------------------- |----------:|----------:|----------:|-------:|-------:|----------:|
| frozen_distinct     |  1.702 us | 0.0135 us | 0.0105 us | 1.0490 |      - |   6.43 KB |
| linq_distinct       |  1.851 us | 0.0151 us | 0.0142 us | 0.6351 | 0.0038 |    3.9 KB |
| sorted_set_distinct | 20.484 us | 0.1463 us | 0.1369 us | 0.8240 |      - |   5.18 KB |
| order_distinct      | 21.589 us | 0.2078 us | 0.1842 us | 1.0681 |      - |   6.61 KB |
| builder_distinct    | 22.084 us | 0.1964 us | 0.1741 us | 0.8240 |      - |   5.07 KB |
| sort_distinct       | 22.515 us | 0.4431 us | 0.6211 us | 0.7019 |      - |   4.35 KB |
| immutable_distinct  | 27.827 us | 0.4503 us | 0.5695 us | 1.1597 |      - |   7.21 KB |

| 80 items cold start | Mean      | Error     | StdDev    | Median    | Allocated |
|-------------------- |----------:|----------:|----------:|----------:|----------:|
| linq_distinct       |  21.48 us |  36.16 us | 106.61 us |  9.800 us |   4.29 KB |
| frozen_distinct     |  44.53 us | 101.01 us | 297.83 us | 13.200 us |   6.82 KB |
| order_distinct      |  54.17 us |  44.92 us | 132.44 us | 39.000 us |      7 KB |
| sort_distinct       |  57.03 us |  46.59 us | 137.38 us | 41.100 us |   4.74 KB |
| sorted_set_distinct |  76.05 us |  89.65 us | 264.34 us | 45.900 us |   5.57 KB |
| builder_distinct    | 113.96 us | 162.60 us | 479.44 us | 57.450 us |   5.46 KB |
| immutable_distinct  | 118.52 us | 185.18 us | 546.00 us | 58.050 us |    7.6 KB |
*/
