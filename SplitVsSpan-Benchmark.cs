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
    }

    [Benchmark]
    public void bench_span()
    {
        split_span("3 words string");
        split_span("frase ben più lunga sticazzi");
        split_span("fin");
    }

    private object split_string(string toSplit)
        => toSplit
            .Split(' ')
            .Select(Encoding.UTF8.GetBytes)
            .ToArray();

    private void split_span(string toSplit)
    {
        ReadOnlySpan<char> chars = toSplit.AsSpan();
        var bytePool = ArrayPool<byte>.Shared;
        var bytesPool = ArrayPool<byte[]>.Shared;

        var hasEndingSpace = chars[^1] == ' ';
        var wordsCount = chars.Count(' ') + (hasEndingSpace ? 0 : 1); // potrebbero esserci Empty
        var wordsArray = bytesPool.Rent(wordsCount);

        int spacePos = chars.IndexOf(' ') + 1;
        int wordId = 0;
        var consumed = chars;
        while (spacePos > 0)
        {
            ReadOnlySpan<char> foundWord = consumed[..spacePos];
            byte[] mem = bytePool.Rent(foundWord.Length * 2);
            int count = Encoding.UTF8.GetBytes(foundWord, mem);
            wordsArray[wordId] = mem;
            ++wordId;
            consumed = consumed[spacePos..];
            spacePos = consumed.IndexOf(' ') + 1;
        }
        if (consumed.Length > 0)
        {
            byte[] lastMem = bytePool.Rent(consumed.Length * 2);
            _ = Encoding.UTF8.GetBytes(consumed, lastMem);
            wordsArray[wordId] = lastMem;
        }

        for (int rentId = 0; rentId < wordsCount; ++rentId) bytePool.Return(wordsArray[rentId]);
        bytesPool.Return(wordsArray);
    }
}

/*
| Method      | Mean     | Error   | StdDev  | Gen0   | Allocated |
|------------ |---------:|--------:|--------:|-------:|----------:|
| bench_split | 484.2 ns | 6.32 ns | 5.92 ns | 0.1869 |    1176 B |
| bench_span  | 567.9 ns | 3.02 ns | 2.68 ns |      - |         - |
*/
