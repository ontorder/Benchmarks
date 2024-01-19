using System;

namespace test;

[BenchmarkDotNet.Attributes.MemoryDiagnoser]
public class exceptionflowperf
{
    bool out_b;

    public exceptionflowperf() => out_b = Random.Shared.Next() % 2 == 0;

    [BenchmarkDotNet.Attributes.Benchmark]
    public void RegularFlow()
    {
        var v1 = R1(2646);
        switch (v1)
        {
            case RegularEnum.None:
                var c = R2();
                if (!c)
                    empt("3");
                else
                    empt("4");
                break;

            case RegularEnum.Stuff:
                var b = R2();
                if (b)
                    empt("2");
                else
                    empt("1");
                break;

            default:
                break;
        }
    }

    private RegularEnum R1(int v) => out_b ? RegularEnum.Stuff : RegularEnum.None;

    public bool R2() => out_b;

    [BenchmarkDotNet.Attributes.Benchmark]
    public void ExceptionFlow()
    {
        try
        {
            Choice();
        }
        catch (ExceptionResult1 r1)
        {
            empt(r1.X);
        }
        catch (ExceptionResult2 r2)
        {
            r2.Y++;
            empt("");
        }
    }

    private void Choice()
    {
        if (out_b) throw new ExceptionResult1 { X = "z" };
        throw new ExceptionResult2() { Y = 5 };
    }

    [BenchmarkDotNet.Attributes.Benchmark]
    public void ObjectFlow()
    {
        switch (give_object())
        {
            case (bool _, string s):
                empt(s);
                break;

            case Exception a:
                empt("e");
                break;

            default:
                break;
        }
    }

    public void empt(string s) { }

    private object give_object() => out_b ? (true, "") : new Exception();

    class ExceptionResult1 : Exception { public string X; }
    class ExceptionResult2 : Exception { public int Y; }

    enum RegularEnum
    {
        None,
        Stuff,
    }
}
