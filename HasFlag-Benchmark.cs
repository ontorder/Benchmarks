using BenchmarkDotNet.Attributes;
using System;

namespace test;

[MemoryDiagnoser]
[DisassemblyDiagnoser(printSource: true)]
public class bench_hasflag
{
    public static volatile testflag s_flagvalue = testflag.FB | testflag.FC;

    [Benchmark] public bool native_hasflag() => s_flagvalue.HasFlag(testflag.FB);
    [Benchmark] public bool flag_not_zero() => (s_flagvalue & testflag.FB) > 0;
    [Benchmark] public byte just_math() => (byte)(s_flagvalue & testflag.FB);
}
[Flags]
public enum testflag : byte
{
    FA = 0b001,
    FB = 0b010,
    FC = 0b100,
}
/*
[Benchmark] public bool native_hasflag() => s_flagvalue.HasFlag(testflag.FB);
    movzx     eax,byte ptr [7FFD89942DD8]
    test      al,2
    setne     al
    movzx     eax,al
    ret
Total bytes of code 16

[Benchmark] public bool flag_not_zero() => (s_flagvalue & testflag.FB) > 0;
    movzx     eax,byte ptr [7FFD89922DD8]
    and       eax,2
    setg      al
    movzx     eax,al
    ret
Total bytes of code 17

[Benchmark] public byte just_math() => (byte)(s_flagvalue & testflag.FB);
    movzx     eax,byte ptr [7FFD89912DD8]
    and       eax,2
    ret
Total bytes of code 11
*/
