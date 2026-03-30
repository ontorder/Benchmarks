using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Planet.Garavot.EntityFrameworkCore;

namespace dbctxbench;

[MemoryDiagnoser]
[MaxIterationCount(30)]
public class DbContextBench
{
    private ServiceProvider _serviceProvider;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();
        services.AddDbContext<GaravotDbContext>(static options =>
            options.UseMySql("server=192.168.150.205;userid=root;pwd=planet;port=3307;database=garavot_ac103u;sslmode=none;charset=utf8mb4;", new MySqlServerVersion("8.0"), null));
        _serviceProvider = services.BuildServiceProvider();
    }

    //[Benchmark]
    public async Task<object> read_notrack()
    {
        var ctx = _serviceProvider.GetRequiredService<GaravotDbContext>();
        return await ctx.Contents.AsNoTracking().ToListAsync();
    }

    //[Benchmark]
    public async Task<object> read_tracking()
    {
        var ctx = _serviceProvider.GetRequiredService<GaravotDbContext>();
        return await ctx.Contents.ToListAsync();
    }

    //[Benchmark]
    public async Task<object> read_notrack_scope()
    {
        using var scop = _serviceProvider.CreateScope();
        var ctx = scop.ServiceProvider.GetRequiredService<GaravotDbContext>();
        return await ctx.Contents.AsNoTracking().ToListAsync();
    }

    //[Benchmark]
    public async Task<object> read_tracking_scope()
    {
        using var scop = _serviceProvider.CreateScope();
        var ctx = scop.ServiceProvider.GetRequiredService<GaravotDbContext>();
        return await ctx.Contents.ToListAsync();
    }

    [Benchmark]
    public async Task<object> read_notrack_scope_cols()
    {
        using var scop = _serviceProvider.CreateScope();
        var ctx = scop.ServiceProvider.GetRequiredService<GaravotDbContext>();
        return await ctx.Contents.AsNoTracking().Select(static _ => new { _.ContentId, _.Title }).ToListAsync();
    }

    [Benchmark]
    public async Task<object> read_tracking_scope_cols()
    {
        using var scop = _serviceProvider.CreateScope();
        var ctx = scop.ServiceProvider.GetRequiredService<GaravotDbContext>();
        return await ctx.Contents.Select(static _ => new { _.ContentId, _.Title }).ToListAsync();
    }
}
/*

363 records (table: 169 columns; Content: 39)

| Method                   | Mean     | Error     | StdDev    | Gen0     | Gen1     | Allocated  |
|------------------------- |---------:|----------:|----------:|---------:|---------:|-----------:|
| read_notrack_scope_cols  | 2.984 ms | 0.1386 ms | 0.2075 ms |  35.1563 |   7.8125 |     235 KB |
| read_tracking_scope_cols | 3.116 ms | 0.1987 ms | 0.2974 ms |  35.1563 |   7.8125 |  234.34 KB |
| read_tracking            | 10.45 ms |  0.811 ms |  1.213 ms |  15.6250 |        - |  166.52 KB |
| read_notrack             | 10.51 ms |  0.648 ms |  0.970 ms | 109.3750 |  46.8750 |  701.56 KB |
| read_notrack_scope       | 11.34 ms |  0.838 ms |  1.255 ms | 125.0000 |  46.8750 |  805.47 KB |
| read_tracking_scope      | 13.23 ms |  0.601 ms |  0.900 ms | 328.1250 | 187.5000 | 2021.24 KB |

*/
