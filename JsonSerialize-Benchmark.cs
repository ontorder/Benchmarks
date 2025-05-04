using BenchmarkDotNet.Attributes;
using System;
using System.Buffers;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace test;

[MemoryDiagnoser]
public class bench_json
{
    static int value1;
    static string value2;
    static double value3;

    static bench_json()
    {
        value1 = Random.Shared.Next();
        value2 = value1.ToString();
        value3 = Random.Shared.NextDouble();
    }

    //[Benchmark]
    public string textjson()
    {
        var obj = new
        {
            value1,
            value2,
            value3,
            value4 = (object?)null
        };
        return System.Text.Json.JsonSerializer.Serialize(obj);
    }

    //[Benchmark]
    public string stringjson()
    {
        var escaped = value2.Replace("\"", "\\\"");
        var nullValue = "null";
        var doubleSerialized = value3.ToString(CultureInfo.InvariantCulture);
        return $$"""
            {
                "value1": {{value1}},
                "value2": "{{escaped}}",
                "value3": {{doubleSerialized}},
                "value4": {{nullValue}}
            }
            """;
    }

    const string jsonvalue1 = "{\n    \"value1\": ";
    const string jsonvalue2 = ",\n    \"value2\": \"";
    const string jsonvalue3 = "\",\n    \"value3\": ";
    const string jsonvalue4 = ",\n    \"value4\": ";
    const string jsonepil = "\n}";
    const string initialtemplate = jsonvalue1 + jsonvalue2 + jsonvalue3 + jsonvalue4 + jsonepil;

    //[Benchmark]
    public string sbjson()
    {
        Span<char> v1mem = stackalloc char[11];
        var v1ok = value1.TryFormat(v1mem, out var value1len);
        if (v1ok == false) throw new Exception();
        Span<char> v1str = v1mem[..value1len];

        var toEscape = 0;
        for (int charId = 0; charId < value2.Length; ++charId) if (value2[charId] == '"') ++toEscape;
        var escValue2Len = value2.Length + toEscape;
        var v2escaped = escValue2Len == value2.Length ? value2.AsSpan() : value2.Replace("\"", "\\\"").AsSpan();

        Span<char> value3mem = stackalloc char[20];
        var ok = value3.TryFormat(value3mem, out var value3len, default, CultureInfo.InvariantCulture);
        if (ok == false) throw new Exception();
        var value3str = value3mem[..value3len];

        var nullValue = "null";

        return new StringBuilder(initialtemplate.Length + value1len + escValue2Len + value3len + nullValue.Length)
            .Append("{\n    \"value1\": ")
            .Append(v1str)
            .Append(",\n    \"value2\": \"")
            .Append(v2escaped)
            .Append("\",\n    \"value3\": ")
            .Append(value3str)
            .Append(",\n    \"value4\": ")
            .Append(nullValue)
            .Append("\n}")
            .ToString();
    }

    [Benchmark]
    public string charjsonslice()
    {
        Span<char> value1mem = stackalloc char[11];
        var v1ok = value1.TryFormat(value1mem, out var value1len);
        if (v1ok == false) throw new Exception();
        var v1str = value1mem[..value1len];

        var toEscape = 0;
        for (int charId = 0; charId < value2.Length; ++charId) if (value2[charId] == '"') ++toEscape;
        var escValue2Len = value2.Length + toEscape;
        var v2str = escValue2Len == value2.Length ? value2.AsSpan() : value2.Replace("\"", "\\\"").AsSpan();

        Span<char> value3mem = stackalloc char[20];
        var ok = value3.TryFormat(value3mem, out var value3len, default, CultureInfo.InvariantCulture);
        if (ok == false) throw new Exception();
        var v3str = value3mem[..value3len];

        var nullValue = "null";
        var totsize = initialtemplate.Length + value1len + escValue2Len + value3len + nullValue.Length;
        Span<char> json = stackalloc char[totsize];
        int start;
        jsonvalue1.CopyTo(json); start = jsonvalue1.Length;
        v1str.CopyTo(json.Slice(start, value1len)); start += value1len;
        jsonvalue2.CopyTo(json.Slice(start, jsonvalue2.Length)); start += jsonvalue2.Length;
        v2str.CopyTo(json.Slice(start, escValue2Len)); start += escValue2Len;
        jsonvalue3.CopyTo(json.Slice(start, jsonvalue3.Length)); start += jsonvalue3.Length;
        v3str.CopyTo(json.Slice(start, value3len)); start += value3len;
        jsonvalue4.CopyTo(json.Slice(start, jsonvalue4.Length)); start += jsonvalue4.Length;
        nullValue.CopyTo(json.Slice(start, nullValue.Length)); start += nullValue.Length;
        jsonepil.CopyTo(json.Slice(start, jsonepil.Length));
        return new string(json);
    }

    //[Benchmark]
    public string charjsonrange()
    {
        Span<char> value1mem = stackalloc char[11];
        var v1ok = value1.TryFormat(value1mem, out var value1len);
        if (v1ok == false) throw new Exception();
        var v1str = value1mem[..value1len];

        var toEscape = 0;
        for (int charId = 0; charId < value2.Length; ++charId) if (value2[charId] == '"') ++toEscape;
        var escValue2Len = value2.Length + toEscape;
        var v2str = escValue2Len == value2.Length ? value2.AsSpan() : value2.Replace("\"", "\\\"").AsSpan();

        Span<char> value3mem = stackalloc char[20];
        var ok = value3.TryFormat(value3mem, out var value3len, default, CultureInfo.InvariantCulture);
        if (ok == false) throw new Exception();
        var v3str = value3mem[..value3len];

        var nullValue = "null";

        Span<char> json = stackalloc char[initialtemplate.Length + value1len + escValue2Len + value3len + nullValue.Length];
        int start;
        jsonvalue1.CopyTo(json); start = jsonvalue1.Length;
        v1str.CopyTo(json[start..(start + value1len)]); start += value1len;
        jsonvalue2.CopyTo(json[start..(start + jsonvalue2.Length)]); start += jsonvalue2.Length;
        v2str.CopyTo(json[start..(start + escValue2Len)]); start += escValue2Len;
        jsonvalue3.CopyTo(json[start..(start + jsonvalue3.Length)]); start += jsonvalue3.Length;
        v3str.CopyTo(json[start..(start + value3len)]); start += value3len;
        jsonvalue4.CopyTo(json[start..(start + jsonvalue4.Length)]); start += jsonvalue4.Length;
        nullValue.CopyTo(json[start..(start + nullValue.Length)]); start += nullValue.Length;
        jsonepil.CopyTo(json[start..(start + jsonepil.Length)]);
        return new string(json);
    }

    //[Benchmark]
    public string charjsonfor()
    {
        Span<char> value1mem = stackalloc char[11];
        var v1ok = value1.TryFormat(value1mem, out var value1len);
        if (v1ok == false) throw new Exception();
        var v1str = value1mem[..value1len];

        int charId = 0;
        var toEscape = 0;
        var v2mem = value2.AsSpan();
        for (; charId < v2mem.Length; ++charId) if (v2mem[charId] == '"') ++toEscape;
        var escValue2Len = v2mem.Length + toEscape;
        var v2str = escValue2Len == value2.Length ? value2.AsSpan() : value2.Replace("\"", "\\\"").AsSpan();

        Span<char> value3mem = stackalloc char[20];
        var ok = value3.TryFormat(value3mem, out var value3len, default, CultureInfo.InvariantCulture);
        if (ok == false) throw new Exception();
        var v3str = value3mem[..value3len];

        var v4str = "null".AsSpan();

        Span<char> json = stackalloc char[initialtemplate.Length + value1len + escValue2Len + value3len + v4str.Length];
        int start = 0; charId = 0; while (charId < jsonvalue1.Length) { json[start + charId] = jsonvalue1[charId]; ++charId; }
        start = jsonvalue1.Length; charId = 0; while (charId < value1len) { json[start + charId] = v1str[charId]; ++charId; }
        start += value1len; charId = 0; while (charId < jsonvalue2.Length) { json[start + charId] = jsonvalue2[charId]; ++charId; }
        start += jsonvalue2.Length; charId = 0; while (charId < escValue2Len) { json[start + charId] = v2str[charId]; ++charId; }
        start += escValue2Len; charId = 0; while (charId < jsonvalue3.Length) { json[start + charId] = jsonvalue3[charId]; ++charId; }
        start += jsonvalue3.Length; charId = 0; while (charId < value3len) { json[start + charId] = v3str[charId]; ++charId; }
        start += value3len; charId = 0; while (charId < jsonvalue4.Length) { json[start + charId] = jsonvalue4[charId]; ++charId; }
        start += jsonvalue4.Length; charId = 0; while (charId < v4str.Length) { json[start + charId] = v4str[charId]; ++charId; }
        start += v4str.Length; charId = 0; while (charId < jsonepil.Length) { json[start + charId] = jsonepil[charId]; ++charId; }
        return new string(json);
    }

    [Benchmark]
    public string charjsonctor()
    {
        value1 = 123;
        var value1len = 3;

        var toEscape = 0;
        var v2mem = value2.AsSpan();
        for (int tempId = 0; tempId < v2mem.Length; ++tempId) if (v2mem[tempId] == '"') ++toEscape;
        var escValue2Len = v2mem.Length + toEscape;

        value3 = 123.4;
        var value3len = 5;

        const string nullValue = "null";

        var totlen = initialtemplate.Length + value1len + escValue2Len + value3len + nullValue.Length;

        var sa = new SpanAction<char, byte>((json, st) =>
        {
            Span<char> value1mem = stackalloc char[11];
            var v1ok = value1.TryFormat(value1mem, out var value1len);
            var v1str = value1mem[..value1len];

            var v2str = escValue2Len == value2.Length ? value2.AsSpan() : value2.Replace("\"", "\\\"").AsSpan();

            Span<char> value3mem = stackalloc char[20];
            var ok = value3.TryFormat(value3mem, out var value3len, default, CultureInfo.InvariantCulture);
            var v3str = value3mem[..value3len];

            var v4str = nullValue.AsSpan();

            int start;
            jsonvalue1.CopyTo(json); start = jsonvalue1.Length;
            v1str.CopyTo(json.Slice(start, value1len)); start += value1len;
            jsonvalue2.CopyTo(json.Slice(start, jsonvalue2.Length)); start += jsonvalue2.Length;
            v2str.CopyTo(json.Slice(start, escValue2Len)); start += escValue2Len;
            jsonvalue3.CopyTo(json.Slice(start, jsonvalue3.Length)); start += jsonvalue3.Length;
            v3str.CopyTo(json.Slice(start, value3len)); start += value3len;
            jsonvalue4.CopyTo(json.Slice(start, jsonvalue4.Length)); start += jsonvalue4.Length;
            v4str.CopyTo(json.Slice(start, v4str.Length)); start += nullValue.Length;
            jsonepil.CopyTo(json.Slice(start, jsonepil.Length));
        });
        return string.Create(totlen, (byte)0, sa);
    }
}
/*
| Method        | Mean     | Error   | StdDev  | Gen0   | Allocated |
|-----------    |---------:|--------:|--------:|-------:|----------:|
| charjsonctor  | 150.0 ns | 1.08 ns | 1.01 ns | 0.0458 |     288 B |
| charjsonslice | 199.2 ns | 0.77 ns | 0.72 ns | 0.0381 |     240 B |
| charjsonrange | 205.9 ns | 1.00 ns | 0.89 ns | 0.0381 |     240 B |
| sbjson        | 236.2 ns | 1.10 ns | 0.98 ns | 0.0863 |     544 B |
| stringjson    | 237.6 ns | 0.80 ns | 0.75 ns | 0.0482 |     304 B |
| charjsonfor   | 272.1 ns | 1.58 ns | 1.47 ns | 0.0381 |     240 B |
| textjson      | 447.7 ns | 2.46 ns | 2.30 ns | 0.0381 |     240 B |
*/
