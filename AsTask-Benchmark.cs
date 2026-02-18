using BenchmarkDotNet.Attributes;
using System;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace test;

[MemoryDiagnoser]
public class bench_astask
{
    private readonly static Memory<byte> buffer = new byte[1] { 99 };
    private readonly static Channel<int> channel = Channel.CreateUnbounded<int>();
    private readonly static Stream stream = new MemoryStream();

    [Benchmark]
    public async Task<int> valuetask_channel()
    {
        await channel.Writer.WriteAsync(1);
        return await channel.Reader.ReadAsync();
    }

    [Benchmark]
    public async Task<byte> valuetask_stream()
    {
        await stream.WriteAsync(buffer);
        await stream.ReadAsync(buffer);
        return buffer.Span[0];
    }

    [Benchmark]
    public async Task<int> astask_channel()
    {
        await channel.Writer.WriteAsync(1).AsTask();
        return await channel.Reader.ReadAsync().AsTask();
    }

    [Benchmark]
    public async Task<byte> astask_stream()
    {
        await stream.WriteAsync(buffer).AsTask();
        await stream.ReadAsync(buffer).AsTask();
        return buffer.Span[0];
    }
}
/*
| Method            | Mean     | Error    | StdDev   | Gen0   | Allocated |
|------------------ |---------:|---------:|---------:|-------:|----------:|
| astask_channel    | 45.42 ns | 0.218 ns | 0.182 ns |      - |         - |
| astask_stream     | 49.26 ns | 0.340 ns | 0.284 ns | 0.0114 |      72 B |
| valuetask_channel | 53.76 ns | 0.174 ns | 0.163 ns |      - |         - |
| valuetask_stream  | 55.64 ns | 0.243 ns | 0.216 ns | 0.0114 |      72 B |
*/
