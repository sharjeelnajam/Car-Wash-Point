IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Cars] (
    [Id] int NOT NULL IDENTITY,
    [LicensePlate] nvarchar(50) NOT NULL,
    [Company] nvarchar(50) NOT NULL,
    [Name] nvarchar(50) NOT NULL,
    [Year] int NOT NULL,
    [Color] nvarchar(max) NOT NULL,
    [LoyaltyPoints] int NOT NULL,
    CONSTRAINT [PK_Cars] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250809103521_init', N'9.0.8');

CREATE TABLE [Transactions] (
    [Id] int NOT NULL IDENTITY,
    [Amount] decimal(18,2) NOT NULL,
    [Date] datetime2 NOT NULL,
    [PaymentMethod] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Transactions] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250813071228_TransactionTableAdded', N'9.0.8');

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cars]') AND [c].[name] = N'Company');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Cars] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Cars] DROP COLUMN [Company];

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cars]') AND [c].[name] = N'Year');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Cars] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Cars] DROP COLUMN [Year];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250821191311_company and year removed from car ', N'9.0.8');

COMMIT;
GO

