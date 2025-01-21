using BenchmarkDotNet.Attributes;
using Planet.Core.Security.Cryptography;
using System.Collections.Generic;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Linq;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace test;

[MemoryDiagnoser]
public class test_stj
{
    private static readonly System.Text.Json.JsonSerializerOptions _stjOptions = new System.Text.Json.JsonSerializerOptions()
    {
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
        WriteIndented = false,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    Content _c = new Content()
    {
        Author = "au",
        AuthorCountry = "ac",
        AuthorCity = "ac",
        AuthorImage = "ai",
        AuthorOrganization = "ao",
        ContentAccess = ContentAccess.All,
        ContentId = 1,
        ContentIngestionWorkflowId = "elimina",
        ContentOrigin = ContentOrigin.Undefined,
        ContentState = ContentState.Awaiting,
        Description = "de",
        EmbedPolicy = ContentRestriction.DomainWhitelist,
        ExternalId = "ex",
        Featured = false,
        GroupName = "gn",
        Group = Guid.Empty,
        Guid = Guid.Empty,
        Hash = "h",
        Image = "i",
        IsDownloadable = false,
        IsShareable = true,
        LongDescription = "ld",
        Owner = Guid.Empty,
        OwnerName = "on",
        Permissions = 3,
        ProcessingStatus = ProcessingStatus.UploadCancelled,
        Reference = "re",
        ReferenceDate = DateTimeOffset.MinValue,
        TimeZoneName = "tz",
        TimeZoneOffset = TimeSpan.Zero,
        Title = "ti",
        ValidFrom = DateTimeOffset.MinValue,
        ValidTo = DateTimeOffset.MinValue,
        Tags = new List<string>() { "t1", "t2" },
        DomainPolicies = new List<string>() { "www.1.com", "www.2.com" },
    };

    [Benchmark]
    public string stj_content()
    {
        var content = MapContentBaseEvent(_c);
        return System.Text.Json.JsonSerializer.Serialize(content, _stjOptions);
    }

    private static readonly System.Text.Json.JsonSerializerOptions stjOptions2 = new System.Text.Json.JsonSerializerOptions()
    {
        MaxDepth = 3,
        WriteIndented = false,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    [Benchmark]
    public string stj_flat()
    {
        var content = MapContentBaseEventFlat(_c);
        return System.Text.Json.JsonSerializer.Serialize(content, stjOptions2);
    }

    public static Content MapContentBaseEvent(Content c)
    {
        var newContent = new Content()
        {
            ContentAccess = c.ContentAccess,
            ContentId = c.ContentId,
            ContentOrigin = c.ContentOrigin,
            ContentState = c.ContentState,
            Tags = c.Tags,
            Description = c.Description,
            Featured = c.Featured,
            Group = c.Group,
            GroupName = c.GroupName,
            Image = c.Image,
            LastModificationTime = c.LastModificationTime,
            Owner = c.Owner,
            OwnerName = c.OwnerName,
            Permissions = c.Permissions,
            ProcessingStatus = c.ProcessingStatus,
            Reference = c.Reference,
            ReferenceDate = c.ReferenceDate,
            Title = c.Title,
            ValidFrom = c.ValidFrom,
            ValidTo = c.ValidTo,
            DomainPolicies = c.DomainPolicies
        };
        TrySetProperty(newContent, _ => _.CreationTime, () => c.CreationTime);
        return newContent;
    }

    public static object MapContentBaseEventFlat(Content c)
    {
        return new
        {
            c.ContentAccess,
            c.ContentId,
            c.ContentOrigin,
            c.ContentState,
            c.Tags,
            c.Description,
            c.Featured,
            c.Group,
            c.GroupName,
            c.Image,
            c.CreationTime,
            c.LastModificationTime,
            c.Owner,
            c.OwnerName,
            c.Permissions,
            c.ProcessingStatus,
            c.Reference,
            c.ReferenceDate,
            c.Title,
            c.ValidFrom,
            c.ValidTo,
            c.DomainPolicies
        };
    }

    [Benchmark]
    public string string_stj()
    {
        var jsonTags = string.Join(',', _c.Tags.Select(tag => System.Text.Json.JsonSerializer.Serialize(tag, stjOptions2)));
        var jsonDescription = System.Text.Json.JsonSerializer.Serialize(_c.Description, stjOptions2);
        var jsonGroupName = System.Text.Json.JsonSerializer.Serialize(_c.GroupName, stjOptions2);
        var jsonImage = System.Text.Json.JsonSerializer.Serialize(_c.Image, stjOptions2);
        var jsonCreationTime = System.Text.Json.JsonSerializer.Serialize(_c.CreationTime, stjOptions2);
        var jsonLastModificationTime = System.Text.Json.JsonSerializer.Serialize(_c.LastModificationTime, stjOptions2);
        var jsonOwnerName = System.Text.Json.JsonSerializer.Serialize(_c.OwnerName, stjOptions2);
        var jsonReference = System.Text.Json.JsonSerializer.Serialize(_c.Reference, stjOptions2);
        var jsonReferenceDate = System.Text.Json.JsonSerializer.Serialize(_c.ReferenceDate, stjOptions2);
        var jsonTitle = System.Text.Json.JsonSerializer.Serialize(_c.Title, stjOptions2);
        var jsonValidFrom = System.Text.Json.JsonSerializer.Serialize(_c.ValidFrom, stjOptions2);
        var jsonValidTo = System.Text.Json.JsonSerializer.Serialize(_c.ValidTo, stjOptions2);
        return $$"""
        {
        "ContentAccess": "{{_c.ContentAccess}}",
        "ContentId": {{_c.ContentId}},
        "ContentOrigin": "{{_c.ContentOrigin}}",
        "ContentState": "{{_c.ContentState}}",
        "Tags": [{jsonTags}],
        "Description": {{jsonDescription}},
        "Featured": {{_c.Featured}},
        "Group": "{{_c.Group}}",
        "GroupName": {{jsonGroupName}},
        "Image": {{jsonImage}},
        "CreationTime": {{jsonCreationTime}},
        "LastModificationTime": {{jsonLastModificationTime}},
        "Owner": "{{_c.Owner}}",
        "OwnerName": {{jsonOwnerName}},
        "Permissions": {{_c.Permissions}},
        "ProcessingStatus": "{{_c.ProcessingStatus}}",
        "Reference": {{jsonReference}},
        "ReferenceDate": {{jsonReferenceDate}},
        "Title": {{jsonTitle}},
        "ValidFrom": {{jsonValidFrom}},
        "ValidTo": {{jsonValidTo}},
        "DomainPolicies": {{_c.DomainPolicies}}
        }
        """;
    }

    [Benchmark]
    public string manual_json()
    {
        static string escape(string? s) => s?.Replace("\"", "\\\"");

        var jsonTags = string.Join(',', _c.Tags.Select(tag => escape(tag)));

        var jsonDescription = escape(_c.Description);
        var jsonGroupName = escape(_c.GroupName);
        var jsonImage = escape(_c.Image);
        var jsonOwnerName = escape(_c.OwnerName);
        var jsonReference = escape(_c.Reference);
        var jsonTitle = escape(_c.Title);

        var jsonCreationTime = _c.CreationTime.ToString("r");
        var jsonReferenceDate = _c.ReferenceDate?.ToString("r");
        var jsonLastModificationTime = _c.LastModificationTime?.ToString("r");
        var jsonValidFrom = _c.ValidFrom?.ToString("r");
        var jsonValidTo = _c.ValidTo?.ToString("r");

        return $$"""
        {"ContentAccess": "{{_c.ContentAccess}}","ContentId": {{_c.ContentId}},"ContentOrigin": "{{_c.ContentOrigin}}","ContentState": "{{_c.ContentState}}",
        "Tags": [{jsonTags}],"Description": {{jsonDescription}},"Featured": {{_c.Featured}},"Group": "{{_c.Group}}","GroupName": {{jsonGroupName}},
        "Image": {{jsonImage}},"CreationTime": {{jsonCreationTime}},"LastModificationTime": {{jsonLastModificationTime}},"Owner": "{{_c.Owner}}",
        "OwnerName": {{jsonOwnerName}},"Permissions": {{_c.Permissions}},"ProcessingStatus": "{{_c.ProcessingStatus}}","Reference": {{jsonReference}},
        "ReferenceDate": {{jsonReferenceDate}},"Title": {{jsonTitle}},"ValidFrom": {{jsonValidFrom}},"ValidTo": {{jsonValidTo}},"DomainPolicies": {{_c.DomainPolicies}}}
        """;
    }

    /*

    | Method      | Mean       | Error    | StdDev   | Gen0   | Allocated |
    |------------ |-----------:|---------:|---------:|-------:|----------:|
    | manual_json |   784.8 ns |  5.44 ns |  4.83 ns | 0.2880 |   1.77 KB |
    | stj_flat    | 1,816.3 ns | 11.83 ns | 11.07 ns | 0.3109 |   1.91 KB |
    | string_stj  | 2,762.6 ns | 24.37 ns | 22.80 ns | 0.3357 |   2.08 KB |
    | stj_content | 5,785.6 ns | 39.26 ns | 36.72 ns | 0.7324 |   4.67 KB |

    */
    private static readonly ConcurrentDictionary<string, PropertyInfo?> CachedObjectProperties =
        new ConcurrentDictionary<string, PropertyInfo?>();

    public static void TrySetProperty<TObject, TValue>(
        TObject obj,
        Expression<Func<TObject, TValue>> propertySelector,
        Func<TValue> valueFactory,
        params Type[] ignoreAttributeTypes)
    {
        TrySetProperty(obj, propertySelector, x => valueFactory(), ignoreAttributeTypes);
    }

    public static void TrySetProperty<TObject, TValue>(
        TObject obj,
        Expression<Func<TObject, TValue>> propertySelector,
        Func<TObject, TValue> valueFactory,
        params Type[]? ignoreAttributeTypes)
    {
        var cacheKey = $"{obj?.GetType().FullName}-" +
                       $"{propertySelector}-" +
                       $"{(ignoreAttributeTypes != null ? "-" + string.Join("-", ignoreAttributeTypes.Select(x => x.FullName)) : "")}";

        var property = CachedObjectProperties.GetOrAdd(cacheKey, _ =>
        {
            if (propertySelector.Body.NodeType != ExpressionType.MemberAccess)
            {
                return null;
            }

            var memberExpression = propertySelector.Body.As<MemberExpression>();

            var propertyInfo = obj?.GetType().GetProperties().FirstOrDefault(x =>
                x.Name == memberExpression.Member.Name &&
                x.GetSetMethod(true) != null);

            if (propertyInfo == null)
            {
                return null;
            }

            if (ignoreAttributeTypes != null &&
                ignoreAttributeTypes.Any(ignoreAttribute => propertyInfo.IsDefined(ignoreAttribute, true)))
            {
                return null;
            }

            return propertyInfo;
        });

        property?.SetValue(obj, valueFactory(obj));
    }
}
public class Content : FullAuditedEntity, IContent
{
    public Content() { }

    public string Author { get; set; }
    public string AuthorCountry { get; set; }
    public string AuthorCity { get; set; }
    public string AuthorImage { get; set; }
    public string AuthorOrganization { get; set; }
    public ContentAccess ContentAccess { get; set; }
    public long ContentId { get; set; }
    public string ContentIngestionWorkflowId { get; set; }
    public ContentOrigin ContentOrigin { get; set; }
    public ContentState ContentState { get; set; }
    public string? Description { get; set; }
    public ContentRestriction EmbedPolicy { get; set; }
    public string ExternalId { get; set; }
    public bool Featured { get; set; }
    public string GroupName { get; set; }
    public Guid? Group { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public string Hash { get; set; }
    public string Image { get; set; }
    public bool IsDownloadable { get; set; }
    public bool IsShareable { get; set; }
    public string LongDescription { get; set; }
    public Guid? Owner { get; set; }
    public string OwnerName { get; set; }
    public ulong Permissions { get; set; }
    public ProcessingStatus ProcessingStatus { get; set; }
    public string Reference { get; set; }
    public DateTimeOffset? ReferenceDate { get; set; }
    public string TimeZoneName { get; set; }
    public TimeSpan TimeZoneOffset { get; set; }
    public string Title { get; set; }
    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? ValidTo { get; set; }

    public ICollection<string> DomainPolicies { get; set; } = new List<string>();
    public ICollection<object> Events { get; set; } = new List<object>();
    public ICollection<object> FileTransferSessions { get; set; } = new List<object>();
    public ICollection<object> Playlists { get; set; } = new List<object>();
    public ICollection<string> Tags { get; set; } = new List<string>();
    public ICollection<object> Topics { get; set; } = new List<object>();
    public ICollection<object> Tracks { get; set; } = new List<object>();

    public static string GenerateHash(Type type, string title = "", string description = "", DateTimeOffset? referenceDate = null)
        => HashGenerator.GenerateHash(
            type.ToString(),
            title,
            description,
            referenceDate == null
                ? "0000-00-00"
                : referenceDate.Value.ToString("yyyy-MM-dd")
        );

    public override object[] GetKeys() => new object[] { ContentId };
}
public interface IContent : IHasGuidProfilation
{
    public string? Author { get; set; }
    public string? AuthorCountry { get; set; }
    public string? AuthorCity { get; set; }
    public string? AuthorImage { get; set; }
    public string? AuthorOrganization { get; set; }
    public ContentAccess ContentAccess { get; set; }
    public long ContentId { get; set; }
    public string? ContentIngestionWorkflowId { get; set; }
    public ContentOrigin ContentOrigin { get; set; }
    public ContentState ContentState { get; set; }
    public string? Description { get; set; }
    public ContentRestriction EmbedPolicy { get; set; }
    public string? ExternalId { get; set; }
    public bool Featured { get; set; }
    public string? GroupName { get; set; }
    public Guid Guid { get; set; }
    public string? Hash { get; set; }
    public string? Image { get; set; }
    public bool IsDownloadable { get; set; }
    public bool IsShareable { get; set; }
    public string? LongDescription { get; set; }
    public string? OwnerName { get; set; }
    public ulong Permissions { get; set; }
    public ProcessingStatus ProcessingStatus { get; set; }
    public string? Reference { get; set; }
    public DateTimeOffset? ReferenceDate { get; set; }
    public string TimeZoneName { get; set; }
    public TimeSpan TimeZoneOffset { get; set; }
    public string Title { get; set; }
    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? ValidTo { get; set; }
}
public interface IHasGuidProfilation
{
    public Guid? Group { get; set; }
    public Guid? Owner { get; set; }
}
public enum ContentAccess : short
{
    Nobody = 0,
    Profilated = 1,
    Authenticated = 2,
    All = 3,
    Unlisted = 4,
}
public enum ContentOrigin : short
{
    Undefined = 0,
    WebUpload = 1,
    BatchUpload = 2,
    Recording = 3,
    CutOperation = 4,
    AutoRecording = 5,
    WebInterface = 6,
    Otmm = 7,
}
public enum ContentState : short
{
    Disabled = 0,
    Active = 1,
    Awaiting = 2,
    Cancelled = 3,
    Expired = 4,
    Suspended = 5,
}
[Flags]
public enum ContentRestriction : uint
{
    // parse referrer host or explicit domain parameter
    None = 0b_00000000_00000000_00000000_00000000,
    DomainBlacklist = 0b_00000000_00000000_00000000_00000010,
    DomainWhitelist = 0b_00000000_00000000_00000000_00000001,
}
public enum ProcessingStatus
{
    Empty = 0,
    OperationInProgress = 1,
    Done = 2,
    WaitingForUpload = 11,
    UploadInProgress = 12,
    UploadPaused = 13,
    UploadCancelled = 14,
    UploadFailed = 15,
    UploadCompleted = 16,
    TranscodingQueued = 21,
    TranscodingInProgress = 22,
    TranscodingFailed = 23,
    TranscodingCompleted = 24,
    RecordingPrestart = 31,
    RecordingInProgress = 32,
    RecordingCompleted = 33,
    RecordingReady = 34,
    RecordingFailed = 35,
    AudioVideoCutQueued = 41,
    AudioVideoCutInProgress = 42,
    AudioVideoCutFailed = 43,
    AudioVideoCutCompleted = 44,
    BroadcastPrestart = 51,
    BroadcastInProgress = 52,
    BroadcastCompleted = 53,
    BroadcastFailed = 54,
    Error = 1000,
}
public abstract class FullAuditedEntity : AuditedEntity, IFullAuditedObject
{
    /// <inheritdoc />
    public virtual bool IsDeleted { get; set; }

    /// <inheritdoc />
    public virtual Guid? DeleterId { get; set; }

    /// <inheritdoc />
    public virtual DateTime? DeletionTime { get; set; }
}
public interface IFullAuditedObject : IAuditedObject, ICreationAuditedObject, IHasCreationTime, IMayHaveCreator, IModificationAuditedObject, IHasModificationTime, IDeletionAuditedObject, IHasDeletionTime, ISoftDelete
{
}
public interface IAuditedObject : ICreationAuditedObject, IHasCreationTime, IMayHaveCreator, IModificationAuditedObject, IHasModificationTime
{
}
public interface ICreationAuditedObject : IHasCreationTime, IMayHaveCreator
{
}
public interface IHasCreationTime
{
    DateTime CreationTime { get; }
}
public interface IMayHaveCreator
{
    Guid? CreatorId { get; }
}
public interface IModificationAuditedObject : IHasModificationTime
{
    Guid? LastModifierId { get; }
}
public interface IHasModificationTime
{
    DateTime? LastModificationTime { get; }
}
public abstract class AuditedEntity : CreationAuditedEntity, IAuditedObject
{
    /// <inheritdoc />
    public virtual DateTime? LastModificationTime { get; set; }

    /// <inheritdoc />
    public virtual Guid? LastModifierId { get; set; }
}
public abstract class CreationAuditedEntity : Entity, ICreationAuditedObject
{
    /// <inheritdoc />
    public virtual DateTime CreationTime { get; protected set; }

    /// <inheritdoc />
    public virtual Guid? CreatorId { get; protected set; }
}
public abstract class Entity : IEntity
{
    protected Entity()
    {
        EntityHelper.TrySetTenantId(this);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return "";
    }

    public abstract object?[] GetKeys();

    public bool EntityEquals(IEntity other)
    {
        return EntityHelper.EntityEquals(this, other);
    }
}
public interface IEntity
{
    //
    // Summary:
    //     Returns an array of ordered keys for this entity.
    object?[] GetKeys();
}
public static class EntityHelper
{
    public static bool IsMultiTenant<TEntity>()
        where TEntity : IEntity
    {
        return IsMultiTenant(typeof(TEntity));
    }

    public static bool IsMultiTenant(Type type)
    {
        return false;
    }

    public static bool EntityEquals(IEntity? entity1, IEntity? entity2)
    {
        if (entity1 == null || entity2 == null)
        {
            return false;
        }

        //Same instances must be considered as equal
        if (ReferenceEquals(entity1, entity2))
        {
            return true;
        }

        //Must have a IS-A relation of types or must be same type
        var typeOfEntity1 = entity1.GetType();
        var typeOfEntity2 = entity2.GetType();
        if (!typeOfEntity1.IsAssignableFrom(typeOfEntity2) && !typeOfEntity2.IsAssignableFrom(typeOfEntity1))
        {
            return false;
        }

        if (HasDefaultKeys(entity1) && HasDefaultKeys(entity2))
        {
            return false;
        }

        var entity1Keys = entity1.GetKeys();
        var entity2Keys = entity2.GetKeys();

        if (entity1Keys.Length != entity2Keys.Length)
        {
            return false;
        }

        for (var i = 0; i < entity1Keys.Length; i++)
        {
            var entity1Key = entity1Keys[i];
            var entity2Key = entity2Keys[i];

            if (entity1Key == null)
            {
                if (entity2Key == null)
                {
                    //Both null, so considered as equals
                    continue;
                }

                //entity2Key is not null!
                return false;
            }

            if (entity2Key == null)
            {
                //entity1Key was not null!
                return false;
            }

            if (!entity1Key.Equals(entity2Key))
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsEntity([NotNull] Type type)
    {
        return typeof(IEntity).IsAssignableFrom(type);
    }

    public static Func<Type, bool> IsValueObjectPredicate = _ => false;

    public static bool IsValueObject([NotNull] Type type)
    {
        return IsValueObjectPredicate(type);
    }

    public static bool IsValueObject(object? obj)
    {
        return obj != null && IsValueObject(obj.GetType());
    }

    public static void CheckEntity([NotNull] Type type)
    {
        if (!IsEntity(type))
        {
            throw new AbpException($"Given {nameof(type)} is not an entity: {type.AssemblyQualifiedName}. It must implement {typeof(IEntity).AssemblyQualifiedName}.");
        }
    }

    public static bool IsEntityWithId([NotNull] Type type)
    {
        foreach (var interfaceType in type.GetInterfaces())
        {
            if (interfaceType.GetTypeInfo().IsGenericType &&
                interfaceType.GetGenericTypeDefinition() == typeof(IEntity<>))
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasDefaultId<TKey>(IEntity<TKey> entity)
    {
        if (EqualityComparer<TKey>.Default.Equals(entity.Id, default!))
        {
            return true;
        }

        //Workaround for EF Core since it sets int/long to min value when attaching to dbcontext
        if (typeof(TKey) == typeof(int))
        {
            return Convert.ToInt32(entity.Id) <= 0;
        }

        if (typeof(TKey) == typeof(long))
        {
            return Convert.ToInt64(entity.Id) <= 0;
        }

        return false;
    }

    private static bool IsDefaultKeyValue(object? value)
    {
        return false;
    }

    public static bool HasDefaultKeys([NotNull] IEntity entity)
    {
        foreach (var key in entity.GetKeys())
        {
            if (!IsDefaultKeyValue(key))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Tries to find the primary key type of the given entity type.
    /// May return null if given type does not implement <see cref="IEntity{TKey}"/>
    /// </summary>
    public static Type? FindPrimaryKeyType<TEntity>()
        where TEntity : IEntity
    {
        return FindPrimaryKeyType(typeof(TEntity));
    }

    /// <summary>
    /// Tries to find the primary key type of the given entity type.
    /// May return null if given type does not implement <see cref="IEntity{TKey}"/>
    /// </summary>
    public static Type? FindPrimaryKeyType([NotNull] Type entityType)
    {
        if (!typeof(IEntity).IsAssignableFrom(entityType))
        {
            throw new AbpException(
                $"Given {nameof(entityType)} is not an entity. It should implement {typeof(IEntity).AssemblyQualifiedName}!");
        }

        foreach (var interfaceType in entityType.GetTypeInfo().GetInterfaces())
        {
            if (interfaceType.GetTypeInfo().IsGenericType &&
                interfaceType.GetGenericTypeDefinition() == typeof(IEntity<>))
            {
                return interfaceType.GenericTypeArguments[0];
            }
        }

        return null;
    }

    public static Expression<Func<TEntity, bool>> CreateEqualityExpressionForId<TEntity, TKey>(TKey id)
        where TEntity : IEntity<TKey>
    {
        var lambdaParam = Expression.Parameter(typeof(TEntity));
        var leftExpression = Expression.PropertyOrField(lambdaParam, "Id");
        var idValue = Convert.ChangeType(id, typeof(TKey));
        Expression<Func<object?>> closure = () => idValue;
        var rightExpression = Expression.Convert(closure.Body, leftExpression.Type);
        var lambdaBody = Expression.Equal(leftExpression, rightExpression);
        return Expression.Lambda<Func<TEntity, bool>>(lambdaBody, lambdaParam);
    }

    public static void TrySetId<TKey>(
        IEntity<TKey> entity,
        Func<TKey> idFactory,
        bool checkForDisableIdGenerationAttribute = false)
    {
    }

    public static void TrySetTenantId(IEntity entity)
    {
    }
}
public class AbpException : Exception
{
    public AbpException()
    {
    }

    public AbpException(string? message)
        : base(message)
    {
    }

    public AbpException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    public AbpException(SerializationInfo serializationInfo, StreamingContext context)
        : base(serializationInfo, context)
    {
    }
}
public interface IEntity<TKey> : IEntity
{
    //
    // Summary:
    //     Unique identifier for this entity.
    TKey Id { get; }
}
public interface IDeletionAuditedObject : IHasDeletionTime, ISoftDelete
{
    //
    // Summary:
    //     Id of the deleter user.
    Guid? DeleterId { get; }
}
public interface IHasDeletionTime : ISoftDelete
{
    //
    // Summary:
    //     Deletion time.
    DateTime? DeletionTime { get; }
}
public interface ISoftDelete
{
    //
    // Summary:
    //     Used to mark an Entity as 'Deleted'.
    bool IsDeleted { get; }
}
public static class AbpObjectExtensions
{
    /// <summary>
    /// Used to simplify and beautify casting an object to a type.
    /// </summary>
    /// <typeparam name="T">Type to be casted</typeparam>
    /// <param name="obj">Object to cast</param>
    /// <returns>Casted object</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T As<T>(this object obj)
        where T : class
    {
        return (T)obj;
    }
}
public sealed class FlatContent
{
    public long ContentId { get; set; }
}
