using BenchmarkDotNet.Attributes;

namespace test;

[MemoryDiagnoser]
public class bench_distinct
{
    private readonly Random _random = new(Seed: 12345);

    private const int Count = 100;
    private const int MaxValue = 50;

    [Benchmark]
    public List<int> CheckBeforeAdd()
    {
        var list = new List<int>(Count);

        for (int i = 0; i < Count; i++)
        {
            int value = _random.Next(MaxValue);

            if (!list.Contains(value))
            {
                list.Add(value);
            }
        }

        return list;
    }

    [Benchmark]
    public ICollection<int> UsingHashset()
    {
        var list = new HashSet<int>(Count);

        for (int i = 0; i < Count; i++)
        {
            int value = _random.Next(MaxValue);
            list.Add(value);
        }

        return list;
    }

    [Benchmark]
    public List<int> DistinctAtEnd()
    {
        var list = new List<int>(Count);
        var random = new Random(12345);

        for (int i = 0; i < Count; i++)
        {
            list.Add(random.Next(MaxValue));
        }

        return [.. list.Distinct()];
    }
}

/*

x100 net10
| Method         | Mean     | Error     | StdDev    | Gen0   | Allocated |
|--------------- |---------:|----------:|----------:|-------:|----------:|
| CheckBeforeAdd | 1.889 us | 0.0334 us | 0.0296 us | 0.0725 |     456 B |
| DistinctAtEnd  | 1.931 us | 0.0200 us | 0.0156 us | 0.4997 |    3152 B |
| UsingHashset   | 2.133 us | 0.0156 us | 0.0139 us | 0.2899 |    1832 B |

x10 net10
| Method         | Mean     | Error   | StdDev  | Gen0   | Allocated |
|--------------- |---------:|--------:|--------:|-------:|----------:|
| CheckBeforeAdd | 137.5 ns | 1.38 ns | 1.15 ns | 0.0153 |      96 B |
| UsingHashset   | 217.0 ns | 1.43 ns | 1.26 ns | 0.0470 |     296 B |
| DistinctAtEnd  | 549.8 ns | 8.85 ns | 9.47 ns | 0.1421 |     896 B |

x100 75% net10
| Method         | Mean     | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|--------------- |---------:|----------:|----------:|-------:|-------:|----------:|
| DistinctAtEnd  | 1.621 us | 0.0065 us | 0.0057 us | 0.4730 |      - |    2976 B |
| UsingHashset   | 1.857 us | 0.0103 us | 0.0091 us | 0.2918 | 0.0019 |    1832 B |
| CheckBeforeAdd | 2.051 us | 0.0140 us | 0.0131 us | 0.0725 |      - |     456 B |

x100 50% net10
| Method         | Mean     | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|--------------- |---------:|----------:|----------:|-------:|-------:|----------:|
| DistinctAtEnd  | 1.615 us | 0.0180 us | 0.0168 us | 0.4654 | 0.0019 |    2928 B |
| UsingHashset   | 1.695 us | 0.0249 us | 0.0256 us | 0.2918 | 0.0019 |    1832 B |
| CheckBeforeAdd | 2.104 us | 0.0355 us | 0.0296 us | 0.0725 |      - |     456 B |

*/
