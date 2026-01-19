[MemoryDiagnoser]
public class assholestj
{
    static private sertest main = new sertest(f1: DateTimeOffset.Now, l: 2L, d: "ddd");
    static private object boxed = main;
    static private porcamad interf = main;
    private interface porcamad { DateTimeOffset f1 { get; } }
    private sealed record sertest(DateTimeOffset f1, long l, string d) : porcamad;

    [Benchmark]
    public string rawtype() => System.Text.Json.JsonSerializer.Serialize(main);

    [Benchmark]
    public string justobject() => System.Text.Json.JsonSerializer.Serialize(boxed);

    [Benchmark]
    public string viainterface() => System.Text.Json.JsonSerializer.Serialize(interf);
}
/*

| Method       | Mean     | Error   | StdDev  | Gen0   | Allocated |
|------------- |---------:|--------:|--------:|-------:|----------:|
| viainterface | 193.2 ns | 1.27 ns | 1.13 ns | 0.0176 |     112 B |
| justobject   | 285.0 ns | 1.74 ns | 1.54 ns | 0.0229 |     144 B |
| rawtype      | 285.7 ns | 3.75 ns | 3.51 ns | 0.0229 |     144 B |

*/
