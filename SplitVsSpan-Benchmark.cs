using System.Buffers;
using System.Text;
using System;
using BenchmarkDotNet.Attributes;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using Orleans.Serialization.Codecs;

namespace test;

[MemoryDiagnoser]
public class bench_splits
{
    private Vector<byte> _spacesVec;

    public bench_splits()
    {
        var spacesBytes = Encoding.UTF8.GetBytes(new string(' ', 32));
        _spacesVec = new Vector<byte>(spacesBytes);
    }

    [Benchmark]
    public void bench_split()
    {
        _ = split_string("3 words string");
        _ = split_string("frase ben piu lunga sticazzi");
        _ = split_string("fin");
        _ = split_string("mamma mia");
        _ = split_string("sto ascoltando luigi nono");
        _ = split_string("voglio musica piu strana");
        _ = split_string("mi serve un testo piu lungo di c");
        _ = split_string("che i sogni si realizzino");
        _ = split_string("consiglio del giorno giovedi ven");
        _ = split_string("tutti presenti apriamo la seduta");
        _ = split_string("votate");
        _ = split_string("cento favorevoli venticinque ast");
    }

    [Benchmark]
    public void bench_span()
    {
        _ = split_v256("3 words string");
        _ = split_v256("frase ben piu lunga sticazzi");
        _ = split_v256("fin");
        _ = split_v256("mamma mia");
        _ = split_v256("sto ascoltando luigi nono");
        _ = split_v256("voglio musica piu strana");
        _ = split_v256("mi serve un testo piu lungo di c");
        _ = split_v256("che i sogni si realizzino");
        _ = split_v256("consiglio del giorno giovedi ven");
        _ = split_v256("tutti presenti apriamo la seduta");
        _ = split_v256("votate");
        _ = split_v256("cento favorevoli venticinque ast");
    }

    private byte[][] split_string(string toSplit)
        => toSplit
            .Split(' ')
            .Select(Encoding.UTF8.GetBytes)
            .ToArray();

    private readonly static ArrayPool<byte> _bytePool = ArrayPool<byte>.Shared;
    private readonly static ArrayPool<byte[]> _bytesPool = ArrayPool<byte[]>.Shared;

    private void split_span_v2(string toSplit)
    {
        var chars = toSplit.AsSpan();

        var hasEndingSpace = chars[^1] == ' ';
        var wordsCount = chars.Count(' ') + (hasEndingSpace ? 0 : 1); // potrebbero esserci Empty
        var wordsArray = _bytesPool.Rent(wordsCount);

        int spacePos = chars.IndexOf(' ') + 1;
        int wordId = 0;
        var consumed = chars;
        while (spacePos > 0)
        {
            var foundWord = consumed[..spacePos];
            var mem = _bytePool.Rent((int)(foundWord.Length * 1.5));
            _ = Encoding.UTF8.GetBytes(foundWord, mem);
            wordsArray[wordId] = mem;
            ++wordId;
            consumed = consumed[spacePos..];
            spacePos = consumed.IndexOf(' ') + 1;
        }
        if (consumed.Length > 0)
        {
            var lastMem = _bytePool.Rent(consumed.Length * 2);
            _ = Encoding.UTF8.GetBytes(consumed, lastMem);
            wordsArray[wordId] = lastMem;
        }

        for (int rentId = 0; rentId < wordsCount; ++rentId) _bytePool.Return(wordsArray[rentId]);
        _bytesPool.Return(wordsArray);
    }

    private readonly static ArrayPool<(short Start, short End)> _sePool = ArrayPool<(short Start, short End)>.Shared;

    readonly struct StartEnd
    {
        public readonly short Start;
        public readonly short End;

        public StartEnd(short start, short end)
        {
            Start = start;
            End = end;
        }
    }

    // potrei semplicemente convertire tutta la stringa in byte e lavorare sugli spazi dopo?
    private void split_span_v3(string toSplit)
    {
        var chars = toSplit.AsSpan();
        int wordsCount = 0;
        var positions = _sePool.Rent(10);
        short prev = 0;
        for (int charId = 0; charId < chars.Length; ++charId)
        {
            // potrebbero esserci Empty
            if (chars[charId] == ' ' || charId == (chars.Length - 1))
            {
                var end = (short)(charId + 1);
                positions[wordsCount] = (prev, end);
                prev = end;
                ++wordsCount;
            }
        }
        var wordsArray = _bytesPool.Rent(wordsCount);

        for (int wordId = 0; wordId < wordsCount; ++wordId)
        {
            var pos = positions[wordId];
            var foundWord = chars[pos.Start..pos.End];
            var mem = _bytePool.Rent((int)(foundWord.Length * 1.5));
            _ = Encoding.UTF8.GetBytes(foundWord, mem);
            wordsArray[wordId] = mem;
        }

        for (int rentId = 0; rentId < wordsCount; ++rentId) _bytePool.Return(wordsArray[rentId]);
        _bytesPool.Return(wordsArray);
        _sePool.Return(positions);
    }

    static readonly byte Spazio = " "u8[0];

    private void split_span_v4(string toSplit)
    {
        var strlen = toSplit.Length;
        var bytes = _bytePool.Rent(strlen * 2);
        var slice = bytes.AsSpan();
        var len = Encoding.UTF8.GetBytes(toSplit, bytes);
        var positions = _bytesPool.Rent(10);
        short prev = 0;
        int wordId = 0;
        for (int byteId = 0; byteId < len; ++byteId)
        {
            // potrebbero esserci Empty
            if (bytes[byteId] == Spazio || byteId == (strlen - 1))
            {
                var end = (short)(byteId + 1);
                var word = slice[prev..end];
                var copied = _bytePool.Rent(word.Length);
                word.CopyTo(copied);
                positions[wordId] = copied;
                prev = end;
                ++wordId;
            }
        }

        for (int i = 0; i < wordId; ++i) _bytePool.Return(positions[i]);
        _bytesPool.Return(positions);
        _bytePool.Return(bytes);
    }

    private static readonly ArrayPool<ReadOnlyMemory<byte>> _romPool = ArrayPool<ReadOnlyMemory<byte>>.Shared;

    private void split_span_v5(string toSplit)
    {
        var strlen = toSplit.Length;
        var bytes = _bytePool.Rent(strlen * 2);
        var len = Encoding.UTF8.GetBytes(toSplit, bytes);
        var romBytes = new ReadOnlyMemory<byte>(bytes);
        var romSlices = _romPool.Rent(10);
        short prev = 0;
        int wordId = 0;
        for (int byteId = 0; byteId < len; ++byteId)
        {
            // potrebbero esserci Empty
            if (bytes[byteId] == Spazio || byteId == (strlen - 1))
            {
                var end = (short)(byteId + 1);
                romSlices[wordId] = romBytes[prev..end];
                prev = end;
                ++wordId;
            }
        }

        _bytePool.Return(bytes);
        _romPool.Return(romSlices);
    }

    private SplitSpan5B_Data split_span_v5b(string toSplit)
    {
        var strlen = toSplit.Length;
        var bytes = _bytePool.Rent(strlen * 2);
        var len = Encoding.UTF8.GetBytes(toSplit, bytes);
        var romBytes = new ReadOnlyMemory<byte>(bytes);
        var romSlices = _romPool.Rent(10);
        short prev = 0;
        int wordId = 0;
        for (int byteId = 0; byteId < len; ++byteId)
        {
            // potrebbero esserci Empty
            if (bytes[byteId] == Spazio || byteId == (strlen - 1))
            {
                var end = (short)(byteId + 1);
                romSlices[wordId] = romBytes[prev..end];
                prev = end;
                ++wordId;
            }
        }
        return new SplitSpan5B_Data(bytes, romSlices);
    }

    public readonly struct SplitSpan5B_Data : IDisposable
    {
        public readonly byte[] Bytes;
        public readonly ReadOnlyMemory<byte>[] RomSlices;

        public SplitSpan5B_Data(byte[] bytes, ReadOnlyMemory<byte>[] romSlices)
        {
            Bytes = bytes;
            RomSlices = romSlices;
        }

        public void Dispose()
        {
            _bytePool.Return(Bytes);
            _romPool.Return(RomSlices);
        }
    }

    Vector256<byte> _spacesV256 = Vector256<byte>.One * 32;

    public IEnumerable<ReadOnlyMemory<byte>> split_v256(string toSplit)
    {
        var toSplitBytes = new byte[Vector<byte>.Count];
        Encoding.UTF8.GetBytes(toSplit, toSplitBytes);
        var toSplitVec = new Vector<byte>(toSplitBytes);
        var toSplitV256 = toSplitVec.AsVector256();

        var matchesV256 = Vector256.Equals(toSplitV256, _spacesV256);
        var spacesBitMask = (uint)Avx2.MoveMask(matchesV256);
        var nonZeroes = BitOperations.PopCount(spacesBitMask);
        var posArr = new int[nonZeroes];
        int prevTzc = 0;
        int posArrId = 0;

        while (spacesBitMask != 0)
        {
            var trailZeroCount = BitOperations.TrailingZeroCount(spacesBitMask);
            posArr[posArrId++] = trailZeroCount + prevTzc;
            spacesBitMask >>= trailZeroCount + 1;
            prevTzc += trailZeroCount + 1;
        }

        int prev = 0;
        var toSplitMem = toSplitBytes.AsMemory();
        var spans = new ReadOnlyMemory<byte>[posArr.Length + 1];
        for (int i = 0; i < posArr.Length; ++i)
        {
            spans[i] = toSplitMem[prev..posArr[i]];
            prev = posArr[i];
        }
        if (prev < toSplitBytes.Length)
            spans[^1] = toSplitMem[prev..];

        return spans;
    }

    public IEnumerable<ReadOnlyMemory<byte>> split_vector(string toSplit)
    {
        var toSplitBytes = new byte[Vector<byte>.Count];
        Encoding.UTF8.GetBytes(toSplit, toSplitBytes);
        var toSplitVec = new Vector<byte>(toSplitBytes);

        Vector<byte> matchesVec = Vector.Equals(toSplitVec, _spacesVec);

        var spacesBitMask = 0u;
        var nonZeroes = BitOperations.PopCount(spacesBitMask);
        var posArr = new List<int>(nonZeroes);
        int prevTzc = 0;
    reshiftmedaddy:
        {
            var trailZeroCount = BitOperations.TrailingZeroCount(spacesBitMask);
            posArr.Add(trailZeroCount + prevTzc);
            spacesBitMask >>= trailZeroCount + 1;
            prevTzc += trailZeroCount + 1;
            if (spacesBitMask != 0)
                goto reshiftmedaddy;
        }

        int prev = 0;
        var spans = new ReadOnlyMemory<byte>[posArr.Count + 1];
        for (int i = 0; i < posArr.Count; ++i)
        {
            var span = toSplitBytes[prev..posArr[i]];
            spans[i] = span;
            prev = posArr[i];
        }
        if (prev < toSplitBytes.Length)
            spans[^1] = toSplitBytes[prev..];

        return spans;
    }

    // test 1: usare .ExtractMostSignificantBits();
    // test 2: mettere resettare bit trovato e usare sempre LeadingZeroCount
    // test 3: tentare lo stesso Vector512
    // non esiste un modo per parallelizzare conteggio bit, vero?
    // se tipo facessi 000101010100 *
    //                 012345678901 = indici
    // potrei recuperare valori non zero?
}

/*
| Method      | Mean     | Error   | StdDev  | Gen0   | Allocated |
|------------ |---------:|--------:|--------:|-------:|----------:|
| bench_split | 484.2 ns | 6.32 ns | 5.92 ns | 0.1869 |    1176 B |
| bench_span  | 567.9 ns | 3.02 ns | 2.68 ns |      - |         - |

span v2
| Method      | Mean     | Error     | StdDev    | Median   | Gen0   | Allocated |
|------------ |---------:|----------:|----------:|---------:|-------:|----------:|
| bench_split | 2.687 us | 0.0512 us | 0.0986 us | 2.633 us | 0.9537 |    6000 B |
| bench_span  | 3.281 us | 0.0217 us | 0.0181 us | 3.275 us |      - |         - |

span v3 con StartEnd
| Method      | Mean     | Error     | StdDev    | Gen0   | Allocated |
|------------ |---------:|----------:|----------:|-------:|----------:|
| bench_split | 2.552 us | 0.0493 us | 0.0659 us | 0.9537 |    6000 B |
| bench_span  | 3.697 us | 0.0642 us | 0.0600 us |      - |         - |

span v3 con tupla
| Method      | Mean     | Error     | StdDev    | Gen0   | Allocated |
|------------ |---------:|----------:|----------:|-------:|----------:|
| bench_split | 2.397 us | 0.0137 us | 0.0122 us | 0.9537 |    6000 B |
| bench_span  | 3.605 us | 0.0710 us | 0.0899 us |      - |         - |

span v4 -- ho sbagliato qualcosa?
| Method      | Mean     | Error     | StdDev    | Gen0   | Allocated |
|------------ |---------:|----------:|----------:|-------:|----------:|
| bench_split | 2.381 us | 0.0104 us | 0.0087 us | 0.9537 |   5.86 KB |
| bench_span  | 1.361 us | 0.0076 us | 0.0067 us | 0.2613 |    1.6 KB |

span v4 corretto
| Method      | Mean     | Error     | StdDev    | Gen0   | Allocated |
|------------ |---------:|----------:|----------:|-------:|----------:|
| bench_split | 2.380 us | 0.0143 us | 0.0127 us | 0.9537 |    6000 B |
| bench_span  | 3.416 us | 0.0670 us | 0.0772 us |      - |         - |

span v5
| Method      | Mean       | Error    | StdDev   | Gen0   | Allocated |
|------------ |-----------:|---------:|---------:|-------:|----------:|
| bench_split | 2,416.7 ns | 21.65 ns | 20.25 ns | 0.9537 |    6000 B |
| bench_span  |   931.5 ns |  3.52 ns |  3.12 ns |      - |         - |

span v256
| Method      | Mean       | Error    | StdDev   | Gen0   | Allocated |
|------------ |-----------:|---------:|---------:|-------:|----------:|
| bench_split | 2,239.8 ns | 22.62 ns | 18.89 ns | 0.9193 |   5.65 KB |
| bench_span  |   438.8 ns |  2.53 ns |  2.37 ns | 0.3443 |   2.11 KB |
*/
