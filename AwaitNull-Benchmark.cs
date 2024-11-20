using BenchmarkDotNet.Attributes;
using System;
using System.Threading.Tasks;

namespace test;

public class null_task_bench
{
    Func<Task>? _nulltask;

    public null_task_bench(Func<Task>? nulltask = null)
        => _nulltask = nulltask;

    [Benchmark]
    public Task waitif()
        => _nulltask?.Invoke() ?? Task.CompletedTask;

    [Benchmark]
    public async Task awaitif()
        => await (_nulltask?.Invoke() ?? Task.CompletedTask);

    [Benchmark]
    public async Task<object?> retnull()
        => await Task.FromResult<object?>(null);

    [Benchmark]
    public async Task waitnull()
        => await Task.CompletedTask;
}
/*

| Method   | Mean      | Error     | StdDev    |
|--------- |----------:|----------:|----------:|
| waitif   | 0.5833 ns | 0.0145 ns | 0.0129 ns |
| awaitif  | 8.3054 ns | 0.0774 ns | 0.0724 ns |
| retnull  | 6.6307 ns | 0.0383 ns | 0.0320 ns |
| waitnull | 6.8746 ns | 0.0661 ns | 0.0618 ns |

*/
