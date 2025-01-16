using BenchmarkDotNet.Attributes;
using System.Buffers;

namespace test;

[MemoryDiagnoser]
public class test_frequent_alloc
{
    private readonly byte[] src = new byte[20];
    private readonly ArrayPool<byte> pool = ArrayPool<byte>.Shared;

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
/* for many reasons i don't trust this that much
| Method               | Mean      | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|--------------------- |----------:|---------:|---------:|-------:|-------:|----------:|
| regular_alloc        |  16.12 ns | 0.144 ns | 0.120 ns | 0.0306 |      - |     192 B |
| object_pool          | 101.56 ns | 0.804 ns | 0.752 ns |      - |      - |         - |
| regular_alloc_bigger | 103.81 ns | 2.023 ns | 1.794 ns | 0.3723 | 0.0004 |    2336 B |
| object_pool_bigger   | 141.00 ns | 2.087 ns | 1.850 ns |      - |      - |         - |
*/
