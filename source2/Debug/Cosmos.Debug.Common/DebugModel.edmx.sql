
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 08/05/2012 20:29:02
-- Generated from EDMX file: D:\source\Cosmos\source2\Debug\Cosmos.Debug.Common\DebugModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [Guess];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_AssemblyFileMethod]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Methods] DROP CONSTRAINT [FK_AssemblyFileMethod];
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
IF OBJECT_ID(N'[dbo].[MLSYMBOLs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[MLSYMBOLs];
GO
IF OBJECT_ID(N'[dbo].[AssemblyFiles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[AssemblyFiles];
GO
IF OBJECT_ID(N'[dbo].[Methods]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Methods];
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
    [Name] nvarchar(512)  NOT NULL,
    [Address] bigint  NOT NULL
);
GO

-- Creating table 'LOCAL_ARGUMENT_INFO'
CREATE TABLE [dbo].[LOCAL_ARGUMENT_INFO] (
    [METHODLABELNAME] nvarchar(512)  NOT NULL,
    [IsArgument] bit  NOT NULL,
    [INDEXINMETHOD] int  NOT NULL,
    [OFFSET] int  NOT NULL,
    [NAME] nvarchar(64)  NOT NULL,
    [TYPENAME] nvarchar(512)  NOT NULL,
    [ID] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'MLSYMBOLs'
CREATE TABLE [dbo].[MLSYMBOLs] (
    [LABELNAME] nvarchar(512)  NOT NULL,
    [STACKDIFF] int  NOT NULL,
    [ILASMFILE] nvarchar(256)  NOT NULL,
    [TYPETOKEN] int  NOT NULL,
    [METHODTOKEN] int  NOT NULL,
    [ILOFFSET] int  NOT NULL,
    [METHODNAME] nvarchar(512)  NOT NULL,
    [ID] uniqueidentifier  NOT NULL
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
    [LabelName] nvarchar(512)  NOT NULL,
    [AddressStartFuture] bigint  NULL,
    [AddressEndFuture] bigint  NULL,
    [AssemblyFileID] uniqueidentifier  NOT NULL
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

-- Creating primary key on [ID] in table 'MLSYMBOLs'
ALTER TABLE [dbo].[MLSYMBOLs]
ADD CONSTRAINT [PK_MLSYMBOLs]
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

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------