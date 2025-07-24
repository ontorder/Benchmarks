using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace test;
#pragma warning disable SYSLIB0011

[MemoryDiagnoser]
public class bench_clone
{
    private TalkStartedEto o;

    public bench_clone()
    {
        o = new TalkStartedEto()
        {
            Account = "test_account",
            //CreationDate = DateTime.UtcNow,
            //Id = Guid.NewGuid(),
            Payload = "test_payload",
            TimedMetadataSourceId = "test_source_id",
            TimedMetadataSourceType = TimedMetadataSourceType.Radio,
            TimeStamp = DateTimeOffset.UtcNow,
            ActiveSpeakerList = new List<string> { "speaker1", "speaker2" },
            CardNumber = "ooo1",
            GroupAbbreviation = "GAB",
            GroupAcronym = "GAC",
            GroupName = "Group Name",
            MicrophoneStatus = TalkMicrophoneStatus.Muted,
            SeatNumber = "S1",
            SpeakerName = "John Doe",
            SpeakerType = "Guest",
        };

        AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", true);
    }

    //[Benchmark]
    public object stj()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(o);
        return System.Text.Json.JsonSerializer.Deserialize<TalkStartedEto>(json);
    }

    //[Benchmark]
    public object depre_bf()
    {
        var bf = new BinaryFormatter();
        var mem = new System.IO.MemoryStream();
        bf.Serialize(mem, o);
        mem.Seek(0, System.IO.SeekOrigin.Begin);
        return bf.Deserialize(mem);
    }

    //[Benchmark]
    public object clone()
    {
        return o.ShallowCopy();
    }

    //[Benchmark]
    public object ctor()
    {
        return new TalkStartedEto
        {
            Account = o.Account,
            ActiveSpeakerList = o.ActiveSpeakerList,
            CardNumber = o.CardNumber,
            //CreationDate = o.CreationDate,
            //Id = o.Id,
            Payload = o.Payload,
            TimedMetadataSourceId = o.TimedMetadataSourceId,
            TimedMetadataSourceType = o.TimedMetadataSourceType,
            TimeStamp = o.TimeStamp,
            GroupAbbreviation = o.GroupAbbreviation,
            GroupAcronym = o.GroupAcronym,
            GroupName = o.GroupName,
            MicrophoneStatus = o.MicrophoneStatus,
            SeatNumber = o.SeatNumber,
            SpeakerName = o.SpeakerName,
            SpeakerType = o.SpeakerType,
        };
    }

    [Benchmark]
    public object bw()
    {
        var mem = new System.IO.MemoryStream();
        var bw = new BinaryWriter(mem);
        bw.Write(o.Account);
        bw.Write(o.Payload);
        bw.Write(o.TimedMetadataSourceId);
        bw.Write((int)o.TimedMetadataSourceType);
        bw.Write(o.TimeStamp.ToString("o")); // ISO 8601 format
        bw.Write(o.GroupAbbreviation);
        bw.Write(o.GroupAcronym);
        bw.Write(o.GroupName);
        bw.Write((int)o.MicrophoneStatus);
        bw.Write(o.SeatNumber);
        bw.Write(o.SpeakerName);
        bw.Write(o.SpeakerType);
        bw.Write(o.ActiveSpeakerList is null ? 0 : o.ActiveSpeakerList.Count());
        if (o.ActiveSpeakerList != null)
        {
            foreach (var speaker in o.ActiveSpeakerList)
            {
                bw.Write(speaker);
            }
        }
        bw.Write(o.CardNumber);
        bw.Flush();
        mem.Seek(0, System.IO.SeekOrigin.Begin);
        var br = new BinaryReader(mem);
        var result = new TalkStartedEto
        {
            Account = br.ReadString(),
            Payload = br.ReadString(),
            TimedMetadataSourceId = br.ReadString(),
            TimedMetadataSourceType = (TimedMetadataSourceType)br.ReadInt32(),
            TimeStamp = DateTimeOffset.Parse(br.ReadString()),
            GroupAbbreviation = br.ReadString(),
            GroupAcronym = br.ReadString(),
            GroupName = br.ReadString(),
            MicrophoneStatus = (TalkMicrophoneStatus)br.ReadInt32(),
            SeatNumber = br.ReadString(),
            SpeakerName = br.ReadString(),
            SpeakerType = br.ReadString(),
        };
        var activeSpeakerCount = br.ReadInt32();
        if (activeSpeakerCount > 0)
        {
            var activeSpeakers = new List<string>(activeSpeakerCount);
            for (int i = 0; i < activeSpeakerCount; i++)
            {
                activeSpeakers.Add(br.ReadString());
            }
            result.ActiveSpeakerList = activeSpeakers;
        }
        else
        {
            result.ActiveSpeakerList = new List<string>();
        }
        result.CardNumber = br.ReadString();
        br.Close();
        mem.Close();
        bw.Close();
        mem.Dispose();
        bw.Dispose();
        br.Dispose();
        return result;
    }
}

/*

| Method    | Mean        | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|---------- |------------:|----------:|----------:|-------:|-------:|----------:|
| clone     |    43.25 ns |  0.921 ns |  0.946 ns | 0.0293 |      - |     184 B |
| ctor      |   139.02 ns |  0.534 ns |  0.499 ns | 0.0343 |      - |     216 B |
| bw        |    1.542 us | 0.0078 us | 0.0065 us | 0.3128 |      - |   1.92 KB |
| stj       | 3,574.92 ns | 23.317 ns | 20.670 ns | 0.4158 | 0.0038 |    2624 B |
| depre_bf  |    35.12 us |  0.339 us |  0.317 us | 5.8594 | 0.3052 |  35.98 KB |

*/

[Serializable]
public sealed class TalkStartedEto : TimedMetadataEto
{
    public IEnumerable<string> ActiveSpeakerList { get; set; } = new List<string>();
    public string CardNumber { get; set; } = string.Empty;
    public DateTimeOffset? DateTime { get; set; }
    public string GroupAbbreviation { get; set; } = string.Empty;
    public string GroupAcronym { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public TalkMicrophoneStatus MicrophoneStatus { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public string SpeakerName { get; set; } = string.Empty;
    public string SpeakerType { get; set; } = string.Empty;
}
[Serializable]
public class TimedMetadataEto : PlanetIntegrationEvent
{
    public string Payload { get; set; }
    public string TimedMetadataSourceId { get; set; }
    public TimedMetadataSourceType TimedMetadataSourceType { get; set; }
    public DateTimeOffset TimeStamp { get; set; }
}
[Serializable]
public class PlanetIntegrationEvent
{
    public string Account { get; set; }
    public DateTime CreationDate { get; private set; }
    public Guid Id { get; private set; }

    public PlanetIntegrationEvent()
    {
        Id = Guid.NewGuid();
        CreationDate = DateTime.UtcNow;
    }

    public PlanetIntegrationEvent(Guid id, DateTime creationDate)
    {
        Id = id;
        CreationDate = creationDate;
    }

    public PlanetIntegrationEvent ShallowCopy()
    {
        return (PlanetIntegrationEvent)MemberwiseClone();
    }
}
public enum TalkMicrophoneStatus
{
    Unknown,
    Muted,
    Unmuted,
    Disabled
}
public enum TimedMetadataSourceType
{
    Unknown,
    Radio,
    Television,
    Internet
}
