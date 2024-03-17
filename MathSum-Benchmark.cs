using System.Numerics;

namespace test;

[BenchmarkDotNet.Attributes.MemoryDiagnoser]
public class bench_types
{
    const ulong ncycles = 100_000_000;

    public int prop_int { get; set; }
    private BigInteger bigint1000 = 0;

    //[BenchmarkDotNet.Attributes.Benchmark]
    public ulong bench_ulong()
    {
        ulong count = 0;
        for (ulong i = 0; i < ncycles; ++i)
        {
            count += 1; count += 1; count += 1; count += 1; count += 1;
            count += 1; count += 1; count += 1; count += 1; count += 1;
        }
        return count;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public double bench_double()
    {
        double count = 0;
        for (ulong i = 0; i < ncycles; ++i)
        {
            count += 1; count += 1; count += 1; count += 1; count += 1;
            count += 1; count += 1; count += 1; count += 1; count += 1;
        }
        return count;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public decimal bench_decimal()
    {
        decimal count = 0;
        for (ulong i = 0; i < ncycles; ++i)
        {
            count += 1; count += 1; count += 1; count += 1; count += 1;
            count += 1; count += 1; count += 1; count += 1; count += 1;
        }
        return count;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public BigInteger bench_bigint()
    {
        BigInteger count = 0;
        for (ulong i = 0; i < ncycles; ++i)
        {
            count += 1; count += 1; count += 1; count += 1; count += 1;
            count += 1; count += 1; count += 1; count += 1; count += 1;
        }
        return count;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public BigInteger bench_bigint_10()
    {
        BigInteger count = new(10e10);
        for (ulong i = 0; i < ncycles; ++i)
        {
            count += 1; count += 1; count += 1; count += 1; count += 1;
            count += 1; count += 1; count += 1; count += 1; count += 1;
        }
        return count;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public BigInteger bench_bigint_100()
    {
        BigInteger count = new(10e100);
        for (ulong i = 0; i < ncycles; ++i)
        {
            count += 1; count += 1; count += 1; count += 1; count += 1;
            count += 1; count += 1; count += 1; count += 1; count += 1;
        }
        return count;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public BigInteger bench_bigint_1000()
    {
        BigInteger count = bigint1000;
        for (ulong i = 0; i < ncycles; ++i)
        {
            count += 1; count += 1; count += 1; count += 1; count += 1;
            count += 1; count += 1; count += 1; count += 1; count += 1;
        }
        return count;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public float bench_float()
    {
        float count = 0;
        for (ulong i = 0; i < ncycles; ++i)
        {
            count += 1; count += 1; count += 1; count += 1; count += 1;
            count += 1; count += 1; count += 1; count += 1; count += 1;
        }
        return count;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public uint bench_uint()
    {
        uint count = 0;
        for (ulong i = 0; i < ncycles; ++i)
        {
            count += 1; count += 1; count += 1; count += 1; count += 1;
            count += 1; count += 1; count += 1; count += 1; count += 1;
        }
        return count;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public ushort bench_ushort()
    {
        ushort count = 0;
        for (ulong i = 0; i < ncycles; ++i)
        {
            count += 1; count += 1; count += 1; count += 1; count += 1;
            count += 1; count += 1; count += 1; count += 1; count += 1;
        }
        return count;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public nint bench_nint()
    {
        nint count = 0;
        for (ulong i = 0; i < ncycles; ++i)
        {
            count += 1; count += 1; count += 1; count += 1; count += 1;
            count += 1; count += 1; count += 1; count += 1; count += 1;
        }
        return count;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public int bench_prop_int()
    {
        for (ulong i = 0; i < ncycles; ++i)
        {
            prop_int += 1; prop_int += 1; prop_int += 1; prop_int += 1; prop_int += 1;
            prop_int += 1; prop_int += 1; prop_int += 1; prop_int += 1; prop_int += 1;
        }
        return prop_int;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public Complex bench_complex()
    {
        Complex c = new(real: 0, imaginary: 1);
        for (ulong i = 0; i < ncycles; ++i)
        {
            c += 1; c += 1; c += 1; c += 1; c += 1;
            c += 1; c += 1; c += 1; c += 1; c += 1;
        }
        return c;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public Vector2 bench_vector2()
    {
        Vector2 v2 = new(x: 0, y: 1);
        for (ulong i = 0; i < ncycles; ++i)
        {
            v2 += Vector2.One; v2 += Vector2.One; v2 += Vector2.One; v2 += Vector2.One; v2 += Vector2.One;
            v2 += Vector2.One; v2 += Vector2.One; v2 += Vector2.One; v2 += Vector2.One; v2 += Vector2.One;
        }
        return v2;
    }

    [BenchmarkDotNet.Attributes.GlobalSetup]
    public void setup()
    {
        bigint1000 = new(10e100);
        bigint1000 *= 10;
    }
}

/*
sbagliato
ncycles = 1_000_000_000

| Method       | Mean        | Error       | StdDev      | Allocated |
|------------- |------------:|------------:|------------:|----------:|
| bench_ulong  |    460.5 ms |     6.16 ms |     5.46 ms |     400 B |
| bench_double |  8,841.8 ms |    18.86 ms |    17.64 ms |     400 B |
| bech_decimal | 71,252.7 ms | 1,418.59 ms | 2,166.33 ms |     400 B |

ncycles = 100_000_000

| Method         | Mean        | Error     | StdDev    | Allocated |
|--------------- |------------:|----------:|----------:|----------:|
| bench_nint     |    44.52 ms |  0.248 ms |  0.220 ms |      33 B |
| bench_uint     |    44.93 ms |  0.425 ms |  0.398 ms |      36 B |
| bench_ulong    |    45.49 ms |  0.287 ms |  0.269 ms |      33 B |
| bench_ushort   |   275.51 ms |  1.733 ms |  1.447 ms |     200 B |
| bench_prop_int |   298.6  ms |   2.39 ms |   2.11 ms |     200 B |
| bench_vector2  |   890.4  ms |   6.35 ms |   5.31 ms |     400 B |
| bench_double   |   894.84 ms |  5.198 ms |  4.862 ms |     400 B |
| bench_complex  |   895.1  ms |   8.00 ms |   7.48 ms |     400 B |
| bench_float    |   924.91 ms |  6.610 ms |  6.183 ms |     400 B |
| bench_bigint   | 5,549.14 ms | 17.391 ms | 15.417 ms |     400 B |
| bench_decimal  | 5,775.41 ms | 26.748 ms | 22.336 ms |     400 B |
| bigint_10      | 20.00 s     |  0.189 s  | 0.177 s   |   29.8 GB |
| bigint_100     | 25.49 s     |           |           |           |
| bigint_1000    | 26.89 s     |  0.050 s  | 0.047 s   |  67.06 GB |
*/
