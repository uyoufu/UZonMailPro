using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using UzonMail.ProPlugin.SQL;

namespace UzonMail.ProPlugin.Migrations.SqLite;

[DbContext(typeof(SqLiteContextPro))]
[Migration("20260727120002_addInboxVerification")]
public sealed class addInboxVerification : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            CREATE TABLE MxDomainCaches (
              Id INTEGER NOT NULL CONSTRAINT PK_MxDomainCaches PRIMARY KEY AUTOINCREMENT,
              Domain TEXT NOT NULL, AcceptsMail INTEGER NOT NULL, FailureReason TEXT NULL,
              CheckedAtUtc TEXT NOT NULL, ExpiresAtUtc TEXT NOT NULL,
              _id TEXT NOT NULL, CreateDate TEXT NOT NULL, IsDeleted INTEGER NOT NULL, IsHidden INTEGER NOT NULL);
            CREATE UNIQUE INDEX IX_MxDomainCaches_Domain ON MxDomainCaches (Domain);
            CREATE TABLE MxDomainRecords (
              Id INTEGER NOT NULL CONSTRAINT PK_MxDomainRecords PRIMARY KEY AUTOINCREMENT,
              MxDomainCacheId INTEGER NOT NULL, Host TEXT NOT NULL, Priority INTEGER NOT NULL,
              _id TEXT NOT NULL, CreateDate TEXT NOT NULL, IsDeleted INTEGER NOT NULL, IsHidden INTEGER NOT NULL,
              CONSTRAINT FK_MxDomainRecords_MxDomainCaches_MxDomainCacheId FOREIGN KEY (MxDomainCacheId) REFERENCES MxDomainCaches (Id));
            CREATE INDEX IX_MxDomainRecords_MxDomainCacheId ON MxDomainRecords (MxDomainCacheId);
            CREATE TABLE InboxVerificationSnapshots (
              Id INTEGER NOT NULL CONSTRAINT PK_InboxVerificationSnapshots PRIMARY KEY AUTOINCREMENT,
              NormalizedEmail TEXT NOT NULL, State INTEGER NOT NULL, FailureReason TEXT NULL,
              VerifiedAtUtc TEXT NOT NULL, ExpiresAtUtc TEXT NOT NULL, SyntaxDomain TEXT NULL, SyntaxUsername TEXT NULL,
              SyntaxSuggestion TEXT NULL, IsValidSyntax INTEGER NOT NULL, IsDisposable INTEGER NOT NULL,
              IsRoleAccount INTEGER NOT NULL, IsB2C INTEGER NOT NULL, MxDomainCacheId INTEGER NULL,
              CanConnectSmtp INTEGER NOT NULL, HasFullInbox INTEGER NOT NULL, IsCatchAll INTEGER NOT NULL,
              IsDeliverable INTEGER NOT NULL, IsDisabled INTEGER NOT NULL,
              _id TEXT NOT NULL, CreateDate TEXT NOT NULL, IsDeleted INTEGER NOT NULL, IsHidden INTEGER NOT NULL,
              CONSTRAINT FK_InboxVerificationSnapshots_MxDomainCaches_MxDomainCacheId FOREIGN KEY (MxDomainCacheId) REFERENCES MxDomainCaches (Id));
            CREATE UNIQUE INDEX IX_InboxVerificationSnapshots_NormalizedEmail ON InboxVerificationSnapshots (NormalizedEmail);
            CREATE INDEX IX_InboxVerificationSnapshots_MxDomainCacheId ON InboxVerificationSnapshots (MxDomainCacheId);
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql(
            "DROP TABLE InboxVerificationSnapshots; DROP TABLE MxDomainRecords; DROP TABLE MxDomainCaches;"
        );
}
