SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

USE [master];
GO


IF EXISTS (SELECT * FROM sys.databases WHERE name = 'FCQ-Org')
                DROP DATABASE [FCQ-Org];
GO

CREATE DATABASE [FCQ-Org] COLLATE SQL_Latin1_General_CP1_CI_AS;
GO

-- Specify a simple recovery model to keep the log growth to a minimum.
ALTER DATABASE [FCQ-Org] SET RECOVERY SIMPLE;
GO

USE [FCQ-Org];
GO

CREATE TABLE [dbo].[Departments] (
    [Dpt] NVARCHAR (255) NOT NULL,
    PRIMARY KEY CLUSTERED ([Dpt] ASC)
);

CREATE TABLE [dbo].[Employees] (
    [Emp] NVARCHAR (255) NOT NULL,
    [Dpt] NVARCHAR (255) NOT NULL,
    [Salary] INT		NOT NULL,
    PRIMARY KEY CLUSTERED ([Dpt],[Emp] ASC)
);

CREATE TABLE [dbo].[Contacts] (
    [Dpt] NVARCHAR (255)		NOT NULL,
    [Contact] NVARCHAR (255) NOT NULL,
    [Client] INT			NOT NULL,
    PRIMARY KEY CLUSTERED ([Dpt],[Contact] ASC)
);

CREATE TABLE [dbo].[Tasks] (
    [Emp] NVARCHAR (255) NOT NULL,
    [Tsk] NVARCHAR (255) NOT NULL,
    PRIMARY KEY CLUSTERED ([Tsk],[Emp] ASC)
);
