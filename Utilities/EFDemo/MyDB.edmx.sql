
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 03/08/2018 15:37:23
-- Generated from EDMX file: C:\codes\github\.net-utilities\Utilities\EFDemo\MyDB.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [DemoDB];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Person]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Person];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Persons'
CREATE TABLE [dbo].[Persons] (
    [Id] uniqueidentifier  NOT NULL,
    [Name] nvarchar(256)  NOT NULL,
    [Age] nvarchar(max)  NOT NULL,
    [Address] nvarchar(max)  NULL,
    [TeamId] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'Teams'
CREATE TABLE [dbo].[Teams] (
    [Id] uniqueidentifier  NOT NULL,
    [Name] nvarchar(256)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Persons'
ALTER TABLE [dbo].[Persons]
ADD CONSTRAINT [PK_Persons]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Teams'
ALTER TABLE [dbo].[Teams]
ADD CONSTRAINT [PK_Teams]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [TeamId] in table 'Persons'
ALTER TABLE [dbo].[Persons]
ADD CONSTRAINT [FK_PersonTeam]
    FOREIGN KEY ([TeamId])
    REFERENCES [dbo].[Teams]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_PersonTeam'
CREATE INDEX [IX_FK_PersonTeam]
ON [dbo].[Persons]
    ([TeamId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------