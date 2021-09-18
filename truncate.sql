TRUNCATE TABLE Images;
TRUNCATE TABLE Vars;
GO

TRUNCATE TABLE Images;
GO
INSERT INTO [dbo].[Vars] ([Id], [Family]) VALUES (0, 1)
GO

DBCC SHRINKDATABASE(N'D:\Users\Murad\Documents\Sdb\Db\images.mdf')
GO
