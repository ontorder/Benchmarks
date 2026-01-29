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
    public string[] toimmutable_distinct()
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
    public string[] immutable_distinct()
    {
        var imm = ImmutableSortedSet.Create<string>();
        foreach (var term in enumterms()) imm = imm.Add(term);
        return [.. imm.Distinct()];
    }

    [Benchmark]
    public string[] just_hashset()
        => new HashSet<string>(enumterms()).ToArray();

    [Benchmark]
    public string[] hashset_capacity()
    {
        var hs = new HashSet<string>(40);
        foreach (var term in enumterms()) hs.Add(term);
        return [.. hs];
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
| 10 items            | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|-------------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| hashset_capacity    |   255.0 ns |  5.10 ns |  6.63 ns | 0.1874 | 0.0005 |    1176 B |
| just_hashset        |   391.4 ns |  2.52 ns |  2.24 ns | 0.1402 |      - |     880 B |
| linq_distinct       |   394.3 ns |  6.65 ns |  5.90 ns | 0.1502 |      - |     944 B |
| sorted_set_distinct | 1,445.1 ns | 13.65 ns | 11.40 ns | 0.1755 |      - |    1104 B |
| builder_distinct    | 1,759.2 ns | 19.96 ns | 17.69 ns | 0.1621 |      - |    1024 B |
| immutable_distinct  | 2,024.0 ns | 22.19 ns | 18.53 ns | 0.3242 |      - |    2040 B |
| sort_distinct       | 2,112.5 ns | 37.61 ns | 53.94 ns | 0.1411 |      - |     888 B |
| order_distinct      | 2,144.3 ns | 41.70 ns | 46.35 ns | 0.2480 |      - |    1576 B |
| toimmutable_distinct| 2,897.7 ns | 33.31 ns | 27.81 ns | 0.2136 |      - |    1352 B |

| 10 items cold start | Mean     | Error     | StdDev    | Median    | Allocated |
|-------------------- |---------:|----------:|----------:|----------:|----------:|
| linq_distinct       | 13.80 us |  23.02 us |  67.88 us |  5.700 us |   1.31 KB |
| just_hashset        | 15.56 us |  23.59 us |  69.56 us |  5.100 us |   1.25 KB |
| hashset_capacity    | 19.40 us |  47.94 us | 141.36 us |  4.400 us |   1.54 KB |
| order_distinct      | 22.42 us |  30.79 us |  90.79 us | 12.100 us |   1.93 KB |
| sort_distinct       | 23.07 us |  32.94 us |  97.13 us | 11.650 us |   1.26 KB |
| sorted_set_distinct | 36.86 us |  53.63 us | 158.12 us | 13.900 us |   1.47 KB |
| immutable_distinct  | 47.47 us | 111.68 us | 329.29 us | 13.050 us |   2.38 KB |
| builder_distinct    | 50.44 us | 114.09 us | 336.39 us | 12.000 us |   1.39 KB |
| toimmutable_distinct| 55.45 us | 114.74 us | 338.33 us | 18.300 us |   1.71 KB |

| 20 items            | Mean       | Error    | StdDev   | Gen0   | Allocated |
|-------------------- |-----------:|---------:|---------:|-------:|----------:|
| linq_distinct       |   592.0 ns |  3.32 ns |  2.94 ns | 0.1593 |    1000 B |
| sorted_set_distinct | 3,163.7 ns | 15.79 ns | 14.77 ns | 0.2594 |    1632 B |
| builder_distinct    | 3,906.0 ns | 30.32 ns | 23.67 ns | 0.2441 |    1536 B |
| sort_distinct       | 4,205.9 ns | 15.21 ns | 13.49 ns | 0.2289 |    1472 B |
| immutable_distinct  | 4,409.4 ns | 16.25 ns | 15.20 ns | 0.6409 |    4024 B |
| order_distinct      | 4,506.8 ns | 15.11 ns | 14.14 ns | 0.3052 |    1952 B |
| toimmutable_distinct| 5,603.9 ns | 26.09 ns | 23.13 ns | 0.3357 |    2144 B |

| 20 items cold start | Mean     | Error     | StdDev    | Median    | Allocated |
|-------------------- |---------:|----------:|----------:|----------:|----------:|
| linq_distinct       | 15.80 us |  25.45 us |  75.05 us |  6.600 us |   1.37 KB |
| sort_distinct       | 28.52 us |  35.60 us | 104.96 us | 13.750 us |   1.83 KB |
| order_distinct      | 33.45 us |  32.43 us |  95.61 us | 15.350 us |    2.3 KB |
| sorted_set_distinct | 38.96 us |  62.08 us | 183.04 us | 14.300 us |   1.98 KB |
| builder_distinct    | 54.95 us | 108.43 us | 319.70 us | 17.050 us |   1.89 KB |
| immutable_distinct  | 60.68 us | 104.66 us | 308.58 us | 25.650 us |   4.32 KB |
| toimmutable_distinct| 67.53 us | 143.03 us | 421.73 us | 20.850 us |   2.48 KB |

| 40 items            | Mean      | Error     | StdDev    | Gen0   | Allocated |
|-------------------- |----------:|----------:|----------:|-------:|----------:|
| linq_distinct       |  1.091 us | 0.0114 us | 0.0107 us | 0.3052 |   1.88 KB |
| sorted_set_distinct |  8.475 us | 0.1140 us | 0.1066 us | 0.4578 |   2.88 KB |
| builder_distinct    |  9.714 us | 0.0448 us | 0.0397 us | 0.4425 |   2.77 KB |
| order_distinct      | 10.555 us | 0.0359 us | 0.0318 us | 0.5493 |   3.41 KB |
| sort_distinct       | 11.166 us | 0.0258 us | 0.0215 us | 0.3967 |   2.46 KB |
| immutable_distinct  | 11.501 us | 0.1870 us | 0.2154 us | 1.4801 |   9.07 KB |
| toimmutable_distinct| 14.636 us | 0.0707 us | 0.0590 us | 0.6256 |   3.88 KB |

| 80 items            | Mean      | Error     | StdDev    | Median    | Gen0   | Gen1   | Allocated |
|-------------------- |----------:|----------:|----------:|----------:|-------:|-------:|----------:|
| hashset_capacity    |  1.940 us | 0.0381 us | 0.0337 us |  1.923 us | 0.6027 | 0.0038 |    3.7 KB |
| linq_distinct       |  2.098 us | 0.0205 us | 0.0182 us |  2.095 us | 0.6332 | 0.0038 |    3.9 KB |
| just_hashset        |  2.246 us | 0.0420 us | 0.0820 us |  2.207 us | 0.6256 | 0.0038 |   3.84 KB |
| sorted_set_distinct | 21.986 us | 0.2468 us | 0.2188 us | 21.984 us | 0.8240 |      - |   5.18 KB |
| order_distinct      | 23.442 us | 0.3090 us | 0.2890 us | 23.359 us | 1.0681 |      - |   6.61 KB |
| sort_distinct       | 24.595 us | 0.4820 us | 0.4273 us | 24.507 us | 0.7019 |      - |   4.35 KB |
| builder_distinct    | 24.815 us | 0.2183 us | 0.1823 us | 24.775 us | 0.8240 |      - |   5.07 KB |
| immutable_distinct  | 29.560 us | 0.3381 us | 0.2997 us | 29.546 us | 3.2959 |      - |  20.28 KB |
| toimmutable_distinct| 30.189 us | 0.4020 us | 0.4128 us | 30.146 us | 1.1597 |      - |   7.21 KB |

| 80 items cold start | Mean      | Error     | StdDev    | Median    | Allocated |
|-------------------- |----------:|----------:|----------:|----------:|----------:|
| just_hashset        |  21.22 us |  36.29 us | 107.00 us |  9.100 us |   4.23 KB |
| hashset_capacity    |  22.66 us |  39.29 us | 115.83 us | 10.100 us |   4.09 KB |
| linq_distinct       |  26.02 us |  38.09 us | 112.31 us | 10.500 us |   4.29 KB |
| sort_distinct       |  59.72 us |  46.98 us | 138.52 us | 40.700 us |   4.74 KB |
| order_distinct      |  64.44 us |  76.79 us | 226.42 us | 37.750 us |      7 KB |
| sorted_set_distinct |  71.79 us |  65.12 us | 192.01 us | 44.500 us |   5.57 KB |
| builder_distinct    |  92.75 us | 118.14 us | 348.35 us | 54.200 us |   5.46 KB |
| toimmutable_distinct| 101.97 us | 133.28 us | 392.99 us | 57.200 us |    7.6 KB |
| immutable_distinct  | 121.89 us | 113.10 us | 333.48 us | 81.350 us |  20.67 KB |
*/
