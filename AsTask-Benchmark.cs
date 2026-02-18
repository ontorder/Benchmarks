using BenchmarkDotNet.Attributes;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace test;

[MemoryDiagnoser]
public class bench_astask
{
    private static readonly Memory<byte> buffer = new byte[1] { 99 };
    private readonly static Channel<int> channel = Channel.CreateUnbounded<int>();
    private readonly static Stream stream = new MemoryStream();

    [Benchmark]
    public async Task<int> valuetask_channel()
    {
        await channel.Writer.WriteAsync(1);
        return await channel.Reader.ReadAsync();
    }

    [Benchmark]
    public async ValueTask<int> valuetask_channel_vt()
    {
        await channel.Writer.WriteAsync(1);
        return await channel.Reader.ReadAsync();
    }

    [Benchmark]
    public int valuetask_channel_sync()
    {
        var twrite = channel.Writer.WriteAsync(1);
        if (twrite.IsCompleted == false) E.InliningThrowHelper();
        var tread = channel.Reader.ReadAsync();
        if (tread.IsCompleted == false) E.InliningThrowHelper();
        return tread.Result;
    }

    [Benchmark]
    public async Task<byte> valuetask_stream()
    {
        await stream.WriteAsync(buffer);
        await stream.ReadAsync(buffer);
        return buffer.Span[0];
    }

    [Benchmark]
    public async ValueTask<byte> valuetask_stream_vt()
    {
        await stream.WriteAsync(buffer);
        await stream.ReadAsync(buffer);
        return buffer.Span[0];
    }

    [Benchmark]
    public byte valuetask_stream_sync()
    {
        var twrite = stream.WriteAsync(buffer);
        if (twrite.IsCompleted == false) E.InliningThrowHelper();
        var tread = stream.ReadAsync(buffer);
        if (tread.IsCompleted == false) E.InliningThrowHelper();
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
file static class E
{
    [DoesNotReturn] public static int InliningThrowHelper() => throw new Exception();
}
/*
first run
| Method            | Mean     | Error    | StdDev   | Gen0   | Allocated |
|------------------ |---------:|---------:|---------:|-------:|----------:|
| astask_channel    | 45.42 ns | 0.218 ns | 0.182 ns |      - |         - |
| astask_stream     | 49.26 ns | 0.340 ns | 0.284 ns | 0.0114 |      72 B |
| valuetask_channel | 53.76 ns | 0.174 ns | 0.163 ns |      - |         - |
| valuetask_stream  | 55.64 ns | 0.243 ns | 0.216 ns | 0.0114 |      72 B |

second run
| Method                 | Mean     | Error    | StdDev   | Gen0   | Allocated |
|----------------------- |---------:|---------:|---------:|-------:|----------:|
| valuetask_stream_sync  | 42.20 ns | 0.185 ns | 0.154 ns |      - |         - |
| astask_stream          | 49.16 ns | 0.293 ns | 0.244 ns | 0.0114 |      72 B |
| valuetask_stream_vt    | 51.15 ns | 0.256 ns | 0.214 ns |      - |         - |
| valuetask_stream       | 55.14 ns | 0.926 ns | 0.821 ns | 0.0114 |      72 B |

| astask_channel         | 45.26 ns | 0.175 ns | 0.163 ns |      - |         - |
| valuetask_channel_sync | 46.50 ns | 0.615 ns | 0.576 ns |      - |         - |
| valuetask_channel      | 57.28 ns | 0.379 ns | 0.355 ns |      - |         - |
| valuetask_channel_vt   | 57.56 ns | 0.274 ns | 0.243 ns |      - |         - |

third run
| Method                 | Mean     | Error    | StdDev   | Gen0   | Allocated |
|----------------------- |---------:|---------:|---------:|-------:|----------:|
| valuetask_stream_sync  | 31.59 ns | 0.347 ns | 0.308 ns |      - |         - |
| astask_stream          | 51.08 ns | 0.245 ns | 0.204 ns | 0.0114 |      72 B |
| valuetask_stream_vt    | 52.62 ns | 0.513 ns | 0.401 ns |      - |         - |
| valuetask_stream       | 55.21 ns | 0.226 ns | 0.189 ns | 0.0114 |      72 B |

| valuetask_channel_sync | 35.93 ns | 0.184 ns | 0.172 ns |      - |         - |
| astask_channel         | 45.94 ns | 0.212 ns | 0.188 ns |      - |         - |
| valuetask_channel      | 55.89 ns | 0.444 ns | 0.416 ns |      - |         - |
| valuetask_channel_vt   | 57.13 ns | 0.262 ns | 0.232 ns |      - |         - |
*/
