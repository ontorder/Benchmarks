using BenchmarkDotNet.Attributes;
using StackExchange.Redis;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace test;

public class bench_redis
{
    private static IConnectionMultiplexer _connection;
    private static IDatabase _db;
    private static ConfigurationOptions _settings;
    private static redisclient _redisclient;

    static bench_redis()
    {
        //initredis();
        //_db = _connection.GetDatabase();

        _redisclient = new redisclient();
        _redisclient.conn().Wait();

        Console.WriteLine("redis connected");
    }

    private static void initredis()
    {
        _settings = new ConfigurationOptions()
        {
            EndPoints = new EndPointCollection()
            {
                //{ "192.168.10.214", 6379 }
                { "192.168.150.205", 6379 }
            },
            ClientName = "test",
            ConnectTimeout = 5000,
        };
        _connection = ConnectionMultiplexer.Connect(_settings);
    }

    [Benchmark]
    public async Task redis1()
    {
        //await _db.StringSetAsync("test_redis1", "a");
        var t1 = _redisclient.send("SET test_redis1_x a");
        var t2 = _redisclient.readlineasync();
        await t1;
        await t2;
    }

    [Benchmark]
    public async Task redis10()
    {
        //await _db.StringSetAsync("test_redis10a", "a");
        //await _db.StringSetAsync("test_redis10b", "a");
        //await _db.StringSetAsync("test_redis10c", "a");
        //await _db.StringSetAsync("test_redis10d", "a");
        //await _db.StringSetAsync("test_redis10e", "a");
        //await _db.StringSetAsync("test_redis10f", "a");
        //await _db.StringSetAsync("test_redis10g", "a");
        //await _db.StringSetAsync("test_redis10h", "a");
        //await _db.StringSetAsync("test_redis10j", "a");
        //await _db.StringSetAsync("test_redis10k", "a");

        Task t1, t2;
        t1 = _redisclient.send("SET test_redis1_x 1"); t2 = _redisclient.readlineasync(); await t1; await t2;
        t1 = _redisclient.send("SET test_redis1_x 2"); t2 = _redisclient.readlineasync(); await t1; await t2;
        t1 = _redisclient.send("SET test_redis1_x 3"); t2 = _redisclient.readlineasync(); await t1; await t2;
        t1 = _redisclient.send("SET test_redis1_x 4"); t2 = _redisclient.readlineasync(); await t1; await t2;
        t1 = _redisclient.send("SET test_redis1_x 5"); t2 = _redisclient.readlineasync(); await t1; await t2;
        t1 = _redisclient.send("SET test_redis1_x 6"); t2 = _redisclient.readlineasync(); await t1; await t2;
        t1 = _redisclient.send("SET test_redis1_x 7"); t2 = _redisclient.readlineasync(); await t1; await t2;
        t1 = _redisclient.send("SET test_redis1_x 8"); t2 = _redisclient.readlineasync(); await t1; await t2;
        t1 = _redisclient.send("SET test_redis1_x 9"); t2 = _redisclient.readlineasync(); await t1; await t2;
        t1 = _redisclient.send("SET test_redis1_x 0"); t2 = _redisclient.readlineasync(); await t1; await t2;
    }

    [Benchmark]
    public async Task redis100()
    {
        for (int i = 0; i < 100; ++i)
        {
            var t1 = _redisclient.send($"SET test_redis1_x {i}");
            var t2 = _redisclient.readlineasync();
            await t1;
            await t2;
            //await _db.StringSetAsync($"test_redis100_{i}", "a");
        }
    }
}
public class redisclient
{
    private Socket s;
    public async Task conn()
    {
        s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await s.ConnectAsync("192.168.150.205", 6379);
        //await s.ConnectAsync("192.168.10.70", 6379);
        //_ = loopreadnullAsync();
    }

    private async Task loopreadnullAsync()
    {
        var b = new byte[8];
        do
        {
            var temp = await s.ReceiveAsync(b);
            if (temp == 0)
                break;
            foreach (var i in b)
                Debug.Write((char)i);
        }
        while (s.Connected);
    }
    public async Task readlineasync()
    {
        var b = new byte[8];
        do
        {
            var temp = await s.ReceiveAsync(b);
            if (temp == 0) break;
            var y = System.Text.Encoding.ASCII.GetString(b[..temp]);
            if (y == "+OK\r\n") return;
            else throw new Exception();
        }
        while (s.Connected);
    }
    public async Task send(string m)
    {
        var t = m + "\x0d\x0a";
        var b = System.Text.Encoding.ASCII.GetBytes(t);
        await s.SendAsync(b);
    }
}
/*
docker dev, redis client
| Method   | Mean        | Error     | StdDev      |
|--------- |------------:|----------:|------------:|
| redis1   |    371.2 us |  10.75 us |    30.50 us |
| redis10  |  3,520.2 us |  68.72 us |   122.15 us |
| redis100 | 34,857.7 us | 924.93 us | 2,698.07 us |

prod, raw client
| Method   | Mean       | Error      | StdDev     |
|--------- |-----------:|-----------:|-----------:|
| redis1   |   8.275 us |  0.1603 us |  0.1646 us |
| redis10  |  79.754 us |  1.5666 us |  1.5386 us |
| redis100 | 823.891 us | 13.3309 us | 11.8175 us |

docker dev, raw client
| Method   | Mean       | Error      | StdDev     |
|--------- |-----------:|-----------:|-----------:|
| redis1   |   7.807 us |  0.0856 us |  0.0801 us |
| redis10  |  78.442 us |  1.3042 us |  1.2200 us |
| redis100 | 823.574 us | 16.4111 us | 23.5363 us |

redis, raw client + serial
| Method   | Mean        | Error     | StdDev      |
|--------- |------------:|----------:|------------:|
| redis1   |    292.6 us |   5.82 us |     9.23 us |
| redis10  |  3,416.1 us | 131.48 us |   357.69 us |
| redis100 | 32,801.4 us | 936.30 us | 2,594.47 us |

redis, raw client + slightly parallel
| Method   | Mean        | Error     | StdDev      |
|--------- |------------:|----------:|------------:|
| redis1   |    326.2 us |   6.40 us |     6.57 us |
| redis10  |  3,284.7 us |  63.57 us |   142.19 us |
| redis100 | 31,174.8 us | 618.61 us | 1,517.46 us |

ugly mysql memory table test with ExecueSqlRaw because of keyless table
insert
test 1: 4,571 ms
test 10: 17,2224 ms
test 100: 159,3676 ms

update
test 1: 3,9886 ms
test 10: 2,7049 ms
test 100: 84,6015 ms

i expected it to be a little faster?
*/
/*
services.AddDbContext<memdbcontext>(setup =>
    setup.UseMySql("server=192.168.150.205;userid=root;pwd=papapapapa;port=5325;database=testdb;sslmode=none;charset=utf8mb4;",
    new MySqlServerVersion(new Version(8, 0))));
var ctx = services.BuildServiceProvider().GetService<memdbcontext>();

if (true)
{
    ctx.Database.ExecuteSqlRaw("update benchmem set stringvalue='-' where stringkey='warmup'");

    var sw = Stopwatch.StartNew();
    ctx.Database.ExecuteSqlRaw("update benchmem set stringvalue='c' where stringkey='test3_mem1_1'");
    sw.Stop();
    Console.WriteLine($"test 1: {sw.Elapsed.TotalMilliseconds} ms");

    sw = Stopwatch.StartNew();
    ctx.Database.ExecuteSqlRaw("update benchmem set stringvalue='c' where stringkey='test_mem10_0';"
        + "update benchmem set stringvalue='c' where stringkey='test3_mem10_1';"
        + "update benchmem set stringvalue='c' where stringkey='test3_mem10_2';"
        + "update benchmem set stringvalue='c' where stringkey='test3_mem10_3';"
        + "update benchmem set stringvalue='c' where stringkey='test3_mem10_4';"
        + "update benchmem set stringvalue='c' where stringkey='test3_mem10_5';"
        + "update benchmem set stringvalue='c' where stringkey='test3_mem10_6';"
        + "update benchmem set stringvalue='c' where stringkey='test3_mem10_7';"
        + "update benchmem set stringvalue='c' where stringkey='test3_mem10_8';"
        + "update benchmem set stringvalue='c' where stringkey='test3_mem10_9';");
    sw.Stop();
    Console.WriteLine($"test 10: {sw.Elapsed.TotalMilliseconds} ms");

    sw = Stopwatch.StartNew();
    for (int i = 0; i < 100; ++i)
        ctx.Database.ExecuteSqlRaw($"update benchmem set stringvalue='c' where stringkey='test3_mem100_{i}'");
    sw.Stop();
    Console.WriteLine($"test 100: {sw.Elapsed.TotalMilliseconds} ms");
}
else
{
    ctx.Database.ExecuteSqlRaw("insert into benchmem values('warmup3','x')");

    var sw = Stopwatch.StartNew();
    ctx.Database.ExecuteSqlRaw("insert into benchmem values('test3_mem1_1', 'c')");
    sw.Stop();
    Console.WriteLine($"test 1: {sw.Elapsed.TotalMilliseconds} ms");

    sw = Stopwatch.StartNew();
    ctx.Database.ExecuteSqlRaw("insert into benchmem values('test3_mem10_0', 'C')");
    ctx.Database.ExecuteSqlRaw("insert into benchmem values('test3_mem10_1', 'C')");
    ctx.Database.ExecuteSqlRaw("insert into benchmem values('test3_mem10_2', 'C')");
    ctx.Database.ExecuteSqlRaw("insert into benchmem values('test3_mem10_3', 'C')");
    ctx.Database.ExecuteSqlRaw("insert into benchmem values('test3_mem10_4', 'C')");
    ctx.Database.ExecuteSqlRaw("insert into benchmem values('test3_mem10_5', 'C')");
    ctx.Database.ExecuteSqlRaw("insert into benchmem values('test3_mem10_6', 'C')");
    ctx.Database.ExecuteSqlRaw("insert into benchmem values('test3_mem10_7', 'C')");
    ctx.Database.ExecuteSqlRaw("insert into benchmem values('test3_mem10_8', 'C')");
    ctx.Database.ExecuteSqlRaw("insert into benchmem values('test3_mem10_9', 'C')");
    sw.Stop();
    Console.WriteLine($"test 10: {sw.Elapsed.TotalMilliseconds} ms");

    sw = Stopwatch.StartNew();
    for (int i = 0; i < 100; ++i)
        ctx.Database.ExecuteSqlRaw($"insert into benchmem values('test3_mem100_{i}', 'c')");
    sw.Stop();
    Console.WriteLine($"test 100: {sw.Elapsed.TotalMilliseconds} ms");
}

Environment.Exit(0);

public class memdbcontext : DbContext
{
    public memdbcontext(DbContextOptions<memdbcontext> options) : base(options)
    {
    }

    public memdbcontext() { }

    public DbSet<memrecord> memrecords { get; set; }
}
[Keyless]
public class memrecord
{
    public string stringkey { get; set; }
    public string stringvalue { get; set; }
}
*/
