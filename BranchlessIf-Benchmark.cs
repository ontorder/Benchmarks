using BenchmarkDotNet.Attributes;
using System.Runtime.CompilerServices;

namespace test;

[MemoryDiagnoser]
[DisassemblyDiagnoser(printSource: true)]
public class bench_branchlessif
{
    private volatile bool somt = false;
    private object to_return1 = new object();
    private object to_return2 = new object();

    [Benchmark]
    public object ternary_if() => somt ? to_return1 : to_return2;

    [Benchmark]
    public unsafe object* branchless_if()
    {
        var cond = Unsafe.As<bool, int>(ref somt);
        fixed (object* ret1ptr = &to_return1)
        fixed (object* ret2ptr = &to_return2)
        {
            var resptr = (cond * (nint)ret1ptr) + ((1 - cond) * (nint)ret2ptr);
            return (object*)resptr;
        }
    }
}
/*
| Method        | Mean      | Error     | StdDev    | Code Size | Allocated |
|-------------- |----------:|----------:|----------:|----------:|----------:|
| ternary_if    | 0.7431 ns | 0.0173 ns | 0.0154 ns |      16 B |         - |
| branchless_if | 0.2668 ns | 0.0140 ns | 0.0125 ns |      60 B |         - |

## .NET 8.0.24 (8.0.2426.7010), X64 RyuJIT AVX2
public object ternary_if() => somt ? to_return1 : to_return2;
       cmp       byte ptr [rcx+18],0
       jne       short M00_L00
       mov       rax,[rcx+10]
       ret
M00_L00:
       mov       rax,[rcx+8]
       ret
Total bytes of code 16

test.bench_branchlessif.branchless_if()
       sub       rsp,18
       mov       eax,[rcx+18]
       lea       rdx,[rcx+8]
       mov       [rsp+10],rdx
       mov       rdx,[rsp+10]
       add       rcx,10
       mov       [rsp+8],rcx
       movsxd    rcx,eax
       imul      rdx,rcx
       neg       eax
       inc       eax
       cdqe
       mov       rcx,[rsp+8]
       imul      rax,rcx
       add       rax,rdx
       add       rsp,18
       ret
Total bytes of code 60
*/
