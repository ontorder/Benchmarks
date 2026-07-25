using BenchmarkDotNet.Attributes;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace test;

[MemoryDiagnoser]
public class bench_binprim_stj
{
    private static readonly byte[] JsonData;
    private static readonly byte[] BinaryData;

    static bench_binprim_stj()
    {
        var value = new TestData(
            123456789,
            9_876_543_210_123_456,
            new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc),
            new DateTime(2025, 6, 7, 8, 9, 10, DateTimeKind.Local),
            "Hello, System.Text.Json",
            "Hello, BinaryReader");

        JsonData = JsonSerializer.SerializeToUtf8Bytes(value, options: new JsonSerializerOptions() { WriteIndented = false });

        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(value.IntValue);
            writer.Write(value.LongValue);
            writer.Write(value.FirstDateTime.Ticks);
            writer.Write(value.SecondDateTime.Ticks);
            writer.Write(value.FirstString);
            writer.Write(value.SecondString);
        }

        BinaryData = stream.ToArray();
    }

    [Benchmark(Baseline = true)]
    public TestData stj() => JsonSerializer.Deserialize<TestData>(JsonData)!;

    [Benchmark()]
    public TestDataSg stj_sg() => JsonSerializer.Deserialize(JsonData, BenchBinprimStjJsonContext.Default.TestDataSg)!;

    [Benchmark]
    public TestData bin()
    {
        using var stream = new MemoryStream(BinaryData, writable: false);
        using var reader = new BinaryReader(stream, Encoding.UTF8);

        return new TestData(
            reader.ReadInt32(),
            reader.ReadInt64(),
            new(reader.ReadInt64()),
            new(reader.ReadInt64()),
            reader.ReadString(),
            reader.ReadString());
    }
}

public sealed class TestData(
    int intValue,
    long longValue,
    DateTime firstDateTime,
    DateTime secondDateTime,
    string firstString,
    string secondString)
{
    public int IntValue { get; } = intValue;
    public long LongValue { get; } = longValue;
    public DateTime FirstDateTime { get; } = firstDateTime;
    public DateTime SecondDateTime { get; } = secondDateTime;
    public string FirstString { get; } = firstString;
    public string SecondString { get; } = secondString;
}

public sealed class TestDataSg(
    int intValue,
    long longValue,
    DateTime firstDateTime,
    DateTime secondDateTime,
    string firstString,
    string secondString)
{
    public int IntValue { get; } = intValue;
    public long LongValue { get; } = longValue;
    public DateTime FirstDateTime { get; } = firstDateTime;
    public DateTime SecondDateTime { get; } = secondDateTime;
    public string FirstString { get; } = firstString;
    public string SecondString { get; } = secondString;
}

[JsonSerializable(typeof(TestDataSg))]
internal partial class BenchBinprimStjJsonContext : JsonSerializerContext
{
}

/*

| net10  | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|------- |----------:|---------:|---------:|------:|-------:|----------:|------------:|
| bin    |  97.25 ns | 0.629 ns | 0.588 ns |  0.13 | 0.0509 |     320 B |        0.91 |
| stj    | 739.07 ns | 4.708 ns | 4.404 ns |  1.00 | 0.0553 |     352 B |        1.00 |

| net10  | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|------- |----------:|---------:|---------:|------:|-------:|----------:|------------:|
| bin    |  99.17 ns | 0.367 ns | 0.343 ns |  0.13 | 0.0509 |     320 B |        0.91 |
| stj_sg | 723.95 ns | 5.083 ns | 4.755 ns |  0.99 | 0.0553 |     352 B |        1.00 |
| stj    | 734.63 ns | 2.436 ns | 2.159 ns |  1.00 | 0.0553 |     352 B |        1.00 |

*/
