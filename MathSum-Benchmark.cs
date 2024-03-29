using Microsoft.Diagnostics.Runtime;
using System;
using System.Numerics;
using System.Runtime.Intrinsics;

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
        var one = Vector2.One;
        for (ulong i = 0; i < ncycles; ++i)
        {
            v2 += one; v2 += one; v2 += one; v2 += one; v2 += one;
            v2 += one; v2 += one; v2 += one; v2 += one; v2 += one;
        }
        return v2;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public object bench_vector256i()
    {
        Vector256<int> v = new();
        var one = Vector256<int>.One;
        for (ulong i = 0; i < ncycles; ++i)
        {
            v = Vector256.Add(Vector256.Add(Vector256.Add(Vector256.Add(Vector256.Add(v, one), one), one), one), one);
            v = Vector256.Add(Vector256.Add(Vector256.Add(Vector256.Add(Vector256.Add(v, one), one), one), one), one);
        }
        return v;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public Vector256<double> bench_vector256d()
    {
        Vector256<double> v = new();
        var one = Vector256<double>.One;
        for (ulong i = 0; i < ncycles; ++i)
        {
            v = Vector256.Add(Vector256.Add(Vector256.Add(Vector256.Add(Vector256.Add(v, one), one), one), one), one);
            v = Vector256.Add(Vector256.Add(Vector256.Add(Vector256.Add(Vector256.Add(v, one), one), one), one), one);
        }
        return v;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public Vector64<double> bench_vector64d()
    {
        Vector64<double> v = new();
        var one = Vector64<double>.One;
        for (ulong i = 0; i < ncycles; ++i)
        {
            Vector64.Add(Vector64.Add(Vector64.Add(Vector64.Add(Vector64.Add(v, one), one), one), one), one);
            Vector64.Add(Vector64.Add(Vector64.Add(Vector64.Add(Vector64.Add(v, one), one), one), one), one);
        }
        return v;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public Vector64<int> bench_vector64i()
    {
        Vector64<int> v = new();
        var one = Vector64<int>.One;
        for (ulong i = 0; i < ncycles; ++i)
        {
            v = Vector64.Add(Vector64.Add(Vector64.Add(Vector64.Add(Vector64.Add(v, one), one), one), one), one);
            v = Vector64.Add(Vector64.Add(Vector64.Add(Vector64.Add(Vector64.Add(v, one), one), one), one), one);
        }
        return v;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public object bench_vector512i()
    {
        Vector512<int> v = new();
        var one = Vector512<int>.One;
        for (ulong i = 0; i < ncycles; ++i)
        {
            v = Vector512.Add(Vector512.Add(Vector512.Add(Vector512.Add(Vector512.Add(v, one), one), one), one), one);
            v = Vector512.Add(Vector512.Add(Vector512.Add(Vector512.Add(Vector512.Add(v, one), one), one), one), one);
        }
        return v;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public object bench_vector512d()
    {
        Vector512<double> v = new();
        var one = Vector512<double>.One;
        for (ulong i = 0; i < ncycles; ++i)
        {
            v = Vector512.Add(Vector512.Add(Vector512.Add(Vector512.Add(Vector512.Add(v, one), one), one), one), one);
            v = Vector512.Add(Vector512.Add(Vector512.Add(Vector512.Add(Vector512.Add(v, one), one), one), one), one);
        }
        return v;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public object bench_vector512f()
    {
        Vector512<float> v = new();
        var one = Vector512<float>.One;
        for (ulong i = 0; i < ncycles; ++i)
        {
            v = Vector512.Add(Vector512.Add(Vector512.Add(Vector512.Add(Vector512.Add(v, one), one), one), one), one);
            v = Vector512.Add(Vector512.Add(Vector512.Add(Vector512.Add(Vector512.Add(v, one), one), one), one), one);
        }
        return v;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public object bench_vector256f()
    {
        Vector256<float> v = new();
        var one = Vector256<float>.One;
        for (ulong i = 0; i < ncycles; ++i)
        {
            v = Vector256.Add(Vector256.Add(Vector256.Add(Vector256.Add(Vector256.Add(v, one), one), one), one), one);
            v = Vector256.Add(Vector256.Add(Vector256.Add(Vector256.Add(Vector256.Add(v, one), one), one), one), one);
        }
        return v;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public object bench_vector64f()
    {
        Vector64<float> v = new();
        var one = Vector64<float>.One;
        for (ulong i = 0; i < ncycles; ++i)
        {
            Vector64.Add(Vector64.Add(Vector64.Add(Vector64.Add(Vector64.Add(v, one), one), one), one), one);
            Vector64.Add(Vector64.Add(Vector64.Add(Vector64.Add(Vector64.Add(v, one), one), one), one), one);
        }
        return v;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public object bench_vector64s()
    {
        Vector64<ushort> v = new();
        var one = Vector64<ushort>.One;
        for (ulong i = 0; i < ncycles; ++i)
        {
            v = Vector64.Add(Vector64.Add(Vector64.Add(Vector64.Add(Vector64.Add(v, one), one), one), one), one);
            v = Vector64.Add(Vector64.Add(Vector64.Add(Vector64.Add(Vector64.Add(v, one), one), one), one), one);
        }
        return v;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public object bench_vector64b()
    {
        Vector64<byte> v = new();
        var one = Vector64<byte>.One;
        for (ulong i = 0; i < ncycles; ++i)
        {
            v = Vector64.Add(Vector64.Add(Vector64.Add(Vector64.Add(Vector64.Add(v, one), one), one), one), one);
            v = Vector64.Add(Vector64.Add(Vector64.Add(Vector64.Add(Vector64.Add(v, one), one), one), one), one);
        }
        return v;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public Int128 bench_int128()
    {
        Int128 v = 0;
        for (ulong i = 0; i < ncycles; ++i)
        {
            v += 1; v += 1; v += 1; v += 1; v += 1;
            v += 1; v += 1; v += 1; v += 1; v += 1;
        }
        return v;
        Vector<double> vv = new();
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public object bench_vd()
    {
        Vector<double> one = new(1);
        Vector<double> v = new(0);
        for (ulong i = 0; i < ncycles; ++i)
        {
            v = Vector.Add(Vector.Add(Vector.Add(Vector.Add(Vector.Add(v, one), one), one), one), one);
            v = Vector.Add(Vector.Add(Vector.Add(Vector.Add(Vector.Add(v, one), one), one), one), one);
        }
        return v;
    }

    //[BenchmarkDotNet.Attributes.Benchmark]
    public object bench_vl()
    {
        Vector<long> one = new(1);
        Vector<long> v = new(0);
        for (ulong i = 0; i < ncycles; ++i)
        {
            v = Vector.Add(Vector.Add(Vector.Add(Vector.Add(Vector.Add(v, one), one), one), one), one);
            v = Vector.Add(Vector.Add(Vector.Add(Vector.Add(Vector.Add(v, one), one), one), one), one);
        }
        return v;
    }

    [BenchmarkDotNet.Attributes.Benchmark]
    public object bench_vector128l()
    {
        var one = Vector128<long>.One;
        Vector128<long> v = new();
        for (ulong i = 0; i < ncycles; ++i)
        {
            v = Vector128.Add(Vector128.Add(Vector128.Add(Vector128.Add(Vector128.Add(v, one), one), one), one), one);
            v = Vector128.Add(Vector128.Add(Vector128.Add(Vector128.Add(Vector128.Add(v, one), one), one), one), one);
        }
        return v;
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

| Method         | Mean      | Error     | StdDev    | Allocated |
|--------------- |----------:|----------:|----------:|----------:|
| bench_nint     |  44.52 ms |  0.248 ms |  0.220 ms |      33 B |
| bench_uint     |  44.93 ms |  0.425 ms |  0.398 ms |      36 B |
| bench_ulong    |  45.49 ms |  0.287 ms |  0.269 ms |      33 B |
| vector128l     | 222.60 ms |   0.94 ms |   0.88 ms |     165 B |
| bench_vl       | 222.30 ms |   1.19 ms |   1.12 ms |     181 B |
| vector256i     | 227.30 ms |   1.56 ms |   1.46 ms |     181 B |
| vector128l     | 228.30 ms |   2.21 ms |   1.96 ms |     165 B |
| vector512i     | 240.40 ms |  4.31 ms  |   4.03 ms |     280 B |
| bench_ushort   | 275.51 ms |  1.733 ms |  1.447 ms |     200 B |
| bench_prop_int | 298.60 ms |   2.39 ms |   2.11 ms |     200 B |
| bench_int128   | 428.60 ms |   3.07 ms |   2.40 ms |     400 B |
| vector64d      | 834.46 ms |  8.517 ms |  7.967 ms |     400 B |
| bench_vector2  | 890.40 ms |   6.35 ms |   5.31 ms |     400 B |
| bench_double   | 894.84 ms |  5.198 ms |  4.862 ms |     400 B |
| bench_complex  | 895.10 ms |   8.00 ms |   7.48 ms |     400 B |
| bench_float    | 924.91 ms |  6.610 ms |  6.183 ms |     400 B |
| vector256f     |  1,047 ms |  20.48 ms |  34.21 ms |     448 B |
| vector256d     |  1,048 ms |  20.39 ms |  27.22 ms |     400 B |
| bench_vd       |  1,051 ms |  20.43 ms |  29.30 ms |     448 B |
| vector512f     |  1,108 ms |   4.94 ms |   4.38 ms |     480 B |
| vector512d     |  1,113 ms |  12.39 ms |  11.59 ms |     480 B |
| vector64i      |  3,906 ms |  5.552 ms |  4.637 ms |     400 B |
| vector64f      |  4,591 ms | 10.767 ms | 10.071 ms |     424 B |
| vector64s      |  4,812 ms |   19.8 ms |   18.5 ms |     424 B |
| bench_bigint   |  5,549 ms | 17.391 ms | 15.417 ms |     400 B |
| bench_decimal  |  5,775 ms | 26.748 ms | 22.336 ms |     400 B |
| vector64b      |  7,963 ms |    7.6 ms |    6.8 ms |     424 B |
| bigint_10      | 20,000 ms |    189 ms |    177 ms |   29.8 GB |
| bigint_100     | 25,490 ms |           |           |           |
| bigint_1000    | 26,890 ms |    50 ms  |     47 ms |  67.06 GB |
*/
