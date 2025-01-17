using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using System.Buffers;

namespace test;

[MemoryDiagnoser, SimpleJob(RunStrategy.ColdStart, iterationCount: 5)]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class test_frequent_alloc
{
    private readonly byte[] src = new byte[20];
    private readonly ArrayPool<byte> pool = ArrayPool<byte>.Shared;

    public test_frequent_alloc()
    {
        var pre = pool.Rent(999);
        pool.Return(pre);
    }

    [Benchmark]
    public byte regular_alloc()
    {
        var b1 = new byte[30];
        b1[5] = src[1];
        var b2 = new byte[40];
        b2[10] = b1[2];
        var b3 = new byte[45];
        b3[33] = b2[22];
        return b3[44];
    }

    [Benchmark]
    public byte regular_alloc_bigger()
    {
        var b1 = new byte[300];
        b1[5] = src[1];
        var b2 = new byte[400];
        b2[10] = b1[2];
        var b3 = new byte[450];
        b3[33] = b2[22];
        var b4 = new byte[500];
        b4[330] = b3[220];
        var b5 = new byte[550];
        b5[333] = b4[222];
        return b5[123];
    }

    [Benchmark]
    public byte object_pool()
    {
        var b1 = pool.Rent(30);
        b1[5] = src[1];
        var b2 = pool.Rent(40);
        b2[11] = b1[22];
        pool.Return(b1);
        var b3 = pool.Rent(45);
        b3[23] = b2[13];
        pool.Return(b2);
        var ret = b3[42];
        pool.Return(b3);
        return ret;
    }

    [Benchmark]
    public byte object_pool_bigger()
    {
        var b1 = pool.Rent(300);
        b1[5] = src[1];
        var b2 = pool.Rent(400);
        b2[11] = b1[22];
        pool.Return(b1);
        var b3 = pool.Rent(450);
        b3[23] = b2[13];
        pool.Return(b2);
        var b4 = pool.Rent(500);
        b4[233] = b3[135];
        pool.Return(b3);
        var b5 = pool.Rent(550);
        b5[213] = b4[444];
        pool.Return(b4);
        var ret = b5[420];
        pool.Return(b5);
        return ret;
    }
}
/* throughput test
| Method               | Mean      | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|--------------------- |----------:|---------:|---------:|-------:|-------:|----------:|
| regular_alloc        |  16.12 ns | 0.144 ns | 0.120 ns | 0.0306 |      - |     192 B |
| object_pool          | 101.56 ns | 0.804 ns | 0.752 ns |      - |      - |         - |
| regular_alloc_bigger | 103.81 ns | 2.023 ns | 1.794 ns | 0.3723 | 0.0004 |    2336 B |
| object_pool_bigger   | 141.00 ns | 2.087 ns | 1.850 ns |      - |      - |         - |

coldstart test
| Method               | Mean      | Error     | StdDev    | Median   | Min      | Max      | Allocated |
|--------------------- |----------:|----------:|----------:|---------:|---------:|---------:|----------:|
| regular_alloc        |  58.94 us | 489.45 us | 127.11 us | 1.200 us | 1.200 us | 286.3 us |     928 B |
| regular_alloc_bigger |  63.48 us | 527.67 us | 137.03 us | 1.600 us | 1.200 us | 308.6 us |    3072 B |
| object_pool          | 117.68 us | 954.16 us | 247.79 us | 7.100 us | 3.200 us | 560.9 us |     880 B |
| object_pool_bigger   | 118.98 us | 957.75 us | 248.73 us | 9.000 us | 4.800 us | 563.9 us |    2320 B |
*/
/*
var test = new test_frequent_alloc();
var test_alloc = new int[1_000_000];
var test_alloc_big = new int[1_000_000];
var test_pool = new int[1_000_000];
var test_pool_big = new int[1_000_000];
var sw = new Stopwatch();

for (int j = 0; j < 1_000_000; ++j)
{
    sw.Restart();
    test.object_pool_bigger();
    sw.Stop();
    test_pool_big[j] = sw.Elapsed.Nanoseconds;
}

for (int j = 0; j < 1_000_000; ++j)
{
    sw.Restart();
    test.object_pool();
    sw.Stop();
    test_pool[j] = sw.Elapsed.Nanoseconds;
}

for (int j = 0; j < 1_000_000; ++j)
{
    sw.Restart();
    test.regular_alloc_bigger();
    sw.Stop();
    test_alloc_big[j] = sw.Elapsed.Nanoseconds;
}

for (int j = 0; j < 1_000_000; ++j)
{
    sw.Restart();
    test.regular_alloc();
    sw.Stop();
    test_alloc[j] = sw.Elapsed.Nanoseconds;
}
*/
