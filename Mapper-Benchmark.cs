using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace test;

[BenchmarkDotNet.Attributes.MemoryDiagnoser()]
public class bench_my_mapper
{
    readonly SortedList<Type, object> _methods = new();
    readonly Hashtable _methods_ht = new();
    readonly Hashtable _methods_ht2 = new();
    readonly SortedList<object, object> _methods_sl = new();

    public bench_my_mapper()
    {
        _methods.Add(typeof(Func<sample_model, Task<int>>), (Func<sample_model, Task<int>>)test);
        _methods_sl.Add(typeof(Func<sample_model, Task<int>>), (Func<sample_model, Task<int>>)test);
        _methods_ht.Add(typeof(Func<sample_model, Task<int>>), (Func<sample_model, Task<int>>)test);
        _methods_ht2.Add((typeof(sample_model), typeof(int)), (Func<sample_model, Task<int>>)test);
    }

    //[BenchmarkDotNet.Attributes.Benchmark()]
    public async Task via_dynamic() => _ = await call_dynamic<int, sample_model>(new sample_model());

    //[BenchmarkDotNet.Attributes.Benchmark()]
    public async Task via_reflection() => _ = await call_reflection<int, sample_model>(new sample_model());

    [BenchmarkDotNet.Attributes.Benchmark()]
    public async Task via_cast() => _ = await call_cast<int, sample_model>(new sample_model());

    [BenchmarkDotNet.Attributes.Benchmark()]
    public async Task via_cast_hashtable() => _ = await call_cast_hashtable<int, sample_model>(new sample_model());

    [BenchmarkDotNet.Attributes.Benchmark()]
    public async Task via_cast_sortedlist() => _ = await call_cast_sortedlist<int, sample_model>(new sample_model());

    [BenchmarkDotNet.Attributes.Benchmark()]
    public async Task via_cast_tuple_key() => _ = await call_cast_sortedlist<int, sample_model>(new sample_model());

    [BenchmarkDotNet.Attributes.Benchmark()]
    public async Task via_cast_object() => _ = await call_cast_object_par<int>(new sample_model());

    // -----------------

    private async Task<TRet> call_dynamic<TRet, TPar>(TPar par)
    {
        var f = _methods[typeof(Func<TPar, Task<TRet>>)];
        var t = (Task<TRet>)((dynamic)f)(par);
        return await t;
    }

    private async Task<TRet> call_reflection<TRet, TPar>(TPar par)
    {
        var f = _methods[typeof(Func<TPar, Task<TRet>>)];
        var i = f.GetType().GetMethod("Invoke")!;
        var r = (Task<TRet>)i.Invoke(f, new object[] { par });
        return await r;
    }

    private async Task<TRet> call_cast<TRet, TPar>(TPar par)
    {
        var f = (Func<TPar, Task<TRet>>)_methods[typeof(Func<TPar, Task<TRet>>)];
        return await f(par);
    }

    private async Task<TRet> call_cast_hashtable<TRet, TPar>(TPar par)
    {
        var f = (Func<TPar, Task<TRet>>)_methods_ht[typeof(Func<TPar, Task<TRet>>)];
        return await f(par);
    }

    private async Task<TRet> call_cast_sortedlist<TRet, TPar>(TPar par)
    {
        var f = (Func<TPar, Task<TRet>>)_methods_sl[typeof(Func<TPar, Task<TRet>>)];
        return await f(par);
    }

    private async Task<TRet> call_cast_tuple_key<TRet, TPar>(TPar par)
    {
        var f = (Func<TPar, Task<TRet>>)_methods_ht2[(typeof(TPar), typeof(TRet))];
        return await f(par);
    }

    private async Task<TRet> call_cast_object_par<TRet>(object par)
    {
        var f = _methods_ht2[(par.GetType(), typeof(TRet))];
        var i = f.GetType().GetMethod("Invoke")!;
        var t = (Task)i.Invoke(f, new object[] { par });
        await t;
        return (TRet)i.ReturnType.GetProperty("Result")!.GetValue(t)!;
    }

    private Task<int> test(sample_model par1) => Task.FromResult(2);
}

public class sample_model
{
    public int id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public int count { get; set; }
    public bool is_active { get; set; }
    public bool is_deleted { get; set; }
    public string? value { get; set; }
}

/*

| Method              | Mean      | Error    | StdDev   | Gen0   | Allocated |
|---------------      |----------:|---------:|---------:|-------:|----------:|
| via_cast_hashtable  |  44.12 ns | 0.599 ns | 0.531 ns | 0.0114 |      72 B |
| via_cast_sortedlist |  55.17 ns | 0.321 ns | 0.268 ns | 0.0114 |      72 B |
| via_cast            |  59.29 ns | 1.152 ns | 1.183 ns | 0.0114 |      72 B |
| via_cast_tuple_key  |  65.29 ns | 0.399 ns | 0.374 ns | 0.0114 |      72 B |
| via_dynamic         |  87.12 ns | 1.729 ns | 2.424 ns | 0.0114 |      72 B |
| via_reflection      | 122.32 ns | 2.400 ns | 2.668 ns | 0.0165 |     104 B |
| via_cast_object     | 203.65 ns | 1.419 ns | 1.327 ns | 0.0253 |     160 B |

*/
