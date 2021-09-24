TRUNCATE TABLE Images;
TRUNCATE TABLE Vars;
GO

TRUNCATE TABLE Images;
GO
INSERT INTO [dbo].[Vars] ([Id]) VALUES (0)
GO

DBCC SHRINKDATABASE(N'D:\Users\Murad\Documents\Sdb\Db\images.mdf')
GO
