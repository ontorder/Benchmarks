using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace test;

[MemoryDiagnoser]
public class test_switch_type
{
    private readonly Dictionary<string, Delegate> _stringDelegate;
    private readonly Dictionary<Type, Delegate> _typeDelegate;

    private testmatch0 _source = new();
    private testmatch2 _target = new();

    public test_switch_type()
    {
        _stringDelegate = new() {
            { "test.test_switch_type+testmatch1", map1 },
            { "test.test_switch_type+testmatch2", map2 },
            { "test.test_switch_type+testmatch3", map3 },
        };

        _typeDelegate = new() {
            { typeof(testmatch1), map1 },
            { typeof(testmatch2), map2 },
            { typeof(testmatch3), map3 },
        };
    }

    [Benchmark]
    public testmatch0 match_type_dic_string()
    {
        var mapper = (Func<testmatch0, testmatch2>)_stringDelegate[_target.GetType().FullName];
        return mapper(_source);
    }

    [Benchmark]
    public testmatch0 match_type_dic_typeof()
    {
        var mapper = (Func<testmatch0, testmatch2>)_typeDelegate[_target.GetType()];
        return mapper(_source);
    }

    [Benchmark]
    public testmatch0 match_type_is_type()
    {
        return (testmatch0)_target switch
        {
            testmatch1 _ => map1(_source),
            testmatch2 _ => map2(_source),
            testmatch3 _ => map3(_source),
        };
    }

    private testmatch1 map1(testmatch0 x) => new testmatch1();
    private testmatch2 map2(testmatch0 x) => new testmatch2();
    private testmatch3 map3(testmatch0 x) => new testmatch3();

    public class testmatch0 { }
    public class testmatch1 : testmatch0 { }
    public class testmatch2 : testmatch0 { }
    public class testmatch3 : testmatch0 { }
}
/*
| Method                | Mean      | Error     | StdDev    | Gen0   | Allocated |
|---------------------- |----------:|----------:|----------:|-------:|----------:|
| match_type_dic_string | 48.256 ns | 0.1216 ns | 0.1138 ns | 0.0038 |      24 B |
| match_type_dic_typeof | 18.735 ns | 0.0743 ns | 0.0695 ns | 0.0038 |      24 B |
| match_type_is_type    |  6.581 ns | 0.0537 ns | 0.0502 ns | 0.0038 |      24 B |
*/
