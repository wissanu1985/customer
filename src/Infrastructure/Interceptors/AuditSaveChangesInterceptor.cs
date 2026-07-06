using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Common;
using Domain.Common.Attributes;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Interceptors;

// Auto-writes Audit rows on SaveChanges. Thai-safe via UnsafeRelaxedJsonEscaping
// (default System.Text.Json escapes non-ASCII as \uXXXX which some viewers render as ???).
//
// Storage model: one Audit row per change event with a single `Values` snapshot.
// Timeline is reconstructable via EntityId + TableName + Timestamp — no need for
// separate Old/New columns.
public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private static readonly HashSet<string> MetadataProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(AuditableEntity.Id),
        nameof(AuditableEntity.Seq),
        nameof(AuditableEntity.CreatedDate),
        nameof(AuditableEntity.CreatedBy),
        nameof(AuditableEntity.UpdatedDate),
        nameof(AuditableEntity.UpdatedBy),
        nameof(AuditableEntity.DeletedDate),
        nameof(AuditableEntity.DeletedBy),
        nameof(AuditableEntity.IsDeleted),
        "RowVersion"
    };

    // UnsafeRelaxedJsonEscaping writes Thai (and all non-ASCII) as literal chars,
    // not \uXXXX escapes. Safe here because output goes to NVARCHAR column, not HTML.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };

    private readonly ICurrentUserProvider _currentUser;

    public AuditSaveChangesInterceptor(ICurrentUserProvider currentUser)
    {
        _currentUser = currentUser;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        BuildAudits(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        BuildAudits(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void BuildAudits(DbContext? context)
    {
        if (context is null) return;

        var userName = _currentUser.UserName;
        var now = DateTime.UtcNow;
        // Materialize first — calling context.Add(audit) below mutates the ChangeTracker,
        // which would throw "Collection was modified" if we enumerated lazily.
        var entries = context.ChangeTracker.Entries<AuditableEntity>()
            .Where(e => e.Entity is not Audit
                        && e.Metadata.IsOwned() == false
                        && e.Entity.GetType().GetCustomAttributes(typeof(IgnoreAuditAttribute), inherit: true).Length == 0
                        && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            var audit = BuildAuditEntry(entry, userName, now);
            if (audit is not null)
                context.Add(audit);
        }
    }

    private static Audit? BuildAuditEntry(EntityEntry<AuditableEntity> entry, string userName, DateTime now)
    {
        // Insert/Update -> snapshot of current values.
        // Delete (soft or hard) -> snapshot of original values (last known state).
        var (action, values) = entry.State switch
        {
            EntityState.Added    => ("Insert", SerializeCurrent(entry)),
            EntityState.Modified => IsSoftDelete(entry) ? ("Delete", SerializeOriginal(entry)) : ("Update", SerializeCurrent(entry)),
            EntityState.Deleted  => ("Delete", SerializeOriginal(entry)),
            _ => (null, (string?)null)
        };

        if (action is null) return null;

        return new Audit
        {
            Id = Guid.NewGuid(),
            EntityType = entry.Entity.GetType().Name,
            EntityId = entry.Entity.Id.ToString(),
            Action = action,
            Values = values,
            ChangedBy = userName,
            TableName = entry.Metadata.GetTableName(),
            AuditType = "Entity",
            Timestamp = now,
            CreatedDate = now,
            CreatedBy = userName
        };
    }

    // Soft-delete = IsDeleted flipped false->true (BaseRepository.Delete sets IsDeleted=true + Update).
    private static bool IsSoftDelete(EntityEntry<AuditableEntity> entry)
    {
        var isDeletedProp = entry.Property(nameof(AuditableEntity.IsDeleted));
        return isDeletedProp.OriginalValue is bool orig
               && isDeletedProp.CurrentValue is bool cur
               && !orig && cur;
    }

    private static string? SerializeCurrent(EntityEntry<AuditableEntity> entry)
    {
        var dict = entry.CurrentValues.Properties
            .Where(p => !MetadataProperties.Contains(p.Name))
            .ToDictionary(p => p.Name, p => entry.CurrentValues[p]);
        return dict.Count == 0 ? null : JsonSerializer.Serialize(dict, JsonOptions);
    }

    private static string? SerializeOriginal(EntityEntry<AuditableEntity> entry)
    {
        var dict = entry.OriginalValues.Properties
            .Where(p => !MetadataProperties.Contains(p.Name))
            .ToDictionary(p => p.Name, p => entry.OriginalValues[p]);
        return dict.Count == 0 ? null : JsonSerializer.Serialize(dict, JsonOptions);
    }
}
