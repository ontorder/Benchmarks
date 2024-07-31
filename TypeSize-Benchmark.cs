using BenchmarkDotNet.Attributes;

namespace test;

[MemoryDiagnoser]
public class bench_typesize
{
    [Benchmark]
    public void class4()
    {
        var test = new tclass4() { a = 1 };
        _ = mclass4(mclass4(mclass4(mclass4(test))));
    }

    private tclass4 mclass4(tclass4 c)
    {
        c.a += 1;
        return c;
    }

    [Benchmark]
    public void class8()
    {
        var test = new tclass8() { a = 1 };
        _ = mclass8(mclass8(mclass8(mclass8(test))));
    }

    private tclass8 mclass8(tclass8 c)
    {
        c.a = c.b;
        return c;
    }

    [Benchmark]
    public void class16()
    {
        var test = new tclass16() { a = 1 };
        _ = mclass16(mclass16(mclass16(mclass16(test))));
    }

    private tclass16 mclass16(tclass16 c)
    {
        c.d = c.c;
        return c;
    }

    [Benchmark]
    public void class32()
    {
        var test = new tclass32() { a = 1 };
        _ = mclass32(mclass32(mclass32(mclass32(test))));
    }

    private tclass32 mclass32(tclass32 c)
    {
        c.b = c.c;
        return c;
    }

    [Benchmark]
    public void class64()
    {
        var test = new tclass64() { a = 1 };
        _ = mclass64(mclass64(mclass64(mclass64(test))));
    }

    private tclass64 mclass64(tclass64 c)
    {
        c.p = c.a;
        return c;
    }

    //[Benchmark]
    public void struct4()
    {
        tstruct4 _struct4 = new();
        _ = struct4_(struct4_(struct4_(struct4_(_struct4))));
    }

    private tstruct4 struct4_(tstruct4 p)
    {
        p.a += 1;
        return p;
    }

    //[Benchmark]
    public void struct8()
    {
        tstruct8 _struct8 = new();
        _ = struct8_(struct8_(struct8_(struct8_(_struct8))));
    }

    private tstruct8 struct8_(tstruct8 p)
    {
        p.b = p.a;
        return p;
    }

    //[Benchmark]
    public void struct16()
    {
        tstruct16 _struct16 = new();
        _ = struct16_(struct16_(struct16_(struct16_(_struct16))));
    }

    private tstruct16 struct16_(tstruct16 p)
    {
        p.d = p.c;
        return p;
    }

    //[Benchmark]
    public void struct32()
    {
        tstruct32 _struct32 = new();
        _ = struct32_(struct32_(struct32_(struct32_(_struct32))));
    }

    private tstruct32 struct32_(tstruct32 p)
    {
        p.a = p.b;
        return p;
    }

    //[Benchmark]
    public void struct64()
    {
        tstruct64 _struct64 = new();
        _ = struct64_(struct64_(struct64_(struct64_(_struct64))));
    }

    private tstruct64 struct64_(tstruct64 p)
    {
        p.p = p.a;
        return p;
    }
}
class tclass4 { public int a; }
class tclass8 { public int a; public int b; }
class tclass16 { public int a; public int b; public int c; public int d; }
class tclass32 { public int a; public int b; public int c; public int d; public int e; public int f; public int g; public int h; }
class tclass64 { public int a; public int b; public int c; public int d; public int e; public int f; public int g; public int h; public int i; public int l; public int m; public int n; public int o; public int p; public int q; public int r; }
struct tstruct4 { public int a; }
struct tstruct8 { public int a; public int b; }
struct tstruct16 { public int a; public int b; public int c; public int d; }
struct tstruct32 { public int a; public int b; public int c; public int d; public int e; public int f; public int g; public int h; }
struct tstruct64 { public int a; public int b; public int c; public int d; public int e; public int f; public int g; public int h; public int i; public int l; public int m; public int n; public int o; public int p; public int q; public int r; }

/*
empty
| Method  | Mean     | Error     | StdDev    | Gen0   | Allocated |
|-------- |---------:|----------:|----------:|-------:|----------:|
| class4  | 5.291 ns | 0.0432 ns | 0.0337 ns | 0.0038 |      24 B |
| class8  | 2.672 ns | 0.0243 ns | 0.0216 ns | 0.0038 |      24 B |
| class16 | 2.846 ns | 0.0249 ns | 0.0233 ns | 0.0051 |      32 B |
| class32 | 3.544 ns | 0.0619 ns | 0.0579 ns | 0.0076 |      48 B |
| class64 | 4.877 ns | 0.1226 ns | 0.1981 ns | 0.0127 |      80 B |

struct fanno qualcosa
| Method   | Mean      | Error     | StdDev    | Median    | Allocated |
|--------- |----------:|----------:|----------:|----------:|----------:|
| struct4  | 0.0996 ns | 0.0307 ns | 0.0287 ns | 0.1012 ns |         - |
| struct8  | 0.0830 ns | 0.0285 ns | 0.0795 ns | 0.0568 ns |         - |
| struct16 | 0.0113 ns | 0.0229 ns | 0.0203 ns | 0.0000 ns |         - |
| struct32 | 0.0030 ns | 0.0083 ns | 0.0077 ns | 0.0000 ns |         - |
| struct64 | 0.0294 ns | 0.0262 ns | 0.0459 ns | 0.0006 ns |         - |
*/
