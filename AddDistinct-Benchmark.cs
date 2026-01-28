using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace test;

[MemoryDiagnoser]
[SimpleJob(BenchmarkDotNet.Engines.RunStrategy.ColdStart)]
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
        foreach (var term in enumterms()) frozen = frozen.Add(term);
        return [.. frozen.Distinct()];
    }

    private IEnumerable<string> enumterms()
    {
        yield return "trinité"; yield return "montagna"; yield return "hotel"; yield return "salle"; yield return "fuochi"; yield return "artificio"; yield return "montagna";
        yield return "hotel"; yield return "comune"; yield return "aymavilles";
        yield return "aymavilles"; yield return "elicottero"; yield return "fuochi";
        yield return "cervo"; yield return "stambecco"; yield return "polenta"; yield return "lard"; yield return "hone"; yield return "guardia"; yield return "montagna";

        yield return "volpe"; yield return "cervo"; yield return "aosta"; yield return "sarre"; yield return "macchina"; yield return "fotografia"; yield return "video";
        yield return "volpe"; yield return "brasato"; yield return "costa"; yield return "collina"; yield return "sole"; yield return "mare"; yield return "video";
        yield return "quindici"; yield return "gennaio"; yield return "inverno"; yield return "estate"; yield return "primavera"; yield return "mare";

        yield return "volpe1"; yield return "cervo1"; yield return "aosta1"; yield return "sarre1"; yield return "macchina1"; yield return "fotografia1"; yield return "video";
        yield return "volpe1"; yield return "brasato1"; yield return "costa1"; yield return "collina1"; yield return "sole1"; yield return "mare1"; yield return "video";
        yield return "quindici1"; yield return "gennaio1"; yield return "inverno1"; yield return "estate1"; yield return "primavera1"; yield return "mare";

        yield return "1volpe"; yield return "1cervo"; yield return "1aosta"; yield return "1sarre"; yield return "1macchina"; yield return "1fotografia"; yield return "video";
        yield return "1volpe"; yield return "1brasato"; yield return "1costa"; yield return "1collina"; yield return "1sole"; yield return "1mare"; yield return "video";
        yield return "1quindici"; yield return "gennaio"; yield return "1inverno"; yield return "1estate"; yield return "primavera"; yield return "mare";
    }
}
/*
| 10 items            | Mean       | Error    | StdDev   | Median     | Gen0   | Allocated |
|-------------------- |-----------:|---------:|---------:|-----------:|-------:|----------:|
| linq_distinct       |   349.0 ns |  3.99 ns |  3.73 ns |   350.2 ns | 0.1502 |     944 B |
| sorted_set_distinct | 1,344.9 ns | 26.56 ns | 42.89 ns | 1,321.6 ns | 0.1755 |    1104 B |
| builder_distinct    | 1,611.9 ns | 15.54 ns | 14.54 ns | 1,611.9 ns | 0.1621 |    1024 B |
| frozen_distinct     | 1,829.5 ns | 12.70 ns | 11.88 ns | 1,827.5 ns | 0.3242 |    2040 B |
| order_distinct      | 1,957.5 ns | 15.77 ns | 14.75 ns | 1,958.8 ns | 0.2480 |    1576 B |
| sort_distinct       | 2,017.4 ns | 27.29 ns | 25.53 ns | 2,020.4 ns | 0.1411 |     888 B |
| immutable_distinct  | 2,732.3 ns | 30.06 ns | 25.10 ns | 2,731.2 ns | 0.2136 |    1352 B |

| 10 items cold start | Mean     | Error     | StdDev    | Median    | Allocated |
|-------------------- |---------:|----------:|----------:|----------:|----------:|
| linq_distinct       | 13.71 us |  24.79 us |  73.11 us |  5.800 us |   1.31 KB |
| order_distinct      | 22.03 us |  29.98 us |  88.39 us | 11.650 us |   1.93 KB |
| sort_distinct       | 22.22 us |  34.12 us | 100.59 us | 11.200 us |   1.26 KB |
| sorted_set_distinct | 28.07 us |  50.90 us | 150.09 us | 11.500 us |   1.47 KB |
| frozen_distinct     | 43.16 us |  97.64 us | 287.89 us | 12.700 us |   2.38 KB |
| builder_distinct    | 46.66 us | 104.68 us | 308.66 us | 12.400 us |   1.39 KB |
| immutable_distinct  | 54.19 us | 116.53 us | 343.60 us | 16.900 us |   1.71 KB |

                REDO oopsie

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
| linq_distinct       |  24.10 us |  35.47 us | 104.58 us |  9.600 us |   4.29 KB |
| order_distinct      |  67.94 us |  41.51 us | 122.41 us | 51.650 us |      7 KB |
| sorted_set_distinct |  71.64 us |  66.71 us | 196.69 us | 49.800 us |   5.57 KB |
| sort_distinct       |  86.96 us | 102.70 us | 302.81 us | 51.400 us |   4.74 KB |
| immutable_distinct  | 103.51 us | 121.31 us | 357.68 us | 62.900 us |    7.6 KB |
| builder_distinct    | 104.24 us | 116.00 us | 342.04 us | 69.450 us |   5.46 KB |
| frozen_distinct     | 120.91 us | 109.30 us | 322.28 us | 77.000 us |  20.67 KB |
*/
