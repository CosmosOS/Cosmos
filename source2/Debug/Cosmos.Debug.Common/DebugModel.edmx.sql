
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 04/20/2013 10:32:22
-- Generated from EDMX file: C:\Data\Sources\Cosmos\source2\Debug\Cosmos.Debug.Common\DebugModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_AssemblyFileMethod]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Methods] DROP CONSTRAINT [FK_AssemblyFileMethod];
GO
IF OBJECT_ID(N'[dbo].[FK_DocumentMethod]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Methods] DROP CONSTRAINT [FK_DocumentMethod];
GO
IF OBJECT_ID(N'[dbo].[FK_LabelMethod]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Methods] DROP CONSTRAINT [FK_LabelMethod];
GO
IF OBJECT_ID(N'[dbo].[FK_LabelMethodEnd]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Methods] DROP CONSTRAINT [FK_LabelMethodEnd];
GO
IF OBJECT_ID(N'[dbo].[FK_MethodIlOpMethod]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[MethodIlOps] DROP CONSTRAINT [FK_MethodIlOpMethod];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FIELD_INFO]', 'U') IS NOT NULL
    DROP TABLE [dbo].[FIELD_INFO];
GO
IF OBJECT_ID(N'[dbo].[FIELD_MAPPING]', 'U') IS NOT NULL
    DROP TABLE [dbo].[FIELD_MAPPING];
GO
IF OBJECT_ID(N'[dbo].[Labels]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Labels];
GO
IF OBJECT_ID(N'[dbo].[LOCAL_ARGUMENT_INFO]', 'U') IS NOT NULL
    DROP TABLE [dbo].[LOCAL_ARGUMENT_INFO];
GO
IF OBJECT_ID(N'[dbo].[MethodIlOps]', 'U') IS NOT NULL
    DROP TABLE [dbo].[MethodIlOps];
GO
IF OBJECT_ID(N'[dbo].[AssemblyFiles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[AssemblyFiles];
GO
IF OBJECT_ID(N'[dbo].[Methods]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Methods];
GO
IF OBJECT_ID(N'[dbo].[Documents]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Documents];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'FIELD_INFO'
CREATE TABLE [dbo].[FIELD_INFO] (
    [TYPE] nvarchar(512)  NOT NULL,
    [OFFSET] int  NOT NULL,
    [NAME] nvarchar(512)  NOT NULL,
    [ID] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'FIELD_MAPPING'
CREATE TABLE [dbo].[FIELD_MAPPING] (
    [TYPE_NAME] nvarchar(512)  NOT NULL,
    [FIELD_NAME] nvarchar(512)  NOT NULL,
    [ID] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'Labels'
CREATE TABLE [dbo].[Labels] (
    [ID] uniqueidentifier  NOT NULL,
    [Name] nvarchar(256)  NOT NULL,
    [Address] integer  NOT NULL
);
GO

-- Creating table 'LOCAL_ARGUMENT_INFO'
CREATE TABLE [dbo].[LOCAL_ARGUMENT_INFO] (
    [METHODLABELNAME] nvarchar(256)  NOT NULL,
    [IsArgument] bit  NOT NULL,
    [INDEXINMETHOD] int  NOT NULL,
    [OFFSET] int  NOT NULL,
    [NAME] nvarchar(64)  NOT NULL,
    [TYPENAME] nvarchar(256)  NOT NULL,
    [ID] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'MethodIlOps'
CREATE TABLE [dbo].[MethodIlOps] (
    [ID] uniqueidentifier  NOT NULL,
    [LabelName] nvarchar(256)  NOT NULL,
    [StackDiff] int  NOT NULL,
    [IlOffset] int  NOT NULL,
    [MethodID] uniqueidentifier  NULL
);
GO

-- Creating table 'AssemblyFiles'
CREATE TABLE [dbo].[AssemblyFiles] (
    [ID] uniqueidentifier  NOT NULL,
    [Pathname] nvarchar(256)  NOT NULL
);
GO

-- Creating table 'Methods'
CREATE TABLE [dbo].[Methods] (
    [ID] uniqueidentifier  NOT NULL,
    [TypeToken] int  NOT NULL,
    [MethodToken] int  NOT NULL,
    [AssemblyFileID] uniqueidentifier  NOT NULL,
    [LineColStart] integer  NOT NULL,
    [LineColEnd] integer  NOT NULL,
    [DocumentID] uniqueidentifier  NOT NULL,
    [LabelStartID] uniqueidentifier  NULL,
    [LabelEndID] uniqueidentifier  NULL,
    [LabelCall] nvarchar(256)  NOT NULL
);
GO

-- Creating table 'Documents'
CREATE TABLE [dbo].[Documents] (
    [ID] uniqueidentifier  NOT NULL,
    [Pathname] nvarchar(256)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [ID] in table 'FIELD_INFO'
ALTER TABLE [dbo].[FIELD_INFO]
ADD CONSTRAINT [PK_FIELD_INFO]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'FIELD_MAPPING'
ALTER TABLE [dbo].[FIELD_MAPPING]
ADD CONSTRAINT [PK_FIELD_MAPPING]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Labels'
ALTER TABLE [dbo].[Labels]
ADD CONSTRAINT [PK_Labels]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'LOCAL_ARGUMENT_INFO'
ALTER TABLE [dbo].[LOCAL_ARGUMENT_INFO]
ADD CONSTRAINT [PK_LOCAL_ARGUMENT_INFO]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'MethodIlOps'
ALTER TABLE [dbo].[MethodIlOps]
ADD CONSTRAINT [PK_MethodIlOps]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'AssemblyFiles'
ALTER TABLE [dbo].[AssemblyFiles]
ADD CONSTRAINT [PK_AssemblyFiles]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Methods'
ALTER TABLE [dbo].[Methods]
ADD CONSTRAINT [PK_Methods]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Documents'
ALTER TABLE [dbo].[Documents]
ADD CONSTRAINT [PK_Documents]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [AssemblyFileID] in table 'Methods'
ALTER TABLE [dbo].[Methods]
ADD CONSTRAINT [FK_AssemblyFileMethod]
    FOREIGN KEY ([AssemblyFileID])
    REFERENCES [dbo].[AssemblyFiles]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_AssemblyFileMethod'
CREATE INDEX [IX_FK_AssemblyFileMethod]
ON [dbo].[Methods]
    ([AssemblyFileID]);
GO

-- Creating foreign key on [DocumentID] in table 'Methods'
ALTER TABLE [dbo].[Methods]
ADD CONSTRAINT [FK_DocumentMethod]
    FOREIGN KEY ([DocumentID])
    REFERENCES [dbo].[Documents]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_DocumentMethod'
CREATE INDEX [IX_FK_DocumentMethod]
ON [dbo].[Methods]
    ([DocumentID]);
GO

-- Creating foreign key on [LabelStartID] in table 'Methods'
ALTER TABLE [dbo].[Methods]
ADD CONSTRAINT [FK_LabelMethod]
    FOREIGN KEY ([LabelStartID])
    REFERENCES [dbo].[Labels]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_LabelMethod'
CREATE INDEX [IX_FK_LabelMethod]
ON [dbo].[Methods]
    ([LabelStartID]);
GO

-- Creating foreign key on [LabelEndID] in table 'Methods'
ALTER TABLE [dbo].[Methods]
ADD CONSTRAINT [FK_LabelMethodEnd]
    FOREIGN KEY ([LabelEndID])
    REFERENCES [dbo].[Labels]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_LabelMethodEnd'
CREATE INDEX [IX_FK_LabelMethodEnd]
ON [dbo].[Methods]
    ([LabelEndID]);
GO

-- Creating foreign key on [MethodID] in table 'MethodIlOps'
ALTER TABLE [dbo].[MethodIlOps]
ADD CONSTRAINT [FK_MethodIlOpMethod]
    FOREIGN KEY ([MethodID])
    REFERENCES [dbo].[Methods]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_MethodIlOpMethod'
CREATE INDEX [IX_FK_MethodIlOpMethod]
ON [dbo].[MethodIlOps]
    ([MethodID]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------