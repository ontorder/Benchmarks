using BenchmarkDotNet.Attributes;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Intrinsics;

namespace test;

[MemoryDiagnoser]
public class bench_ip_eq
{
    static bench_ip_eq()
    {
        foreach (var (ip, _) in _whitelistRaw)
        {
            var parsed = IPAddress.Parse(ip);
            var b6 = parsed.MapToIPv6().GetAddressBytes();
            _whitelistIpAddr.Add(parsed);
            _whitelistByte.Add(b6);
            _whitelistString.Add(ip);
            _whitelistHash.Add(BinaryPrimitives.ReadUInt128LittleEndian(b6));
            _whitelistHashIp.Add(parsed);
            _whitelistV128.Add(Vector128.LoadUnsafe(ref b6[0]));
            _whitelist2long.Add((BinaryPrimitives.ReadUInt64LittleEndian(b6), BinaryPrimitives.ReadUInt64LittleEndian(b6.AsSpan()[..^8])));
        }
    }

    private static IPAddress testip = IPAddress.Parse("123.123.123.123");

    //[Benchmark]
    public bool test_set_ip()
    {
        return _whitelistHashIp.Contains(testip);
    }

    //[Benchmark]
    public bool test_ip_addr()
    {
        return _whitelistIpAddr.Any(_ => _.Equals(testip));
    }

    //[Benchmark]
    public bool test_bytes()
    {
        var b = testip.MapToIPv6().GetAddressBytes();
        return _whitelistByte.Any(_ => _.SequenceEqual(b));
    }

    //[Benchmark]
    public bool test_string_raw()
    {
        var s = testip.ToString();
        return _whitelistRaw.Any(_ => _.Ip == s);
    }

    //[Benchmark]
    public bool test_string_dic()
    {
        var s = testip.ToString();
        return _whitelistString.Contains(s);
    }

    //[Benchmark]
    public bool test_v128()
    {
        var b = testip.MapToIPv6().GetAddressBytes();
        var v = Vector128.LoadUnsafe(ref b[0]);
        return _whitelistV128.Contains(v);
    }

    [Benchmark]
    public bool test_2long()
    {
        var b = testip.MapToIPv6().GetAddressBytes();
        var k = (BinaryPrimitives.ReadUInt64LittleEndian(b), BinaryPrimitives.ReadUInt64LittleEndian(b.AsSpan()[..^8]));
        return _whitelist2long.Contains(k);
    }

    //[Benchmark]
    public bool test_remap_128()
    {
        var hashAsLong = BinaryPrimitives.ReadUInt128LittleEndian(testip.MapToIPv6().GetAddressBytes());
        return _whitelistHash.Contains(hashAsLong);
    }

    private static readonly List<byte[]> _whitelistByte = new();
    private static readonly HashSet<UInt128> _whitelistHash = new();
    private static readonly HashSet<IPAddress> _whitelistHashIp = new();
    private static readonly List<IPAddress> _whitelistIpAddr = new();
    private static readonly HashSet<Vector128<byte>> _whitelistV128 = new();
    private static readonly HashSet<(ulong, ulong)> _whitelist2long = new();
    private static readonly (string Ip, int MaskBits)[] _whitelistRaw = new[]
    {
        ("102.132.100.0", 24),
        ("102.132.101.0", 24),
        ("102.132.103.0", 24),
        ("102.132.104.0", 24),
        ("102.132.96.0", 20),
        //("102.132.96.0", 24),
        ("102.132.97.0", 24),
        ("102.132.99.0", 24),
        ("103.4.96.0", 22),
        ("129.134.0.0", 16),
        //("129.134.0.0", 17),
        ("129.134.112.0", 24),
        ("129.134.113.0", 24),
        ("129.134.114.0", 24),
        ("129.134.115.0", 24),
        ("129.134.127.0", 24),
        ("129.134.25.0", 24),
        ("129.134.26.0", 24),
        ("129.134.27.0", 24),
        ("129.134.28.0", 24),
        ("129.134.29.0", 24),
        ("129.134.30.0", 23),
        //("129.134.30.0", 24),
        ("129.134.31.0", 24),
        ("129.134.64.0", 24),
        ("129.134.65.0", 24),
        ("129.134.66.0", 24),
        ("129.134.67.0", 24),
        ("129.134.68.0", 24),
        ("129.134.69.0", 24),
        ("129.134.70.0", 24),
        ("129.134.71.0", 24),
        ("129.134.72.0", 24),
        ("129.134.73.0", 24),
        ("129.134.74.0", 24),
        ("129.134.75.0", 24),
        ("129.134.76.0", 24),
        ("129.134.77.0", 24),
        ("129.134.78.0", 24),
        ("129.134.79.0", 24),
        ("147.75.208.0", 20),
        //("147.75.208.0", 20),
        ("157.240.0.0", 16),
        //("157.240.0.0", 17),
        //("157.240.0.0", 24),
        ("157.240.1.0", 24),
        ("157.240.10.0", 24),
        ("157.240.100.0", 24),
        ("157.240.101.0", 24),
    };
    private static readonly HashSet<string> _whitelistString = new();
}
/*

| Method          | Mean         | Error     | StdDev    | Gen0   | Allocated |
|---------------- |-------------:|----------:|----------:|-------:|----------:|
| test_set_ip     |     6.259 ns | 0.0486 ns | 0.0431 ns |      - |         - |
| test_string_dic |     8.993 ns | 0.1013 ns | 0.0898 ns |      - |         - |
| test_remap_128  |    27.81  ns | 0.1044 ns | 0.0872 ns | 0.0191 |     120 B |
| test_2long      |    28.31  ns | 0.238  ns | 0.198  ns | 0.0191 |     120 B |
| test_v128       |    57.847 ns | 0.4483 ns | 0.3974 ns | 0.0191 |     120 B |
| test_string_raw |   331.413 ns | 0.6121 ns | 0.5725 ns | 0.0191 |     120 B |
| test_ip_addr    |   414.847 ns | 3.5285 ns | 3.1279 ns | 0.0062 |      40 B |
| test_bytes      | 1,263.894 ns | 9.4156 ns | 7.8625 ns | 0.0381 |     248 B |

*/
