-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'FIELD_INFO'
CREATE TABLE [FIELD_INFOS] (
    [TYPE] nvarchar(512)  NOT NULL,
    [OFFSET] int  NOT NULL,
    [NAME] nvarchar(512)  NOT NULL,
    [ID] integer primary key
);


-- Creating table 'FIELD_MAPPING'
CREATE TABLE [FIELD_MAPPINGS] (
    [TYPE_NAME] nvarchar(512)  NOT NULL,
    [FIELD_NAME] nvarchar(512)  NOT NULL,
    [ID] integer  primary key
);


-- Creating table 'Labels'
CREATE TABLE [Labels] (
    [ID] integer  NOT NULL primary key,
    [Name] nvarchar(256)  NOT NULL,
    [Address] integer not null
);


-- Creating table 'LOCAL_ARGUMENT_INFO'
CREATE TABLE [LOCAL_ARGUMENT_INFOS] (
    [METHODLABELNAME] nvarchar(256)  NOT NULL,
    [IsArgument] bit  NOT NULL,
    [INDEXINMETHOD] int  NOT NULL,
    [OFFSET] int  NOT NULL,
    [NAME] nvarchar(64)  NOT NULL,
    [TYPENAME] nvarchar(256)  NOT NULL,
    [ID] integer primary key
);


-- Creating table 'MethodIlOps'
CREATE TABLE [MethodIlOps] (
    [ID] integer primary key,
    [LabelName] nvarchar(256)  NOT NULL,
    [StackDiff] int  NOT NULL,
    [IlOffset] int  NOT NULL
);


-- Creating table 'AssemblyFiles'
CREATE TABLE [AssemblyFiles] (
    [ID] integer primary key,
    [Pathname] nvarchar(256)  NOT NULL
);


-- Creating table 'Methods'
CREATE TABLE [Methods] (
    [ID] integer primary key,
    [TypeToken] int  NOT NULL,
    [MethodToken] int  NOT NULL,
    [LineColStart] int  NOT NULL,
    [LineColEnd] int  NOT NULL,
    [LabelCall] nvarchar(256)  NOT NULL
);


-- Creating table 'Documents'
CREATE TABLE [Documents] (
    [ID] integer primary key,
    [Pathname] nvarchar(256)  NOT NULL
);

-- Creating table 'INT3Labels'
CREATE TABLE [INT3Labels] (
	[LabelName] nvarchar(256) NOT NULL,
	[MethodID] integer NOT NULL,
	[LeaveAsINT3] bit NOT NULL
);

alter table methods
  add column AssemblyFileID integer null;-- references AssemblyFiles(ID);
alter table methods
  add column DocumentID integer null;-- references Documents(ID);
alter table methods
  add column LabelStartID integer null;-- references Labels(ID);
alter table methods
  add column LabelEndID integer null;-- references Labels(ID);

alter table MethodIlOps
  add column MethodID integer null;-- references Methods(ID);

