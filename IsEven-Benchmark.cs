using BenchmarkDotNet.Attributes;
using System.Numerics;
using System.Runtime.Intrinsics.X86;

namespace test;

public class themostuselessbenchintheworld
{
    private int[] data = [1, 2, 3, 4, 5, 6, 7, 8, 9];
    private uint[] udata = [1, 2, 3, 4, 5, 6, 7, 8, 9];

    [Benchmark]
    public bool even_and()
    {
        bool b;
        b = (data[0] & 1) == 0;
        b |= (data[1] & 1) == 0;
        b |= (data[2] & 1) == 0;
        b |= (data[3] & 1) == 0;
        b |= (data[4] & 1) == 0;
        b |= (data[5] & 1) == 0;
        b |= (data[6] & 1) == 0;
        b |= (data[7] & 1) == 0;
        b |= (data[8] & 1) == 0;
        return b;
    }

    [Benchmark]
    public bool even_bit()
    {
        bool b;
        b = BitOperations.TrailingZeroCount(data[0]) > 0;
        b |= BitOperations.TrailingZeroCount(data[1]) > 0;
        b |= BitOperations.TrailingZeroCount(data[2]) > 0;
        b |= BitOperations.TrailingZeroCount(data[3]) > 0;
        b |= BitOperations.TrailingZeroCount(data[4]) > 0;
        b |= BitOperations.TrailingZeroCount(data[5]) > 0;
        b |= BitOperations.TrailingZeroCount(data[6]) > 0;
        b |= BitOperations.TrailingZeroCount(data[7]) > 0;
        b |= BitOperations.TrailingZeroCount(data[8]) > 0;
        return b;
    }

    [Benchmark]
    public bool even_bmi()
    {
        bool b;
        b = Bmi1.TrailingZeroCount(udata[0]) > 0;
        b |= Bmi1.TrailingZeroCount(udata[1]) > 0;
        b |= Bmi1.TrailingZeroCount(udata[2]) > 0;
        b |= Bmi1.TrailingZeroCount(udata[3]) > 0;
        b |= Bmi1.TrailingZeroCount(udata[4]) > 0;
        b |= Bmi1.TrailingZeroCount(udata[5]) > 0;
        b |= Bmi1.TrailingZeroCount(udata[6]) > 0;
        b |= Bmi1.TrailingZeroCount(udata[7]) > 0;
        b |= Bmi1.TrailingZeroCount(udata[8]) > 0;
        return b;
    }

    [Benchmark]
    public bool even_mod()
    {
        bool b;
        b = data[0] % 2 == 0;
        b |= data[1] % 2 == 0;
        b |= data[2] % 2 == 0;
        b |= data[3] % 2 == 0;
        b |= data[4] % 2 == 0;
        b |= data[5] % 2 == 0;
        b |= data[6] % 2 == 0;
        b |= data[7] % 2 == 0;
        b |= data[8] % 2 == 0;
        return b;
    }
}
/*
| Method   | Mean     | Error     | StdDev    |
|--------- |---------:|----------:|----------:|
| even_mod | 3.807 ns | 0.0353 ns | 0.0295 ns |
| even_and | 4.032 ns | 0.0373 ns | 0.0349 ns |
| even_bmi | 4.520 ns | 0.0751 ns | 0.0627 ns |
round 2
| even_mod | 3.927 ns | 0.0273 ns | 0.0242 ns |
| even_and | 4.420 ns | 0.0262 ns | 0.0245 ns |
| even_bmi | 4.326 ns | 0.0734 ns | 0.0613 ns |
round 3
| even_mod | 3.656 ns | 0.0169 ns | 0.0141 ns |
| even_and | 4.129 ns | 0.1021 ns | 0.1397 ns |
| even_bit | 3.882 ns | 0.0152 ns | 0.0135 ns |
| even_bmi | 5.267 ns | 0.0358 ns | 0.0299 ns |
*/
