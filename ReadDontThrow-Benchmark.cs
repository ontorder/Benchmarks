using BenchmarkDotNet.Attributes;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace test;

[MemoryDiagnoser]
public class bench_streamnoexcept
{
    private static readonly MemoryStream ms = new();

    static bench_streamnoexcept()
    {
        var b = Enumerable.Range(0, 255).Select(static i => (byte)i).ToArray();
        ms.Write(b);
        ms.Write(b);
    }

    [Benchmark]
    public async Task<object> read_await()
    {
        ms.Position = 0;
        var b = new byte[100];
        await ms.ReadAsync(b);
        return b;
    }

    private readonly static TaskCompletionSource static_infinite_task = new();

    [Benchmark]
    public async Task<object> read_against_tcs()
    {
        ms.Position = 0;
        var b = new byte[100];
        var t = ms.ReadAsync(b).AsTask();
        _ = await Task.WhenAny(t, static_infinite_task.Task);
        return b;
    }

    private readonly static Task static_infinite_delay = Task.Delay(Timeout.Infinite);

    [Benchmark]
    public async Task<object> read_whenany_sdelay()
    {
        ms.Position = 0;
        var b = new byte[100];
        var t = ms.ReadAsync(b).AsTask();
        _ = await Task.WhenAny(t, static_infinite_delay);
        return b;
    }

    [Benchmark]
    public async Task<object> read_whenany_vt()
    {
        ms.Position = 0;
        var b = new byte[100];
        var t = ms.ReadAsync(b).AsTask();
        await new ValueTask(Task.WhenAny(t, static_infinite_task.Task));
        return b;
    }

    ~bench_streamnoexcept()
    {
        ms.Dispose();
    }
}
/*
| Method              | Mean     | Error    | StdDev   |
|-------------------- |---------:|---------:|---------:|
| read_await          | 44.37 ns | 0.673 ns | 0.801 ns |
| read_whenany        |       NA |       NA |       NA |
| read_whenany_sdelay | 54.92 ns | 0.530 ns | 0.470 ns |

| Method              | Mean     | Error    | StdDev   |
|-------------------- |---------:|---------:|---------:|
| read_await          | 45.70 ns | 0.370 ns | 0.309 ns |
| read_against_tcs    | 51.39 ns | 0.656 ns | 0.614 ns |
| read_whenany_sdelay | 51.39 ns | 0.297 ns | 0.278 ns |

| Method              | Mean     | Error    | StdDev   | Gen0   | Allocated |
|-------------------- |---------:|---------:|---------:|-------:|----------:|
| read_await          | 44.55 ns | 0.415 ns | 0.368 ns | 0.0318 |     200 B |
| read_against_tcs    | 51.02 ns | 0.238 ns | 0.223 ns | 0.0548 |     344 B |
| read_whenany_sdelay | 53.27 ns | 0.522 ns | 0.488 ns | 0.0548 |     344 B |
| read_whenany_vt     | 55.47 ns | 0.647 ns | 0.605 ns | 0.0548 |     344 B |
*/
