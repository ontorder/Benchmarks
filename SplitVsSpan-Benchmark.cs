using System.Buffers;
using System.Text;
using System;
using BenchmarkDotNet.Attributes;
using System.Linq;

namespace test;

[MemoryDiagnoser]
public class bench_splits
{
    [Benchmark]
    public void bench_split()
    {
        _ = split_string("3 words string");
        _ = split_string("frase ben più lunga sticazzi");
        _ = split_string("fin");
        _ = split_string("mamma mia");
        _ = split_string("sto ascoltando luigi nono");
        _ = split_string("voglio musica più strana");
        _ = split_string("mi serve un testo più lungo di così");
        _ = split_string("che i sogni si realizzino");
        _ = split_string("consiglio del giorno giovedì ventotto");
        _ = split_string("tutti presenti apriamo la seduta");
        _ = split_string("votate");
        _ = split_string("cento favorevoli venticinque astenuti settantasette contrari");
    }

    [Benchmark]
    public void bench_span()
    {
        split_span_v5("3 words string");
        split_span_v5("frase ben più lunga sticazzi");
        split_span_v5("fin");
        split_span_v5("mamma mia");
        split_span_v5("sto ascoltando luigi nono");
        split_span_v5("voglio musica più strana");
        split_span_v5("mi serve un testo più lungo di così");
        split_span_v5("che i sogni si realizzino");
        split_span_v5("consiglio del giorno giovedì ventotto");
        split_span_v5("tutti presenti apriamo la seduta");
        split_span_v5("votate");
        split_span_v5("cento favorevoli venticinque astenuti settantasette contrari");
    }

    private object split_string(string toSplit)
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

    private static ArrayPool<ReadOnlyMemory<byte>> _romPool = ArrayPool<ReadOnlyMemory<byte>>.Shared;

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

    // stavo pensando se si può elaborare stringa tramite simd ma
    // tanto la parte che occupa di più è Rent()
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
*/
