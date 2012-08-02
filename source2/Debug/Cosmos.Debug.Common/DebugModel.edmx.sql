
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 08/01/2012 23:11:07
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
IF OBJECT_ID(N'[dbo].[Methods]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Methods];
GO
IF OBJECT_ID(N'[dbo].[MLSYMBOLs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[MLSYMBOLs];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'FIELD_INFO'
CREATE TABLE [dbo].[FIELD_INFO] (
    [TYPE] nvarchar(4000)  NOT NULL,
    [OFFSET] int  NOT NULL,
    [NAME] nvarchar(4000)  NOT NULL,
    [ID] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'FIELD_MAPPING'
CREATE TABLE [dbo].[FIELD_MAPPING] (
    [TYPE_NAME] nvarchar(4000)  NOT NULL,
    [FIELD_NAME] nvarchar(4000)  NOT NULL,
    [ID] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'Labels'
CREATE TABLE [dbo].[Labels] (
    [LABELNAME] nvarchar(4000)  NOT NULL,
    [ADDRESS] bigint  NOT NULL,
    [ID] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'LOCAL_ARGUMENT_INFO'
CREATE TABLE [dbo].[LOCAL_ARGUMENT_INFO] (
    [METHODLABELNAME] nvarchar(255)  NOT NULL,
    [ISARGUMENT] smallint  NOT NULL,
    [INDEXINMETHOD] int  NOT NULL,
    [OFFSET] int  NOT NULL,
    [NAME] nvarchar(255)  NOT NULL,
    [TYPENAME] nvarchar(4000)  NOT NULL,
    [ID] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'Methods'
CREATE TABLE [dbo].[Methods] (
    [MethodId] int  NOT NULL,
    [LabelPrefix] nvarchar(255)  NOT NULL,
    [ID] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'MLSYMBOLs'
CREATE TABLE [dbo].[MLSYMBOLs] (
    [LABELNAME] nvarchar(255)  NOT NULL,
    [STACKDIFF] int  NOT NULL,
    [ILASMFILE] nvarchar(255)  NOT NULL,
    [TYPETOKEN] int  NOT NULL,
    [METHODTOKEN] int  NOT NULL,
    [ILOFFSET] int  NOT NULL,
    [METHODNAME] nvarchar(255)  NOT NULL,
    [ID] uniqueidentifier  NOT NULL
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

-- Creating primary key on [ID] in table 'Methods'
ALTER TABLE [dbo].[Methods]
ADD CONSTRAINT [PK_Methods]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'MLSYMBOLs'
ALTER TABLE [dbo].[MLSYMBOLs]
ADD CONSTRAINT [PK_MLSYMBOLs]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------