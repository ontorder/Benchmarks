[MemoryDiagnoser]
public class bench_double_deserz
{
    private const string json = @"{""discr"":""aaaaaaaaa"",""sample_datetime"":""2026-05-02T20:55:38.7322853+02:00"",""sample_int"":12345,""sample_string"":""test""}";
    private const string json_type_last_field = @"{""sample_datetime"":""2026-05-02T20:55:38.7322853+02:00"",""sample_int"":12345,""sample_string"":""test"",""discr"":""aaaaaaaaa""}";
    private const string json_envelope = @"{""discr"":""aaaaaaaaa"",""content"":{""sample_datetime"":""2026-05-02T20:55:38.7322853+02:00"",""sample_int"":12345,""sample_string"":""test""}}";

    [Benchmark]
    public object pure_double_reference()
    {
        var discr = JsonSerializer.Deserialize<ModelType>(json);
        var temp = JsonSerializer.Deserialize<ModelData>(json);
        return (discr, temp);
    }

    static readonly JsonSerializerOptions DeserializeFields = new() { IncludeFields = true };
    [Benchmark]
    public object pure_double_field()
    {
        var discr = JsonSerializer.Deserialize<ModelTypeField>(json, DeserializeFields);
        var temp = JsonSerializer.Deserialize<ModelFields>(json, DeserializeFields);
        return (discr, temp);
    }

    [Benchmark]
    public object via_jsondoc()
    {
        var temp = JsonSerializer.Deserialize<JsonDocument>(json);
        var discr = JsonSerializer.Deserialize<ModelType>(temp);
        var data = JsonSerializer.Deserialize<ModelData>(temp);
        return (discr, data);
    }

    [Benchmark]
    public object discrim_last_field()
    {
        var discr = JsonSerializer.Deserialize<ModelType>(json_type_last_field);
        var temp = JsonSerializer.Deserialize<ModelData>(json_type_last_field);
        return (discr, temp);
    }

    [Benchmark]
    public object regular_envelope()
    {
        var discr = JsonSerializer.Deserialize<Envelope>(json_envelope);
        var data = JsonSerializer.Deserialize<ModelData>(discr.content);
        return data;
    }

    //[Benchmark]
    public object via_utf8json()
    {
        var rom = Encoding.UTF8.GetBytes(json);
        var u8r = new Utf8JsonReader(rom, isFinalBlock: true, new JsonReaderState());
        var temp = JsonSerializer.Deserialize<ModelType>(ref u8r);
        var data = JsonSerializer.Deserialize<ModelData>(ref u8r);
        return data;
    }
}

public sealed class ModelType
{
    public string discr { get; set; }
}

public sealed class ModelTypeField
{
    public string discr;
}

public sealed class ModelProperties
{
    [JsonPropertyOrder(order: -1)]
    public string discr { get; set; }
    public DateTime sample_datetime { get; set; }
    public int sample_int { get; set; }
    public string sample_string { get; set; }
}

public sealed class ModelData
{
    public DateTime sample_datetime { get; set; }
    public int sample_int { get; set; }
    public string sample_string { get; set; }
}

public sealed class Envelope
{
    public string discr { get; set; }
    public JsonElement content { get; set; }
}

public sealed class ModelFields
{
    public DateTime sample_datetime;
    public int sample_int;
    public string sample_string;
}

/*

| Method                | Mean       | Error    | StdDev   | Gen0   | Allocated |
|---------------------- |-----------:|---------:|---------:|-------:|----------:|
| discrim_last_field    |   780.7 ns |  4.32 ns |  4.04 ns | 0.0267 |     168 B |
| pure_double_reference |   792.0 ns |  1.51 ns |  1.41 ns | 0.0267 |     168 B |
| pure_double_field     |   803.4 ns | 11.91 ns | 10.56 ns | 0.0267 |     168 B |
| via_jsondoc           | 1,322.1 ns |  8.92 ns |  8.34 ns | 0.0839 |     544 B |
| regular_envelope      | 1,060.0 ns |  6.44 ns |  6.02 ns | 0.0725 |     472 B |

*/
