[MemoryDiagnoser]
public class bench_double_deserz
{
    private const string json = @"{""discr"":""aaaaaaaaa"",""sample_datetime"":""2026-05-02T20:55:38.7322853+02:00"",""sample_int"":12345,""sample_string"":""test""}";
    private const string json_type_last_field = @"{""sample_datetime"":""2026-05-02T20:55:38.7322853+02:00"",""sample_int"":12345,""sample_string"":""test"",""discr"":""aaaaaaaaa""}";
    private const string json_envelope = @"{""discr"":""aaaaaaaaa"",""content"":{""sample_datetime"":""2026-05-02T20:55:38.7322853+02:00"",""sample_int"":12345,""sample_string"":""test""}}";
    private const string json_simpler = @"{""typefield"":{""sample_datetime"":""2026-05-02T20:55:38.7322853+02:00"",""sample_int"":12345,""sample_string"":""test""}}";

    //[Benchmark]
    public object pure_double_reference()
    {
        var discr = JsonSerializer.Deserialize<ModelType>(json);
        var temp = JsonSerializer.Deserialize<ModelData>(json);
        return (discr, temp);
    }

    static readonly JsonSerializerOptions DeserializeFields = new() { IncludeFields = true };

    //[Benchmark]
    public object pure_double_field()
    {
        var discr = JsonSerializer.Deserialize<ModelTypeField>(json, DeserializeFields);
        var temp = JsonSerializer.Deserialize<ModelFields>(json, DeserializeFields);
        return (discr, temp);
    }

    //[Benchmark]
    public object via_jsondoc()
    {
        var temp = JsonSerializer.Deserialize<JsonDocument>(json);
        var discr = JsonSerializer.Deserialize<ModelType>(temp);
        var data = JsonSerializer.Deserialize<ModelData>(temp);
        return (discr, data);
    }

    //[Benchmark]
    public object discrim_last_field()
    {
        var discr = JsonSerializer.Deserialize<ModelType>(json_type_last_field);
        var temp = JsonSerializer.Deserialize<ModelData>(json_type_last_field);
        return (discr, temp);
    }

    //[Benchmark]
    public object regular_envelope()
    {
        var discr = JsonSerializer.Deserialize<Envelope>(json_envelope);
        var data = JsonSerializer.Deserialize<ModelData>(discr.content);
        return data;
    }

    //[Benchmark]
    public object via_utf8json()
    {
        var rom = Encoding.UTF8.GetBytes(json_envelope);
        var u8r = new Utf8JsonReader(rom, isFinalBlock: true, new JsonReaderState());
        u8r.Read();
        u8r.Read();
        u8r.Read();
        var temp = u8r.GetString();
        //var temp = JsonSerializer.Deserialize<ModelType>(ref u8r);
        u8r.Read();
        u8r.Read();
        var data = JsonSerializer.Deserialize<ModelData>(ref u8r);
        return (temp, data);
    }

    //[Benchmark]
    public object utf8json_simpler()
    {
        var rom = Encoding.UTF8.GetBytes(json_simpler);
        var u8r = new Utf8JsonReader(rom, isFinalBlock: true, new JsonReaderState());
        u8r.Read();
        u8r.Read();
        var temp = u8r.GetString();
        //var temp = JsonSerializer.Deserialize<ModelType>(ref u8r);
        u8r.Read();
        var data = JsonSerializer.Deserialize<ModelData>(ref u8r);
        return (temp, data);
    }

    [Benchmark]
    public object via_dictionary()
    {
        var fields = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        var f1 = ((JsonElement)fields["sample_datetime"]).GetDateTime();
        var f2 = ((JsonElement)fields["sample_int"]).GetInt32();
        var f3 = ((JsonElement)fields["sample_string"]).GetString();
        var md = new ModelData()
        {
            sample_datetime = f1,
            sample_int = f2,
            sample_string = f3
        };
        return (md, ((JsonElement)fields["discr"]).GetString());
    }

    //[Benchmark]
    public object via_sortedlist()
    {
        var fields = JsonSerializer.Deserialize<SortedList<string, object>>(json);
        var md = new ModelData()
        {
            sample_datetime = ((JsonElement)fields["sample_datetime"]).GetDateTime(),
            sample_int = ((JsonElement)fields["sample_int"]).GetInt32(),
            sample_string = ((JsonElement)fields["sample_string"]).GetString()
        };
        return (md, ((JsonElement)fields["discr"]).GetString());
    }

    [Benchmark]
    public object fieldslist()
    {
        var doc = JsonDocument.Parse(json);
        var fields = doc.RootElement.EnumerateObject()
           .Select(static p => (p.Name, p.Value))
           .ToArray();
        var md = new ModelData()
        {
            sample_datetime = getfield("sample_datetime").GetDateTime(),
            sample_int = getfield("sample_int").GetInt32(),
            sample_string = getfield("sample_string").GetString()
        };
        return (md, getfield("discr").GetString());

        JsonElement getfield(string field)
        {
            foreach (var (key, content) in fields) if (key == field) return content;
            return default;
        }
    }

    [Benchmark]
    public object fieldslist2()
    {
        var doc = JsonDocument.Parse(json);
        var fields = doc.RootElement.EnumerateObject()
           .Select(static p => (p.Name, p.Value))
           .ToList();
        var md = new ModelData();
        string discr = string.Empty;
        foreach (var (key, content) in fields)
        {
            switch (key)
            {
                case "sample_datetime": md.sample_datetime = content.GetDateTime(); break;
                case "sample_int": md.sample_int = content.GetInt32(); break;
                case "sample_string": md.sample_string = content.GetString(); break;
                case "discr": break;
            }
        }
        return (md, discr);
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

round 1
| Method                | Mean       | Error    | StdDev   | Gen0   | Allocated |
|---------------------- |-----------:|---------:|---------:|-------:|----------:|
| discrim_last_field    |   780.7 ns |  4.32 ns |  4.04 ns | 0.0267 |     168 B |
| pure_double_reference |   792.0 ns |  1.51 ns |  1.41 ns | 0.0267 |     168 B |
| pure_double_field     |   803.4 ns | 11.91 ns | 10.56 ns | 0.0267 |     168 B |
| regular_envelope      | 1,060.0 ns |  6.44 ns |  6.02 ns | 0.0725 |     472 B |
| via_jsondoc           | 1,322.1 ns |  8.92 ns |  8.34 ns | 0.0839 |     544 B |

round 2
| Method                | Mean       | Error    | StdDev   | Gen0   | Allocated |
|---------------------- |-----------:|---------:|---------:|-------:|----------:|
| via_utf8json          |   626.3 ns |  2.38 ns |  2.23 ns | 0.0477 |     304 B |
| discrim_last_field    |   779.8 ns |  3.52 ns |  3.29 ns | 0.0267 |     168 B |
| pure_double_field     |   780.2 ns | 10.90 ns | 10.19 ns | 0.0267 |     168 B |
| pure_double_reference |   798.2 ns |  2.67 ns |  2.50 ns | 0.0267 |     168 B |
| regular_envelope      | 1,037.3 ns |  4.41 ns |  3.91 ns | 0.0725 |     472 B |
| via_jsondoc           | 1,335.5 ns | 11.56 ns | 10.81 ns | 0.0839 |     544 B |

round 3
| Method                | Mean       | Error   | StdDev  | Gen0   | Allocated |
|---------------------- |-----------:|--------:|--------:|-------:|----------:|
| utf8json_simpler      |   602.3 ns | 4.01 ns | 3.56 ns | 0.0439 |     280 B |
| via_utf8json          |   628.7 ns | 2.97 ns | 2.77 ns | 0.0477 |     304 B |
| pure_double_field     |   781.7 ns | 2.45 ns | 2.05 ns | 0.0267 |     168 B |
| pure_double_reference |   794.0 ns | 2.53 ns | 2.24 ns | 0.0267 |     168 B |
| discrim_last_field    |   796.0 ns | 3.20 ns | 2.99 ns | 0.0267 |     168 B |
| regular_envelope      | 1,043.4 ns | 4.42 ns | 4.14 ns | 0.0725 |     472 B |
| via_jsondoc           | 1,333.5 ns | 6.84 ns | 6.06 ns | 0.0839 |     544 B |

round 4
| Method           | Mean       | Error   | StdDev  | Gen0   | Allocated |
|----------------- |-----------:|--------:|--------:|-------:|----------:|
| utf8json_simpler |   596.0 ns | 3.42 ns | 3.20 ns | 0.0439 |     280 B |
| via_utf8json     |   649.5 ns | 2.07 ns | 1.83 ns | 0.0477 |     304 B |
| via_dictionary   | 1,075.0 ns | 2.68 ns | 2.38 ns | 0.2537 |    1592 B |
| via_sortedlist   | 1,625.5 ns | 6.19 ns | 5.79 ns | 0.2060 |    1304 B | <-- fuck you

round 5
| Method           | Mean       | Error   | StdDev  | Gen0   | Allocated |
|----------------- |-----------:|--------:|--------:|-------:|----------:|
| utf8json_simpler |   600.6 ns | 3.13 ns | 2.93 ns | 0.0439 |     280 B |
| via_utf8json     |   649.6 ns | 3.92 ns | 3.47 ns | 0.0458 |     304 B |
| fieldslist       |   698.5 ns | 3.38 ns | 3.17 ns | 0.1554 |     976 B |
| via_dictionary   | 1,067.2 ns | 4.12 ns | 3.65 ns | 0.2537 |    1592 B |
| via_sortedlist   | 1,609.4 ns | 5.76 ns | 5.39 ns | 0.2060 |    1304 B |

round 6
| Method         | Mean       | Error   | StdDev  | Gen0   | Allocated |
|--------------- |-----------:|--------:|--------:|-------:|----------:|
| fieldslist2    |   627.2 ns | 5.27 ns | 4.67 ns | 0.1535 |     968 B |
| fieldslist     |   651.9 ns | 3.90 ns | 3.65 ns | 0.1554 |     976 B |
| via_dictionary | 1,043.4 ns | 2.88 ns | 2.41 ns | 0.2537 |    1592 B |

*/
