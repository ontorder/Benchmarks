using BenchmarkDotNet.Attributes;

namespace test;

[DisassemblyDiagnoser]
public class field_access_nanobench
{
    static fab_a fa = new fab_a();

    static field_access_nanobench()
    {
        fa.fb = new fab_b();
        fa.fb.fc = new fab_c();
        fa.fb.fc.fd = new fab_d();
        fa.fb.fc.fd.i1 = 1;
        fa.fb.fc.fd.i2 = 2;
        fa.fb.fc.fd.i3 = 36;
        fa.fb.fc.fd.i4 = 2235;
        fa.fb.fc.fd.i5 = 226;
        fa.fb.fc.fd.i6 = 2757;
        fa.fb.fc.fd.i7 = 92;
        fa.fb.fc.fd.i8 = 29;
        fa.fb.fc.fd.v1 = [234, 436, 64, 5, 57, 47, 4, 5, 67, 7];
    }

    [Benchmark]
    public int regular()
    {
        int z = fa.fb.fc.fd.i1 + fa.fb.fc.fd.i2 + fa.fb.fc.fd.i3 + fa.fb.fc.fd.i4 + fa.fb.fc.fd.i5 + fa.fb.fc.fd.i6 + fa.fb.fc.fd.i7 + fa.fb.fc.fd.i8;
        return z;
    }

    [Benchmark]
    public int ref_access()
    {
        ref var r = ref fa.fb.fc.fd;
        int z = r.i1 + r.i2 + r.i3 + r.i4 + r.i5 + r.i6 + r.i7 + r.i8;
        return z;
    }

    [Benchmark]
    public int array_access()
    {
        var a = fa.fb.fc.fd.v1;
        int z = a[0] + a[1] + a[2] + a[3] + a[4] + a[5] + a[6] + a[7];
        return z;
    }
}
public class fab_a
{
    public fab_b fb;
}
public class fab_b
{
    public fab_c fc;
}
public class fab_c
{
    public fab_d fd;
}
public class fab_d
{
    public int i1;
    public int i2;
    public int i3;
    public int i4;
    public int i5;
    public int i6;
    public int i7;
    public int i8;
    public int[] v1;
}
