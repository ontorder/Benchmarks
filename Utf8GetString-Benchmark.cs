using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test;

[BenchmarkDotNet.Attributes.MemoryDiagnoser()]
public class bench_getstr
{
    readonly List<byte[]> buffers = new() {
        Encoding.UTF8.GetBytes("{\"field1"),
        Encoding.UTF8.GetBytes("\":500,\"D"),
        Encoding.UTF8.GetBytes("escripti"),
        Encoding.UTF8.GetBytes("on\":\"Com"),
        Encoding.UTF8.GetBytes("mand fai"),
        Encoding.UTF8.GetBytes("led\"}")
    };

    List<(byte[] Buffer, int CumulativeIndex)> buffers2;

    public bench_getstr()
        => buffers2 = buffers.Aggregate(
            (el: new List<(byte[], int)>(), acc: 0),
            (res, cur) => { res.el.Add((cur, res.acc)); return (res.el, res.acc + cur.Length); },
            r => r.el);

    //[BenchmarkDotNet.Attributes.Benchmark()]
    public string LinkedRos()
    {
        wtf? last = null;
        wtf? first = null;
        for (int bufId = buffers2.Count - 1; bufId >= 0; --bufId)
        {
            var buffer = buffers2[bufId];
            wtf curr = new(buffer.Buffer, first, buffer.CumulativeIndex);
            last ??= curr;
            first = curr;
        }
        var ros = new ReadOnlySequence<byte>(first, 0, last, last.Memory.Length);
        return Encoding.UTF8.GetString(ros);
    }

    //[BenchmarkDotNet.Attributes.Benchmark()]
    public string MemStrReader()
    {
        var ms = new MemoryStream();
        foreach (var b in buffers) ms.Write(b);
        ms.Seek(0, SeekOrigin.Begin);
        var sr = new StreamReader(ms);
        return sr.ReadToEnd();
    }

    //[BenchmarkDotNet.Attributes.Benchmark()]
    public string PipeStrReader()
    {
        var p = new System.IO.Pipelines.Pipe();
        var sr = new StreamReader(p.Reader.AsStream());
        foreach (var b in buffers) p.Writer.Write(b);
        p.Writer.Complete();
        return sr.ReadToEnd();
    }

    //[BenchmarkDotNet.Attributes.Benchmark()]
    public string PipeTryRead()
    {
        var p = new System.IO.Pipelines.Pipe();
        foreach (var b in buffers) p.Writer.Write(b);
        p.Writer.Complete();
        p.Reader.TryRead(out var r);
        return Encoding.UTF8.GetString(r.Buffer);
    }

    [BenchmarkDotNet.Attributes.Benchmark()]
    public string SelectMany()
    {
        var lin = buffers.SelectMany(_ => _).ToArray();
        return Encoding.UTF8.GetString(lin);
    }

    //[BenchmarkDotNet.Attributes.Benchmark()]
    public string BlockCopy()
    {
        int tot = 0;
        foreach (var x in buffers) tot += x.Length;
        var agg = new byte[tot];
        int off = 0;
        for (int bufId = 0; bufId < buffers.Count; ++bufId)
        {
            var b = buffers[bufId];
            Buffer.BlockCopy(b, 0, agg, off, b.Length);
            off += b.Length;
        }
        return Encoding.UTF8.GetString(agg);
    }

    //[BenchmarkDotNet.Attributes.Benchmark()]
    //public string MemoryCopy()
    //{
    //    int tot = 0;
    //    foreach (var x in buffers) tot += x.Length;
    //    var agg = new byte[tot];
    //    int off = 0;
    //    for (int bufId = 0; bufId < buffers.Count; ++bufId)
    //    {
    //        var b = buffers[bufId];
    //        Buffer.MemoryCopy(b, agg, 0, 0);
    //        off += b.Length;
    //    }
    //    return Encoding.UTF8.GetString(agg);
    //}

    [BenchmarkDotNet.Attributes.Benchmark()]
    public string SpanCopySlice()
    {
        int tot = 0;
        foreach (var x in buffers) tot += x.Length;
        Span<byte> agg = stackalloc byte[tot];
        int off = 0;
        foreach (byte[] v in buffers)
        {
            v.CopyTo(agg.Slice(off, v.Length));
            off += v.Length;
        }
        return Encoding.UTF8.GetString(agg);
    }

    [BenchmarkDotNet.Attributes.Benchmark()]
    public string SpanCopyRange()
    {
        int tot = 0;
        foreach (var x in buffers) tot += x.Length;
        Span<byte> agg = stackalloc byte[tot];
        int off = 0;
        foreach (byte[] v in buffers) v.CopyTo(agg[off..(off += v.Length)]);
        return Encoding.UTF8.GetString(agg);
    }

    //[BenchmarkDotNet.Attributes.Benchmark()]
    public async Task<string> PipeAsync()
    {
        StringBuilder merger = new();

        var waitMe = new TaskCompletionSource();
        var p = new System.IO.Pipelines.Pipe();
        _ = ReadFromPipeAsync();

        foreach (var b in buffers) await p.Writer.WriteAsync(b);
        p.Writer.Complete();

        await waitMe.Task;
        return merger.ToString();

        async Task ReadFromPipeAsync()
        {
            System.IO.Pipelines.ReadResult readRes;
            try
            {
                do
                {
                    readRes = await p.Reader.ReadAsync();
                    string readed = Encoding.UTF8.GetString(readRes.Buffer);
                    merger.Append(readed);
                    p.Reader.AdvanceTo(readRes.Buffer.End);
                }
                while (readRes.IsCompleted == false);
                await p.Reader.CompleteAsync();

                waitMe.SetResult();
            }
            catch (Exception readErr)
            {
                Debug.WriteLine("read fuck");
                Debug.WriteLine(readErr.Message);
            }
        }
    }

    //[BenchmarkDotNet.Attributes.Benchmark()]
    public async Task<string> PipeAsync2()
    {
        StringBuilder merger = new();

        var p = new System.IO.Pipelines.Pipe();
        foreach (var b in buffers) await p.Writer.WriteAsync(b);
        p.Writer.Complete();

        System.IO.Pipelines.ReadResult readRes;
        do
        {
            readRes = await p.Reader.ReadAsync();
            string readed = Encoding.UTF8.GetString(readRes.Buffer);
            merger.Append(readed);
            p.Reader.AdvanceTo(readRes.Buffer.End);
        }
        while (readRes.IsCompleted == false);
        p.Reader.Complete();

        return merger.ToString();
    }

    //[BenchmarkDotNet.Attributes.Benchmark()]
    public string AbwDecoder()
    {
        int tot = 0;
        foreach (var x in buffers) tot += x.Length;
        var dec = Encoding.UTF8.GetDecoder();
        var abw = new ArrayBufferWriter<char>(tot);
        var bufCount = buffers.Count;
        for (int bufId = 0; bufId < bufCount; ++bufId) dec.Convert(buffers[bufId], abw, flush: bufId == (bufCount - 1), out _, out _);
        return new string(abw.WrittenSpan);
    }

    [BenchmarkDotNet.Attributes.Benchmark()]
    public string ArrayRent()
    {
        var shr = ArrayPool<byte>.Shared;
        int tot = 0;
        foreach (var x in buffers) tot += x.Length;
        var rented = shr.Rent(tot);
        var rentedSpan = rented.AsSpan();
        int off = 0;
        foreach (byte[] v in buffers) v.CopyTo(rentedSpan[off..(off += v.Length)]);
        string conv = Encoding.UTF8.GetString(rentedSpan);
        shr.Return(rented);
        return conv;
    }

    //[BenchmarkDotNet.Attributes.Benchmark()]
    public string StringCreate()
    {
        int tot = 0;
        foreach (var x in buffers) tot += x.Length;
        return string.Create(tot, buffers, (charSpan, bufs) =>
        {
            var dec = Encoding.UTF8.GetDecoder();
            int off = 0;
            int bufCount = bufs.Count;
            for (int bufId = 0; bufId < bufCount; ++bufId)
            {
                byte[] curr = buffers[bufId];
                dec.Convert(curr, charSpan[off..(off += curr.Length)], flush: bufId == (bufCount - 1), out _, out _, out _);
            }
        });
    }

    [BenchmarkDotNet.Attributes.Benchmark()]
    public string MergeIntoRentedBuffer()
    {
        int mergedLength = 0;
        foreach (var buffer in buffers) mergedLength += buffer.Length;
        var merged = ArrayPool<byte>.Shared.Rent(mergedLength);
        int offset = 0;
        foreach (var v in buffers)
        {
            v.CopyTo(merged.AsMemory(offset));
            offset += v.Length;
        }
        return Encoding.UTF8.GetString(merged);
    }

    public sealed class wtf : ReadOnlySequenceSegment<byte>
    {
        public wtf(ReadOnlyMemory<byte> memory, ReadOnlySequenceSegment<byte>? next, long runningIndex)
        {
            Memory = memory;
            Next = next;
            RunningIndex = runningIndex;
        }
    }
}

/*

| Method        | Mean        | Error      | StdDev     | Gen0   | Gen1   | Allocated |
|-------------- |------------:|-----------:|-----------:|-------:|-------:|----------:|
| SpanCopySlice |    55.82 ns |   0.345 ns |   0.306 ns | 0.0178 |      - |     112 B |
| SpanCopyRange |    56.07 ns |   0.251 ns |   0.223 ns | 0.0178 |      - |     112 B |
| ArrayRent     |    74.50 ns |   0.400 ns |   0.355 ns | 0.0242 |      - |     152 B |
| MergeIntoRent |    81.43 ns |   0.944 ns |   0.788 ns | 0.0381 |      - |     240 B |
| BlockCopy     |    92.41 ns |   0.949 ns |   0.888 ns | 0.0293 |      - |     184 B |
| StringCreate  |   154.80 ns |   0.778 ns |   0.689 ns | 0.0370 |      - |     232 B |
| AbwDecoder    |   277.18 ns |   1.585 ns |   1.483 ns | 0.0505 |      - |     320 B |
| MemStrReader  |   363.46 ns |   7.258 ns |   7.129 ns | 0.6337 | 0.0048 |    3976 B |
| SelectMany    |   350.43 ns |   3.024 ns |   2.680 ns | 0.0916 |      - |     576 B |
| PipeTryRead   |   548.74 ns |  10.510 ns |  13.291 ns | 0.7591 | 0.0114 |    4760 B |
| PipeAsync2    |   772.82 ns |   6.102 ns |   5.095 ns | 0.1688 |      - |    1064 B |
| PipeStrReader |   761.41 ns |   8.782 ns |   7.333 ns | 0.6695 | 0.0048 |    4200 B |
| LinkedRos     |   819.73 ns |   5.799 ns |   5.424 ns | 0.1154 |      - |     728 B |
| PipeAsync     | 8,265.63 ns | 130.083 ns | 115.315 ns | 0.2899 |      - |    1854 B |

*/
