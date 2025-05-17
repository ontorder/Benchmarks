using BenchmarkDotNet.Attributes;
using System.Collections.Generic;

namespace test;

[MemoryDiagnoser]
public class bench_iterator_last
{
    static readonly string[] collection = new string[] {
        "aaaa", "afsfsf", "rghrth", "w5by" ,"wnyw46wunw",
        "wnu6nuw6un", "w6nuwnu6n", "w6unwn", "w6unwnw", "w6nuwun" };

    [Benchmark]
    public string last_inner()
    {
        var lastword = string.Empty;
        foreach (var word in collection.WithLast_InnerMethod()) lastword = word.Item;
        return lastword;
    }

    [Benchmark]
    public string last_ref()
    {
        var lastword = string.Empty;
        foreach (var word in collection.WithLast_InnerRef()) lastword = word.Item;
        return lastword;
    }

    //[Benchmark]
    public string last_inlined()
    {
        var lastword = string.Empty;
        foreach (var word in collection.WithLast_Inlined()) lastword = word.Item;
        return lastword;
    }

    //[Benchmark]
    public string last_variables()
    {
        var lastword = string.Empty;
        foreach (var word in collection.WithLast_Variables()) lastword = word.Item;
        return lastword;
    }
}
file static class LocalExt
{
    public static IEnumerable<(bool IsLast, T Item)> WithLast_InnerRef<T>(this IEnumerable<T> collection)
        where T : class
    {
        var enumerator = collection.GetEnumerator();

        var itemN0 = GetNext(ref enumerator);
        if (itemN0 == null)
            yield break;

        T? itemN1;
        do
        {
            itemN1 = GetNext(ref enumerator);
            yield return (itemN1 == null, itemN0!);
            itemN0 = itemN1;
        }
        while (itemN1 != null);

        enumerator.Dispose();

        static T? GetNext(ref readonly IEnumerator<T> e)
            => e.MoveNext()
                ? e.Current
                : null;
    }

    public static IEnumerable<(bool IsLast, T Item)> WithLast_InnerMethod<T>(this IEnumerable<T> collection)
        where T : class
    {
        using var enumerator = collection.GetEnumerator();

        var itemN0 = GetNext();
        if (itemN0 == null)
            yield break;

        T? itemN1;
        do
        {
            itemN1 = GetNext();
            yield return (itemN1 == null, itemN0!);
            itemN0 = itemN1;
        }
        while (itemN1 != null);

        T? GetNext()
            => enumerator.MoveNext()
                ? enumerator.Current
                : null;
    }

    public static IEnumerable<(bool IsLast, T Item)> WithLast_Inlined<T>(this IEnumerable<T> collection)
        where T : class
    {
        using var enumerator = collection.GetEnumerator();

        var itemN0 = enumerator.MoveNext() ? enumerator.Current : default;
        if (itemN0 != null)
            yield break;

        T? itemN1;
        do
        {
            itemN1 = enumerator.MoveNext() ? enumerator.Current : default;
            yield return (itemN1 == null, itemN0);
            itemN0 = itemN1;
        }
        while (itemN1 != null);
    }

    public static IEnumerable<(bool IsLast, T Item)> WithLast_Variables<T>(this IEnumerable<T> collection)
        where T : class
    {
        using var enumerator = collection.GetEnumerator();

        var itemN0exists = enumerator.MoveNext();
        if (itemN0exists)
            yield break;
        var itemN0 = enumerator.Current;

        bool itemN1exists;
        do
        {
            itemN1exists = enumerator.MoveNext();
            yield return (itemN1exists == false, itemN0);
            if (itemN1exists) itemN0 = enumerator.Current;
        }
        while (itemN1exists);
    }

    public static IEnumerable<(bool IsLast, T Item)> WithLastValue<T>(this IEnumerable<T> collection)
        where T : struct
    {
        using var enumerator = collection.GetEnumerator();

        var itemN0 = GetNext();
        if (itemN0.Exists == false)
            yield break;

        (bool Exists, T Item) itemN1;
        do
        {
            itemN1 = GetNext();
            yield return (itemN1.Exists == false, itemN0.Item);
            itemN0 = itemN1;
        }
        while (itemN1.Exists != false);

        (bool Exists, T Item) GetNext()
            => (enumerator.MoveNext(), enumerator.Current);
    }
}
/*
| Method         | Mean      | Error    | StdDev   | Gen0   | Allocated |
|--------------- |----------:|---------:|---------:|-------:|----------:|
| last_inner     | 152.8 ns  | 0.85  ns | 0.75 ns  | 0.0165 |     104 B |
| last_ref       | 105.6  ns | 0.26  ns | 0.25  ns | 0.0166 |     104 B |
| last_inlined   |  28.39 ns | 0.123 ns | 0.109 ns | 0.0166 |     104 B |
| last_variables |  25.52 ns | 0.165 ns | 0.155 ns | 0.0179 |     112 B |
*/
