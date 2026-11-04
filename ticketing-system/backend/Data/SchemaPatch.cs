using Microsoft.EntityFrameworkCore;

namespace EventsLab.Api.Data;

public static class SchemaPatch
{
    public static Task EnsureAuditLogAsync(AppDbContext db)
    {
        const string sql = """
            IF OBJECT_ID(N'[dbo].[AuditLogs]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[AuditLogs] (
                    [Id] bigint IDENTITY(1,1) NOT NULL CONSTRAINT [PK_AuditLogs] PRIMARY KEY,
                    [OccurredAt] datetime2 NOT NULL,
                    [UserId] int NULL,
                    [Username] nvarchar(80) NOT NULL,
                    [Action] nvarchar(80) NOT NULL,
                    [EntityType] nvarchar(80) NOT NULL,
                    [EntityId] nvarchar(40) NULL,
                    [Summary] nvarchar(400) NOT NULL,
                    [Details] nvarchar(2000) NULL
                );
                CREATE INDEX [IX_AuditLogs_OccurredAt] ON [dbo].[AuditLogs] ([OccurredAt]);
                CREATE INDEX [IX_AuditLogs_Action] ON [dbo].[AuditLogs] ([Action]);
            END
            """;

        return db.Database.ExecuteSqlRawAsync(sql);
    }
}
